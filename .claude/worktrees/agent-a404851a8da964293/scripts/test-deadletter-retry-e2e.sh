#!/usr/bin/env bash
# End-to-end smoke test for the Dead Letter Queue management API, against the Notification
# service (the smallest single-consumer service; see docs/testing/deadletter-retry-e2e.md).
#
# Scope note: building a dedicated lightweight "third-party consumer" container was judged
# heavier than necessary. Instead this script drives NotificationTriggerConsumer directly and
# splits the scenario in two, each demonstrating a different half of the feature with a fully
# deterministic trigger (no manual timing/log-watching):
#
#   Part A - real failure -> DeadLetter: publishes a genuinely malformed payload to the
#   "userprofilecreated" topic. NotificationTriggerConsumer.HandleAsync's JSON.Deserialize throws
#   every time, so the existing (pre-this-task) Inbox retry/backoff logic in InboxAttemptExecutor
#   drives the row to DeadLetter after Inbox:Retry:MaxRetryCount attempts - proving the new
#   Search/Get APIs see a row that dead-lettered through the real pipeline, not a fixture.
#
#   Part B - retry -> success: a malformed payload can never succeed on replay (retry republishes
#   the exact stored bytes), so this half seeds one Mongo document directly with a VALID
#   UserProfileCreatedIntegrationEvent payload and Status=DeadLetter, representing "a message
#   that failed while some downstream dependency was unavailable, which has since recovered".
#   Calling POST /deadletters/{id}/retry against it must republish through Kafka and the row must
#   reach Processed once NotificationTriggerConsumer reprocesses it for real.
#
# Requires: the full docker-compose stack running (`docker compose up -d`), notification-api and
# user-api reachable, jq and mongosh available on PATH.

set -euo pipefail

COMPOSE_FILE="${COMPOSE_FILE:-docker-compose.yml}"
NOTIFICATION_URL="${NOTIFICATION_URL:-http://localhost:5108}"
USER_URL="${USER_URL:-http://localhost:5100}"
ADMIN_TOKEN="${ADMIN_TOKEN:?Set ADMIN_TOKEN to a valid admin JWT before running this script}"
MONGO_CONTAINER="${MONGO_CONTAINER:-novacore-mongo}"
MONGO_DB="${MONGO_DB:-notification_db}"

auth_header=(-H "Authorization: Bearer ${ADMIN_TOKEN}")
json_header=(-H "Content-Type: application/json")

echo "== Part A: malformed payload -> real DeadLetter via NotificationTriggerConsumer =="

# Lower the retry budget for this run so Part A doesn't take the default ~5 minutes of backoff.
# Requires notification-api started with these overrides, e.g.:
#   Inbox__Retry__MaxRetryCount=2 Inbox__Retry__InitialRetryDelay=00:00:02 docker compose up -d notification-api
echo "NOTE: for a fast run, restart notification-api with Inbox__Retry__MaxRetryCount=2 and"
echo "      Inbox__Retry__InitialRetryDelay=00:00:02 first. Falling back to configured defaults otherwise."

MESSAGE_ID_A=$(python3 -c "import uuid; print(uuid.uuid4())")
docker compose -f "$COMPOSE_FILE" exec -T kafka kafka-console-producer \
  --bootstrap-server localhost:9092 --topic userprofilecreated \
  --property "parse.key=false" <<EOF
{this is not valid json, deliberately - MESSAGE_ID_A=${MESSAGE_ID_A}}
EOF

echo "Published malformed payload. Polling /deadletters for a DeadLetter row on Consumer=NotificationTriggerConsumer, Topic=userprofilecreated..."
for i in $(seq 1 60); do
  RESULT=$(curl -s -X POST "${NOTIFICATION_URL}/deadletters/search" "${auth_header[@]}" "${json_header[@]}" \
    -d '{"filters":[{"field":"topic","operator":"eq","value":"userprofilecreated"}],"page":1,"pageSize":5,"sorts":[{"field":"createdAt","direction":"desc"}]}')
  COUNT=$(echo "$RESULT" | jq '.data.items | length')
  if [ "$COUNT" -gt 0 ]; then
    DEAD_LETTER_ID_A=$(echo "$RESULT" | jq -r '.data.items[0].id')
    echo "Found DeadLetter row: $DEAD_LETTER_ID_A"
    break
  fi
  sleep 5
done

if [ -z "${DEAD_LETTER_ID_A:-}" ]; then
  echo "FAILED: no DeadLetter row appeared within the poll window - check Inbox:Retry settings/logs."
  exit 1
fi

echo "Fetching detail for $DEAD_LETTER_ID_A ..."
curl -s "${NOTIFICATION_URL}/deadletters/${DEAD_LETTER_ID_A}" "${auth_header[@]}" | jq '.data | {id, status, retryCount, lastError}'

echo
echo "== Part B: seed a recoverable DeadLetter row, then retry it to success =="

MESSAGE_ID_B=$(python3 -c "import uuid; print(uuid.uuid4())")
USER_ID_B=$(python3 -c "import uuid; print(uuid.uuid4())")
NOW_ISO=$(date -u +"%Y-%m-%dT%H:%M:%S.000Z")
PAYLOAD_B=$(cat <<EOF
{"UserId":"${USER_ID_B}","Email":"e2e-test@example.com","UserName":"e2e-test","FirstName":"E2E","MiddleName":"","LastName":"Test","CorrelationId":"${MESSAGE_ID_B}","Roles":[],"TempPassword":"","EventType":"UserProfileCreatedIntegrationEvent","PublishedAt":"${NOW_ISO}"}
EOF
)
HEADERS_B="{\"event-type\":\"UserProfileCreatedIntegrationEvent\",\"correlation-id\":\"${MESSAGE_ID_B}\",\"message-id\":\"${MESSAGE_ID_B}\"}"

docker compose -f "$COMPOSE_FILE" exec -T mongo mongosh "$MONGO_DB" --quiet --eval "
db.inbox_messages.insertOne({
  _id: '${MESSAGE_ID_B}',
  MessageId: '${MESSAGE_ID_B}',
  ConsumerName: 'NotificationTriggerConsumer',
  Topic: 'userprofilecreated',
  Payload: '$(echo "$PAYLOAD_B" | sed "s/'/\\\\'/g")',
  HeadersJson: '$(echo "$HEADERS_B" | sed "s/'/\\\\'/g")',
  Status: 'DeadLetter',
  RetryCount: 5,
  CreatedAt: new Date(),
  ProcessedAt: null,
  NextRetryAt: null,
  LastRetryAt: new Date(),
  LastError: 'Simulated: downstream dependency was unavailable'
});
"

echo "Seeded DeadLetter row for MessageId=${MESSAGE_ID_B}. Looking it up via the search API..."
RESULT_B=$(curl -s -X POST "${NOTIFICATION_URL}/deadletters/search" "${auth_header[@]}" "${json_header[@]}" \
  -d "{\"keyword\":\"${MESSAGE_ID_B}\",\"page\":1,\"pageSize\":5}")
DEAD_LETTER_ID_B=$(echo "$RESULT_B" | jq -r '.data.items[0].id // empty')
if [ -z "$DEAD_LETTER_ID_B" ]; then
  echo "FAILED: seeded row not found via search API."
  exit 1
fi
echo "Found: $DEAD_LETTER_ID_B"

echo "Calling POST /deadletters/${DEAD_LETTER_ID_B}/retry ..."
RETRY_RESULT=$(curl -s -X POST "${NOTIFICATION_URL}/deadletters/${DEAD_LETTER_ID_B}/retry" "${auth_header[@]}" -H "Idempotency-Key: $(python3 -c 'import uuid; print(uuid.uuid4())')")
echo "$RETRY_RESULT" | jq .

echo "Polling for the row to leave DeadLetter status (Succeeded expected)..."
for i in $(seq 1 30); do
  DETAIL=$(curl -s "${NOTIFICATION_URL}/deadletters/${DEAD_LETTER_ID_B}" "${auth_header[@]}")
  STATUS=$(echo "$DETAIL" | jq -r '.data.status // empty')
  if [ "$STATUS" != "DeadLetter" ] && [ -n "$STATUS" ]; then
    echo "Row status is now: $STATUS"
    echo "$DETAIL" | jq '.data.retryHistory'
    if [ "$STATUS" = "Processed" ]; then
      echo "PASS: retry succeeded end-to-end."
      exit 0
    else
      echo "FAILED: row left DeadLetter but did not reach Processed (status=$STATUS)."
      exit 1
    fi
  fi
  sleep 3
done

echo "FAILED: row never left DeadLetter status within the poll window."
exit 1
