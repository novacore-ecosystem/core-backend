#!/usr/bin/env bash
# Stops and removes the User service container only. Deliberately NOT 'docker compose
# down' - that command is project-wide and would tear down every other service sharing
# docker-compose.local.yml (Auth/User/Audit/Notification/Content/Chat + shared
# pg/mongo/redis/kafka/seq), which this script must never touch.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
COMPOSE_FILE="$REPO_ROOT/docker-compose.local.yml"
ENV_FILE="$REPO_ROOT/.env.local"
SERVICE="user-api"

docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" stop "$SERVICE"
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" rm -f "$SERVICE"
