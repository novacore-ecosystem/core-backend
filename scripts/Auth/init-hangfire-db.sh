#!/usr/bin/env bash
# Creates the Auth service's Hangfire storage database in the shared 'pg'
# container, if it doesn't already exist. Idempotent - safe to re-run.
set -euo pipefail

DB_NAME="auth_hangfire_db"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

cd "$REPO_ROOT"

EXISTS="$(docker-compose exec -T pg psql -U postgres -tAc "SELECT 1 FROM pg_database WHERE datname = '$DB_NAME'")"
if [ "$EXISTS" = "1" ]; then
    echo "Database '$DB_NAME' already exists."
else
    docker-compose exec -T pg psql -U postgres -c "CREATE DATABASE $DB_NAME"
    echo "Created database '$DB_NAME'."
fi
