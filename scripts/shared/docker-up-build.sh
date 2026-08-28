#!/usr/bin/env bash
set -euo pipefail

SERVICE="$1"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

cd "$REPO_ROOT"

docker compose -f ./docker-compose.development.yml --env-file .env.local up -d --build "$SERVICE"
