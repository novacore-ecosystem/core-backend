# Setup: Docker

**Scope:** the 3-layer Docker Compose configuration and day-to-day commands. Merges the former `architecture/DOCKER_CONFIGURATION.md` + `setup/DOCKER_COMPLETE.md` (archived, see [08-migration-plan.md](../08-migration-plan.md)).

## 3-layer configuration split

```
docker-compose.yml          — service topology only: names, build contexts, depends_on, networks.
                               NO container names, ports, env vars, or credentials.
docker-compose.override.yml — deployment config: container_name, expose'd ports, env var WIRING
                               (${VAR} references into .env), health checks. No literal credentials.
.env                         — runtime values: credentials, hostnames, connection strings, ports,
                               feature flags. NO Docker Compose syntax.
```

`.env` is git-ignored; `.env.template` is committed with every key present but no secret values — see [environment-config.md](environment-config.md) for the workflow when adding a new variable.

Only the **Gateway** publishes a host port (`5000`, `GATEWAY_PORT` in `.env`). Every other service is `expose`d to the Docker network only, reachable via container name or through the Gateway — see [01-architecture-map.md](../01-architecture-map.md#networking).

## Variable naming convention

| Category | Examples |
|---|---|
| Container names | `AUTH_CONTAINER_NAME`, `GATEWAY_CONTAINER_NAME` |
| Ports | `AUTH_HTTP_PORT`, `AUTH_GRPC_PORT`, `GATEWAY_PORT` |
| Credentials | `POSTGRES_PASSWORD`, `MONGO_INITDB_ROOT_PASSWORD` |
| Connection strings | `AUTH_DB_CONNECTION` |
| Service URLs (internal) | `AUTH_SERVICE_URL`, `USER_SERVICE_URL` |

## Quick start

```bash
bash scripts/validate-docker-setup.sh   # pre-flight checks
bash scripts/startup.sh                 # builds + starts; DO NOT trust its printed summary banner —
                                         # it currently prints stale ports/credentials, see setup/credentials.md
docker-compose up -d --build            # equivalent manual form

# NOTE (per current project constraint): avoid rebuilding the whole stack for a single-service change.
# Rebuild only the affected service — see the "Rebuild after code changes" command below — to
# keep memory usage and iteration time down. Full-stack rebuild is for initial setup only.
```

## Common commands

```bash
docker-compose up -d                      # start in background
docker-compose down                       # stop, keep volumes
docker-compose down -v                    # stop, remove volumes
docker-compose restart auth-api           # restart one service
docker-compose logs -f auth-api           # tail one service's logs
docker-compose up -d --build auth-api     # rebuild + restart ONE service after a code change (preferred over full rebuild)
docker-compose exec auth-api sh
```

## Health checks

```bash
curl http://localhost:5000/health                              # gateway (only host-published health endpoint)
docker exec yarp-api-gateway curl http://auth-api:8080/health   # a service, from inside the network
```

## Database access

```bash
docker exec -it pg psql -U postgres    # \l list dbs, \c auth_db connect, \dt list tables
```
Credentials come from `.env` — see [credentials.md](credentials.md).

## Troubleshooting

| Symptom | Steps |
|---|---|
| Services won't start | `docker ps` → `docker-compose config` (valid compose?) → `docker-compose logs` |
| `${VAR}` not substituted | Confirm the variable exists in `.env` and you're running compose from the project root |
| Port already in use | `lsof -i :5000`, or change `GATEWAY_PORT` in `.env` |
| Database not connecting | `docker-compose ps` (container healthy?) → `docker-compose exec pg pg_isready -U postgres` |
| Container fails to start | `docker-compose logs <service>` → `docker inspect <container-id>` |
| Out of memory during build | Build only the affected project's Dockerfile target, don't `--build` the whole stack (see above) |

## Service wiring status

Every service (Auth, User, Product, Inventory, Order, Audit) is implemented and has a real service block in `docker-compose.yml`/`docker-compose.override.yml` — see [01-architecture-map.md](../01-architecture-map.md#services). Audit's block additionally depends on `mongo` (its domain data is 100% MongoDB) alongside the usual `pg`/`kafka`/`seq`. `docker-compose ps` is authoritative over any doc for what's actually running at a given moment.
