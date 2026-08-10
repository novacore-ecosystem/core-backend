# Setup: Default Dev Credentials & Access Points

**Scope:** local development credentials and service URLs. These are development-only values from `.env` — never use them anywhere reachable from outside your machine. Corrected from the former `setup/CREDENTIALS.md` (archived), which had a stale Postgres password and referenced a `PgAdmin`/`Mongo Express` setup that is no longer part of `docker-compose.yml`.

## Database credentials (from current `.env`)

| Service | Host (internal) | Port | User | Password |
|---|---|---|---|---|
| PostgreSQL | `pg` | 5432 | `postgres` | `POSTGRES_PASSWORD` in `.env` |
| MongoDB | `mongo` | 27017 | `admin` | `MONGO_INITDB_ROOT_PASSWORD` in `.env` |

**Always read the password from your local `.env`, not from this doc** — this table intentionally doesn't reproduce the literal value so this doc can't go stale again the way its predecessor did.

## Application access

| Service | URL |
|---|---|
| API Gateway (only published API) | http://localhost:5000 |
| Swagger (aggregated, via Gateway) | http://localhost:5000/swagger |
| Seq | http://localhost:5341 (no auth for local dev unless `SEQ_FIRSTRUN_ADMINPASSWORD` is set — see [troubleshooting/seq.md](../troubleshooting/seq.md)) |
| Kibana | http://localhost:5601 |
| Elasticsearch | http://localhost:9200 |

Individual services (`auth-api`, `user-api`) are **not** published to the host — see [01-architecture-map.md](../01-architecture-map.md#networking). Reach them via the Gateway or `docker exec`/container-name from inside the network.

## Quick connection commands

```bash
docker exec -it pg psql -U postgres
docker exec -it mongo mongosh -u admin -p "$MONGO_INITDB_ROOT_PASSWORD" --authenticationDatabase admin
docker exec redis redis-cli ping
docker exec kafka kafka-topics --bootstrap-server localhost:9092 --list
```

## Changing credentials

1. Update the value in `.env`.
2. `docker-compose down` (add `-v` only if you also want to wipe existing data encrypted/created under the old credential — otherwise services relying on persisted volumes may fail to reconnect).
3. `docker-compose up -d`.

## Related

- [setup/docker.md](docker.md) — day-to-day Docker commands and troubleshooting
- [setup/environment-config.md](environment-config.md) — `.env` workflow
- [services/gateway.md](../services/gateway.md) — Gateway configuration
