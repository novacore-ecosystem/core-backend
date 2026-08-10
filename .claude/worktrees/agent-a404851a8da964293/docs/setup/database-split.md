# Setup: Splitting a Service to Its Own Database

**Scope:** how to move a service off the shared `pg` container onto a dedicated Postgres container. Condensed from the former `setup/DATABASE_SPLIT_GUIDE.md` (archived).

## Current state

All services share one `pg` container (`auth_db`, `user_db`, plus reserved names for not-yet-built services). This is intentional for development (saves memory) — split only when you have a real reason (staging isolation, or production).

## Split pattern (5 steps, using User Service as the example)

1. **`docker-compose.yml`** — add a `pg-user` service, copying the existing `pg` block: same image/env var references (`POSTGRES_USER`/`POSTGRES_PASSWORD`), its own volume (`postgres_user_data`), its own init script mount, a distinct host port (`5434` — see port table below), same healthcheck shape.
2. **`.env`** — change `USER_DB_CONNECTION` from `Server=pg;...` to `Server=pg-user;...`.
3. **`scripts/postgres/init-user.sql`** — `CREATE DATABASE user_db;` (only needed if the shared init script doesn't already create it).
4. **`docker-compose.yml`** — change `user-api`'s `depends_on` from `pg` to `pg-user`.
5. **Start**: `docker-compose up -d pg-user user-api`.

No application code changes — this is purely a connection-string/topology change.

## Reference tables

| Service | Volume name | Dedicated port (if split) |
|---|---|---|
| auth | `postgres_auth_data` | 5433 |
| user | `postgres_user_data` | 5434 |
| inventory | `postgres_inventory_data` | 5435 |
| order | `postgres_order_data` | 5436 |
| product | `postgres_product_data` | 5437 |

Shared `pg` stays on `5432`.

## When to split

- **Development**: don't — single `pg` container.
- **Staging**: split the services under active load/schema-migration testing.
- **Production**: one dedicated Postgres instance per service (standard microservices data-ownership boundary).

## Verify

```bash
docker ps | grep pg-user
psql -h localhost -p 5434 -U postgres -d user_db
docker logs user-api | grep -i "connect"
```
