# Docker Guide

## Prerequisites

- Docker Desktop (or Docker Engine + Docker Compose)
- .NET 10 SDK (for local development outside containers)
- 8GB+ RAM recommended, 50GB+ free disk space for volumes

## Quick Start

```bash
# 1. Validate setup
bash scripts/validate-docker-setup.sh

# 2. Start services
bash scripts/startup.sh          # recommended — automated startup
# or: docker-compose up -d
# or: make up

# 3. Verify health
bash scripts/health-check.sh
# or: make health
```

## Architecture Overview

**Microservices**: YARP API Gateway (entry point), Auth, Inventory, Order, Product, User

**Infrastructure**: PostgreSQL (primary relational DB), MongoDB (Order service + audit logs), Redis (cache), Kafka in KRaft mode (message queue), Seq (structured logging), Elasticsearch + Kibana (search/analytics)

See [../architecture/NETWORK.md](../architecture/NETWORK.md) for port assignments and traffic flow, [../architecture/DOCKER_CONFIGURATION.md](../architecture/DOCKER_CONFIGURATION.md) for the compose/override/.env layering.

## Access Points

| Category | Service | Address |
|---|---|---|
| API | Gateway (only public API port) | http://localhost:5000 |
| Database | PostgreSQL | localhost:5432 |
| Database | MongoDB | localhost:27017 |
| Cache | Redis | localhost:6379 |
| Queue | Kafka | localhost:9092 |
| Logging | Seq | http://localhost:5341 |
| Search | Elasticsearch | http://localhost:9200 |
| Search | Kibana | http://localhost:5601 |

Individual service APIs (auth-api, user-api, ...) are **not** published to the host — reach them via the gateway (`http://localhost:5000/api/{service}/...`) or from inside the Docker network by container name.

## Common Commands

```bash
# Build / start / stop
docker-compose build
docker-compose up -d                      # start in background
docker-compose down                       # stop, keep volumes
docker-compose down -v                    # stop, remove volumes (full cleanup)
docker-compose restart auth-api           # restart one service

# Logs
docker-compose logs -f                    # all services
docker-compose logs -f auth-api           # one service

# Rebuild after code changes
docker-compose up -d --build auth-api

# Exec into a container
docker-compose exec auth-api sh
docker-compose exec auth-api dotnet ef database update

# Make shortcuts (if Makefile present)
make help | make build | make up | make down | make logs | make health | make clean | make dev-tools
```

## Database Management

**PostgreSQL:**
```bash
docker exec -it pg psql -U postgres
\l                    # list databases
\c auth_db            # connect to a database
\dt                   # list tables
```

**MongoDB:**
```bash
docker exec -it mongo mongosh -u admin -p "$MONGO_ADMIN_PASSWORD"
show dbs
use order_db
```

Credentials come from `.env` — see [CREDENTIALS.md](CREDENTIALS.md), never hardcode them in commands you save/share.

## Health Checks

```bash
curl http://localhost:5000/health                       # gateway
docker exec yarp-api-gateway curl http://auth-api:8080/health   # a service, from inside the network
```

## Development Workflow

1. Edit code in your IDE
2. `docker-compose up -d --build auth-api`
3. `docker-compose logs -f auth-api` to verify

## Testing

```bash
# Unit tests inside a fresh container
docker-compose run --rm auth-api dotnet test

# Integration tests against the running stack
docker-compose up -d
docker-compose exec auth-api dotnet test --logger "console;verbosity=detailed"
```

## CI/CD

```bash
docker-compose build
docker-compose up -d
docker-compose exec -T auth-api dotnet test
docker-compose down -v
```

## Performance (Production Considerations)

```yaml
# Resource limits
services:
  auth-api:
    deploy:
      resources:
        limits: { cpus: '1', memory: 512M }
        reservations: { cpus: '0.5', memory: 256M }

# Log rotation, to prevent disk space issues
services:
  auth-api:
    logging:
      driver: "json-file"
      options: { max-size: "10m", max-file: "3" }
```

## Cleaning Up

```bash
docker-compose down -v                    # remove containers + volumes
docker-compose down -v --rmi all          # also remove images
docker system prune -a --volumes          # prune everything unused (careful — affects other projects too)
```

## Troubleshooting

| Symptom | Steps |
|---|---|
| Services won't start | `docker ps` (Docker running?) → `docker-compose config` (valid compose?) → `docker-compose logs` |
| Port already in use | `lsof -i :5000` to find the process, or change `GATEWAY_PORT` in `.env` |
| Database not connecting | `docker-compose ps` (container healthy?) → `docker-compose exec pg pg_isready -U postgres` |
| Out of disk space | `docker system prune -a --volumes`, or remove just this project's volumes: `docker volume ls \| grep novacore` |
| Container fails to start | `docker-compose logs <service>` → `docker inspect <container-id>` |
| Network issues | `docker network inspect novacore-network`, then `docker-compose down && docker-compose up -d` |
| Memory issues | `docker stats` to see usage; add resource limits (above) or increase system RAM |

## Further Documentation

- [../architecture/DOCKER_CONFIGURATION.md](../architecture/DOCKER_CONFIGURATION.md) — compose/override/.env layering
- [../architecture/NETWORK.md](../architecture/NETWORK.md) — ports and traffic flow
- [ENV_CONFIGURATION.md](../guides/ENV_CONFIGURATION.md) — adding new environment variables
- Docker docs: https://docs.docker.com/ · Docker Compose docs: https://docs.docker.com/compose/
