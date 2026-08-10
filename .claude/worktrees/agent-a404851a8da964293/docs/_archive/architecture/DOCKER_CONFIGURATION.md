# Docker Configuration: 3-Layer Separation

NovaCore's Docker setup splits configuration into three layers with distinct responsibilities.

```
Layer 1: docker-compose.yml (Service Definitions)
├─ Service names, build contexts/Dockerfiles
├─ depends_on relationships
└─ Network definitions
        — does NOT contain container names, ports, env vars, credentials

Layer 2: docker-compose.override.yml (Deployment Config)
├─ Container names, internal `expose`d ports
├─ Environment variable wiring (${VAR} references into .env)
└─ Health checks
        — does NOT contain credentials directly (referenced from .env)

Layer 3: .env (Runtime Configuration)
├─ Credentials, hostnames, connection strings
├─ Port numbers
└─ Feature flags
        — does NOT contain Docker Compose syntax or service definitions
```

## Example

`docker-compose.yml`:
```yaml
auth-api:
  build:
    context: .
    dockerfile: ./src/Services/Auth/Auth.API/Dockerfile
  depends_on:
    pg: { condition: service_healthy }
```

`docker-compose.override.yml`:
```yaml
auth-api:
  container_name: ${AUTH_CONTAINER_NAME}
  expose:
    - "8080"    # REST — internal only, not published to host
    - "5002"    # gRPC — internal only
  environment:
    - ASPNETCORE_HTTP_PORT=8080
    - ConnectionStrings__DefaultConnection=${AUTH_DB_CONNECTION}
```

`.env`:
```
AUTH_CONTAINER_NAME=auth-api
AUTH_HTTP_PORT=8080
AUTH_GRPC_PORT=5002
AUTH_DB_CONNECTION=Server=pg;Port=5432;Database=auth_db;User Id=postgres;Password=...;
```

Only the **gateway** publishes a host port (`5000`). Every service is `expose`d to the Docker network only — see [NETWORK.md](NETWORK.md).

## Benefits

- **Separation of concerns** — topology (compose) is independent of deployment (override) is independent of values (.env)
- **Reusability** — the same `docker-compose.yml` works for dev/test/staging/prod; swap `.env` files
- **Security** — credentials live only in `.env`, which is git-ignored; compose files can be committed safely
- **Flexibility** — change ports/names/credentials by editing `.env`, no compose file changes
- **CI/CD friendly** — inject a different `.env` file per pipeline stage: `docker-compose --env-file .env.staging -p staging up -d`

## Variable Naming Convention

| Category | Examples |
|---|---|
| Container names | `AUTH_CONTAINER_NAME`, `GATEWAY_CONTAINER_NAME` |
| Ports | `AUTH_HTTP_PORT`, `AUTH_GRPC_PORT`, `GATEWAY_PORT` |
| Credentials | `POSTGRES_PASSWORD`, `MONGO_INITDB_ROOT_PASSWORD` |
| Connection strings | `AUTH_DB_CONNECTION`, `ORDER_MONGODB_URL` |
| Service URLs (internal) | `AUTH_SERVICE_URL`, `USER_SERVICE_URL` |
| Config | `ASPNETCORE_ENVIRONMENT`, `LOG_LEVEL` |

## Best Practices

1. `.gitignore` the `.env` file — it holds credentials
2. Commit `.env.template` as the reference (no secrets, every key present)
3. Keep `docker-compose.yml` to service topology only
4. All runtime values live in `.env` — single source of truth
5. Validate `.env` against `.env.template` before deploying (see [ENV_CONFIGURATION.md](../guides/ENV_CONFIGURATION.md))

## Troubleshooting

| Symptom | Check |
|---|---|
| `${VAR}` not substituted | Variable defined in `.env`? Running `docker-compose` from the project root (where `.env` lives)? |
| Port conflict | Change the relevant `*_PORT` value in `.env`, `docker-compose restart` |
| Container name already in use | Change the relevant `*_CONTAINER_NAME` value in `.env` |

## See Also

- [NETWORK.md](NETWORK.md) — traffic flow, port assignments, network isolation
- [../guides/ENV_CONFIGURATION.md](../guides/ENV_CONFIGURATION.md) — `.env` / `.env.template` workflow for adding new variables
- [../setup/DOCKER_COMPLETE.md](../setup/DOCKER_COMPLETE.md) — day-to-day Docker commands
