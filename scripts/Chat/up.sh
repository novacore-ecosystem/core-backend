#!/usr/bin/env bash
# Starts the Chat service (and its dependencies) against the local Docker Compose
# stack, without rebuilding images.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
COMPOSE_FILE="$REPO_ROOT/docker-compose.local.yml"
ENV_FILE="$REPO_ROOT/.env.local"
SERVICE="chat-api"

COMPOSE_NETWORK="$(grep -E '^COMPOSE_NETWORK=' "$ENV_FILE" | cut -d= -f2)"
# docker-compose.local.yml declares its network external:true - it must exist before
# 'up' or compose fails immediately; creating it here is a no-op once it already does.
docker network inspect "$COMPOSE_NETWORK" >/dev/null 2>&1 || docker network create "$COMPOSE_NETWORK"

docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" up -d "$SERVICE"
