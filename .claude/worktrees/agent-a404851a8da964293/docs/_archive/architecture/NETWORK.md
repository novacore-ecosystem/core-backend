# Network Architecture

Internal network-only services, all traffic routed through the API Gateway.

## Overview

```
                     CLIENT (Host Machine)
                            │
                            │ HTTP/REST
                            ↓
                    ┌─────────────────┐
                    │  API Gateway    │
                    │  (Port 5000)    │
                    └────────┬────────┘
                             │
        ┌────────────────────┼────────────────────┐
        │                    │                    │
        ↓                    ↓                    ↓
   ┌─────────┐          ┌─────────┐         ┌──────────┐
   │  Auth   │          │  User   │         │ Product  │
   │ (8080)  │          │ (8080)  │         │ (8080)   │
   └────┬────┘          └────┬────┘         └────┬─────┘
        │ gRPC (5002)        │ gRPC (5002)        │ gRPC (5002)
        └─────────┬───────────┴──────────────────┘
                  │
        Internal Docker Network
                  │
        ┌─────────┼─────────┐
        │         │         │
        ↓         ↓         ↓
      Postgres  Redis  Kafka/Seq/Mongo/Elasticsearch
```

Every service binds REST to `8080` and gRPC to `5002` internally (set via `ASPNETCORE_HTTP_PORT`/`*_GRPC_PORT` per-service env vars — same numbers, different containers, no collision since each is its own container on the Docker network). Only the **gateway** is published to the host.

## Traffic Flows

**REST request**: `Client → Gateway (5000) → Service (internal :8080) → back through Gateway`

**Inter-service gRPC**: `Service A (:5002) → Service B (:5002)`, same Docker network, no gateway involvement

**Database access**: `Service → pg:5432`, same Docker network, no public port needed

## Port Assignments

### Public (exposed to host, from `.env`)
| Service | Port |
|---|---|
| API Gateway | **5000** |
| PostgreSQL | 5432 |
| Redis | 6379 |
| MongoDB | 27017 |
| Kafka | 9092 |
| Seq (logging UI) | 5341 |
| Elasticsearch | 9200 |
| Kibana | 5601 |
| PgAdmin | 5050 |
| Mongo Express | 8081 |

### Internal (Docker network only, uniform across services)
| Service | REST | gRPC |
|---|---|---|
| auth | 8080 | 5002 |
| user | 8080 | 5002 |
| inventory | 8080 | 5002 |
| order | 8080 | 5002 |
| product | 8080 | 5002 |

Each service is its own container, so identical internal port numbers don't collide — they're only reachable by container name (e.g. `http://auth-api:8080`) from inside the Docker network.

## Docker Compose Wiring

`docker-compose.override.yml` uses `expose:` (internal-only), never `ports:`, for individual services:

```yaml
auth-api:
  container_name: ${AUTH_CONTAINER_NAME}
  expose:
    - "8080"   # REST — internal only
    - "5002"   # gRPC — internal only
  environment:
    - ASPNETCORE_HTTP_PORT=8080
    - ASPNETCORE_GRPC_PORT=5002
```

`expose:` vs `ports:` — `ports:` publishes to the host (public), `expose:` is internal-network-only.

## Network Isolation

**Public**: Gateway, Postgres, Redis, Mongo, Kafka, Seq, Elasticsearch, Kibana, PgAdmin, Mongo Express (dev-only management tools)

**Private (internal only)**: every service API (auth-api, user-api, inventory-api, order-api, product-api)

### Client access

```bash
# Correct — through the gateway
curl http://localhost:5000/api/auth/login

# Wrong — services aren't published to the host
curl http://localhost:8080/login   # connection refused from the host; 8080 only resolves inside the Docker network
```

## Service-to-Service Communication

```csharp
// gRPC (preferred for service-to-service)
var channel = GrpcChannel.ForAddress("http://user-api:5002");
var client = new UserService.UserServiceClient(channel);

// REST (via internal network, container name resolves via Docker DNS)
var response = await httpClient.GetAsync("http://user-api:8080/users");
```

## Example: Registration Flow

```
1. Client:        POST http://localhost:5000/api/auth/register
2. Gateway (5000): routes /api/auth/* → http://auth-api:8080
3. Auth (8080):    creates account, calls User service via gRPC (user-api:5002)
4. User (8080):    creates the user profile in Postgres
5. Response propagates back: User → Auth → Gateway → Client
```

## Debugging

```bash
# Health via gateway
curl http://localhost:5000/health

# Health from inside the network
docker exec yarp-api-gateway curl http://auth-api:8080/health

# Network inspection
docker network inspect novacore-network
docker exec auth-api ping user-api
```

## Checklist for a New Service

- [ ] Defined in `docker-compose.yml` (build + `depends_on`, internal network)
- [ ] `expose:` (not `ports:`) in `docker-compose.override.yml`
- [ ] `ASPNETCORE_HTTP_PORT`/`*_GRPC_PORT` env vars set (8080 / 5002 convention)
- [ ] Database connection points at the shared `pg`/`mongo` container (or its own, see [DATABASE_SPLIT_GUIDE.md](../setup/DATABASE_SPLIT_GUIDE.md))
- [ ] Gateway route added for `/api/{service}/*`
- [ ] No `ports:` mapping — only the gateway is public

## See Also

- [DOCKER_CONFIGURATION.md](DOCKER_CONFIGURATION.md) — the compose/override/.env layering this builds on
- [../guides/NEW_SERVICE_WORKFLOW.md](../guides/NEW_SERVICE_WORKFLOW.md) — full checklist for wiring up a new service
- [../setup/DATABASE_SPLIT_GUIDE.md](../setup/DATABASE_SPLIT_GUIDE.md) — splitting a service to its own database
