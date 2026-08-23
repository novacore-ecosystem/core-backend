#!/usr/bin/env bash
# Rebuilds the Chat image and (re)starts the service against the local Docker Compose
# stack.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
COMPOSE_FILE="$REPO_ROOT/docker-compose.local.yml"
ENV_FILE="$REPO_ROOT/.env.local"
SERVICE="chat-api"

COMPOSE_NETWORK="$(grep -E '^COMPOSE_NETWORK=' "$ENV_FILE" | cut -d= -f2)"
docker network inspect "$COMPOSE_NETWORK" >/dev/null 2>&1 || docker network create "$COMPOSE_NETWORK"

docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" up -d --build "$SERVICE"
