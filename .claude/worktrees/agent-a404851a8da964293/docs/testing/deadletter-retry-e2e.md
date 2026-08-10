# Dead Letter Queue retry - manual/scripted E2E scenario

Companion to `scripts/test-deadletter-retry-e2e.sh`. Covers the Dead Letter Queue management
APIs (search/detail/retry) added on top of the generic Inbox infrastructure - see
`docs/tasks/2026-07-27/Task9_dead-letter-handling-db-flag-only.md` for the prior state this
builds on.

## Why a script, not a Testcontainers project

`docs/testing/TestingArchitecture.md` explicitly defers Testcontainers-based integration test
projects (Phase 5) as out of scope for now, and no service currently has a Kafka-backed
integration harness. Building one from scratch for this single scenario would be new
infrastructure, not "keeping it simple." This script exercises the real running docker-compose
stack instead - manual/CI-optional, not part of the automated test suite.

## What it proves

Run against **Notification** (smallest single-consumer service: one `NotificationTriggerConsumer`,
no gRPC, no Elasticsearch write dependency in its consume path).

- **Part A** publishes a genuinely malformed payload to the `userprofilecreated` topic.
  `NotificationTriggerConsumer.HandleAsync`'s `JsonSerializer.Deserialize` throws every time, so
  the pre-existing Inbox retry/backoff logic (`InboxAttemptExecutor`, unchanged by this task)
  drives the row to `DeadLetter` for real. This proves the new `POST /deadletters/search` and
  `GET /deadletters/{id}` APIs see a row that dead-lettered through the actual pipeline.
- **Part B** seeds one Mongo document directly with `Status: DeadLetter` and a *valid*
  `UserProfileCreatedIntegrationEvent` payload, standing in for "a message that failed while some
  downstream dependency was unavailable, which has since recovered" (a malformed payload from
  Part A can never succeed on replay, since retry republishes the exact stored bytes - it isn't a
  vehicle for testing the success path). Calling `POST /deadletters/{id}/retry` must:
  1. Atomically flip the row `DeadLetter -> Retrying` (`IInboxStore.RequeueDeadLetterAsync`).
  2. Republish it through `IOutboxPublisher` onto the real `userprofilecreated` topic - not
     re-invoke `NotificationTriggerConsumer.HandleAsync` in-process.
  3. Have the real consumer pick it up, process it successfully, and the row reach `Processed`
     with a `Succeeded` entry appended to its retry history.

## Running it

```bash
docker compose up -d
# optional, for a fast Part A instead of the default ~5 min backoff:
Inbox__Retry__MaxRetryCount=2 Inbox__Retry__InitialRetryDelay=00:00:02 docker compose up -d notification-api

export ADMIN_TOKEN=<a valid admin JWT>
./scripts/test-deadletter-retry-e2e.sh
```

Requires `jq`, `python3`, and `mongosh` (available inside the `mongo` container, invoked via
`docker compose exec`) on the machine running the script.

## Scope note

A dedicated "lightweight third-party consumer" container (as originally sketched) was not built -
judged heavier than necessary once it became clear that simply stopping `notification-api`
doesn't itself produce a `DeadLetter` row (Kafka just retains the unconsumed message; nothing
attempts and fails processing). Driving `NotificationTriggerConsumer` directly with a payload
that's guaranteed to fail is both simpler and a more faithful trigger of the real failure path.

This script has not been run against a live stack as part of this change (bringing up the full
compose stack - Kafka, Mongo, Postgres, Elasticsearch, APM - was out of scope for this pass);
run it locally per the steps above to verify before relying on it in a demo or CI job.
