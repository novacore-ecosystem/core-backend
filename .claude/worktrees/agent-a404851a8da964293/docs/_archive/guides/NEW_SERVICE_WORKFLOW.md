# New Service Integration Workflow

**Complete checklist for adding a new microservice to NovaCore**

---

## 📋 Overview

When adding a new service (e.g., `notification-api`, `payment-api`), follow these steps:

1. **Clone service structure** from reference (Auth or User)
2. **Update docker-compose.yml**
3. **Configure .env**
4. **Wire API Gateway**
5. **Verify Swagger integration**
6. **Test end-to-end**

---

## ✅ Phase 1: Clone Service Structure

### Step 1.1: Copy Service Template

Clone from User Service (recommended reference):

```bash
cd src/Services/
cp -r User Payment  # Example: cloning to create Payment service
```

### Step 1.2: Rename Everything

Replace all `User` → `Payment` references:

```bash
# Rename project files
mv src/Services/Payment/User.Domain src/Services/Payment/Payment.Domain
mv src/Services/Payment/User.Persistence src/Services/Payment/Payment.Persistence
mv src/Services/Payment/User.Application src/Services/Payment/Payment.Application
mv src/Services/Payment/User.Infrastructure src/Services/Payment/Payment.Infrastructure
mv src/Services/Payment/User.API src/Services/Payment/Payment.API

# Update .csproj files (replace User → Payment)
sed -i 's/User/Payment/g' src/Services/Payment/**/*.csproj
```

### Step 1.3: Update Project Content

Update inside all files:
```csharp
// Old
namespace User.Domain.Entities;

// New
namespace Payment.Domain.Entities;
```

**Files to update:**
- All `.csproj` files
- All `namespace` declarations
- All `using` statements referencing User
- All class names (if needed)
- `GlobalUsings.cs` files

### Step 1.4: Update Domain Entities

Replace domain logic for your service:
```csharp
// Payment.Domain/Entities/Payment.cs (example)
public sealed class Payment : IEntity
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    // ... your domain logic
}
```

---

## ✅ Phase 2: Update docker-compose.yml

### Step 2.1: Add Service to Gateway Dependencies

Edit: `docker-compose.yml`

```yaml
yarp-api-gateway:
  depends_on:
    - auth-api
    - user-api
    - payment-api  # ← Add here
```

### Step 2.2: Add Service Container

Copy from user-api pattern and customize:

```yaml
  payment-api:
    image: ${PAYMENT_CONTAINER_NAME}
    build:
      context: .
      dockerfile: ./src/Services/Payment/Payment.API/Dockerfile
    depends_on:
      pg:
        condition: service_healthy
      redis:
        condition: service_healthy
      kafka:
        condition: service_started
      seq:
        condition: service_healthy
    networks:
      - default
    environment:
      - ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}
      - ConnectionStrings__DefaultConnection=${PAYMENT_DB_CONNECTION}
      - Logging__Seq__Url=${SEQ_URL}
```

---

## ✅ Phase 3: Configure .env

### Step 3.1: Add Service Variables

Add to `.env` (follow existing patterns):

```env
# ============================================================================
# PAYMENT SERVICE
# ============================================================================
PAYMENT_CONTAINER_NAME=payment-api
PAYMENT_SERVICE_URL=http://payment-api:8080
PAYMENT_HTTP_PORT=8080
PAYMENT_GRPC_PORT=5002

# Payment Database (Development: shared pg container | Production: separate payment_db)
PAYMENT_DB_CONNECTION=Server=pg;Port=5432;Database=payment_db;User Id=postgres;Password=${POSTGRES_PASSWORD};

# Payment Dependencies
PAYMENT_REDIS_URL=redis:6379
PAYMENT_KAFKA_BROKERS=kafka:9092
PAYMENT_SEQ_URL=http://seq:5341
```

### Step 3.2: Create Database in init.sql

Edit: `scripts/postgres/init.sql`

```sql
CREATE DATABASE payment_db;
```

---

## ✅ Phase 4: Configure docker-compose.override.yml

### Step 4.1: Add Service Configuration

Edit: `docker-compose.override.yml`

Add to gateway:
```yaml
yarp-api-gateway:
  environment:
    # ... existing variables
    - Payment__Url=${PAYMENT_SERVICE_URL}
```

Add service configuration (copy auth-api pattern, customize):
- **NO public ports** - services only expose internal ports to network
- REST communicates via API gateway (port 8080 internal)
- gRPC communicates via internal port (5002 internal)

```yaml
  payment-api:
    container_name: ${PAYMENT_CONTAINER_NAME}
    expose:
      - "8080"     # REST (internal only)
      - "5002"     # gRPC (internal only)
    environment:
      - ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}
      - ASPNETCORE_HTTP_PORT=8080
      - ASPNETCORE_GRPC_PORT=5002
      # Database
      - ConnectionStrings__DefaultConnection=${PAYMENT_DB_CONNECTION}
      # Logging
      - Logging__Seq__Url=${SEQ_URL}
      # Cache
      - Redis__Url=${PAYMENT_REDIS_URL}
      # Kafka
      - Kafka__BootstrapServers=${PAYMENT_KAFKA_BROKERS}
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
```

---

## ✅ Phase 5: Wire API Gateway

### Step 5.1: Update Gateway Configuration

Edit: `src/ApiGateways/YarpApiGateway/appsettings.json`

Add route cluster (copy from existing service pattern):

```json
{
  "ReverseProxy": {
    "Routes": {
      "payment-api": {
        "ClusterId": "payment",
        "Match": {
          "Path": "/api/payments/{**catch-all}"
        },
        "Transforms": [
          {
            "PathPattern": "/api/payments/{**catch-all}",
            "PathModified": "{**catch-all}"
          }
        ]
      }
    },
    "Clusters": {
      "payment": {
        "Destinations": {
          "payment-api-primary": {
            "Address": "http://payment-api:8080"
          }
        }
      }
    }
  }
}
```

### Step 5.2: Update Gateway appsettings.json

Add configuration mapping:

```json
{
  "GatewayConfig": {
    "Payment": {
      "Url": "http://payment-api:8080",
      "HealthCheckPath": "/health"
    }
  }
}
```

---

## ✅ Phase 6: Swagger Integration

### Step 6.1: Verify Swagger in Service

Service should expose Swagger at:
```
http://localhost:8080/swagger
```

Verify Dockerfile exposes correct port:
```dockerfile
EXPOSE 8080 5002
```

### Step 6.2: Update Gateway Swagger (if applicable)

If gateway aggregates Swagger docs:

Edit: Gateway `DependencyInjection.cs`

```csharp
services.AddSwaggerGen(options =>
{
    // ... existing setup
    
    options.AddServer(new OpenApiServer
    {
        Url = "/api/payments",
        Description = "Payment Service API"
    });
});
```

---

## ✅ Phase 7: Database Migrations

### Step 7.1: Create Migrations

```bash
cd src/Services/Payment/Payment.Persistence

# Create initial migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

### Step 7.2: Verify in Database

```bash
# Connect to PostgreSQL
psql -h localhost -U postgres -d payment_db

# List tables
\dt

# Verify Payment tables created
\dt public."Payment"
```

---

## ✅ Phase 8: Test Integration

### Step 8.1: Start Services

```bash
# Start gateway and services (no public ports exposed)
docker-compose up -d pg redis kafka seq yarp-api-gateway auth-api payment-api
```

### Step 8.2: Health Checks

```bash
# Gateway health (only public port)
curl http://localhost:5000/health

# Gateway routes to service (all traffic via gateway)
curl http://localhost:5000/api/payments/health

# From inside Docker network (for debugging)
docker exec yarp-api-gateway curl http://payment-api:8080/health
```

### Step 8.3: Swagger Access

```
# Only via gateway (if configured)
http://localhost:5000/api/payments/swagger

# Internal access (debug only)
docker exec -it payment-api curl http://localhost:8080/swagger
```

### Step 8.4: Test Endpoints

```bash
# All traffic goes through API Gateway (public port 5000)
curl -X GET http://localhost:5000/api/payments/list
curl -X POST http://localhost:5000/api/payments/create -d '{...}'

# Services are NOT accessible directly from host
# (only through gateway or from within Docker network)
```

---

## 🔍 Quick Checklist

### Clone Phase
- [ ] Copy service directory
- [ ] Rename projects (Domain, Persistence, Application, Infrastructure, API)
- [ ] Update all namespaces
- [ ] Update .csproj file references
- [ ] Update domain entities for business logic
- [ ] Rename Dockerfile references

### Docker Compose Phase
- [ ] Add service to gateway `depends_on`
- [ ] Add service container definition (copy pattern)
- [ ] Update docker-compose.override.yml with service config
- [ ] Add service URL to gateway environment

### Configuration Phase
- [ ] Add service variables to .env
- [ ] Add database creation to init.sql
- [ ] Verify all `${SERVICE_VAR}` references are defined

### Gateway Phase
- [ ] Add YARP route configuration
- [ ] Add cluster definition
- [ ] Add gateway environment variable for service URL
- [ ] Update Swagger docs

### Database Phase
- [ ] Create migrations: `dotnet ef migrations add InitialCreate`
- [ ] Update database: `dotnet ef database update`
- [ ] Verify tables in PostgreSQL

### Testing Phase
- [ ] Start all services: `docker-compose up`
- [ ] Check health endpoints
- [ ] Access Swagger UI
- [ ] Test via gateway routes
- [ ] Verify logs in Seq

---

## 📊 File Changes Summary

### Files to Create/Modify

| Phase | File | Action |
|-------|------|--------|
| 1 | `src/Services/[Service]/**` | Create new service structure |
| 2 | `docker-compose.yml` | Add service + gateway dependency |
| 3 | `.env` | Add service variables |
| 3 | `scripts/postgres/init.sql` | Add database creation |
| 4 | `docker-compose.override.yml` | Add service configuration |
| 5 | `YarpApiGateway/appsettings.json` | Add YARP routes |
| 6 | `[Service]/Program.cs` | Configure swagger |
| 7 | `[Service].Persistence/Migrations/` | Add migrations |

---

## 🚀 Start New Service Immediately

```bash
# 1. Clone structure
cp -r src/Services/User src/Services/NewService
cd src/Services/NewService && rename User NewService

# 2. Update docker-compose.yml
# (Add to depends_on and service definition)

# 3. Update .env
# (Add NewService variables)

# 4. Start
docker-compose up -d pg redis kafka seq yarp-api-gateway newservice-api

# 5. Verify (from inside the network — the service itself isn't published to the host)
docker exec yarp-api-gateway curl http://newservice-api:8080/health
curl http://localhost:5000/api/newservices/health
```

---

## 🔗 Port Convention

See [../architecture/NETWORK.md](../architecture/NETWORK.md) for the full picture. Summary: every service uses the **same** internal ports (8080 REST, 5002 gRPC) — they don't collide because each is its own container, and only the gateway (`5000`) is published to the host.

**Access Pattern:**
- ✅ Client → Gateway (5000) → Service (internal)
- ✅ Service → Service (internal network, by container name)
- ❌ Client → Service directly (not exposed — no `ports:` mapping)

---

## 📝 Template Files

Copy these patterns:

**Service in docker-compose.yml (internal network only):**
```yaml
[service-name]-api:
  image: ${[SERVICE]_CONTAINER_NAME}
  build:
    context: .
    dockerfile: ./src/Services/[ServiceName]/[ServiceName].API/Dockerfile
  depends_on:
    pg: { condition: service_healthy }
    redis: { condition: service_healthy }
    kafka: { condition: service_started }
    seq: { condition: service_healthy }
  networks: [default]
  environment:
    - ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}
    - ConnectionStrings__DefaultConnection=${[SERVICE]_DB_CONNECTION}
    - Logging__Seq__Url=${SEQ_URL}
```

**Service in docker-compose.override.yml (no public ports):**
```yaml
[service-name]-api:
  container_name: ${[SERVICE]_CONTAINER_NAME}
  expose:                          # Internal network only, no public ports
    - "8080"                       # REST port (internal)
    - "5002"                       # gRPC port (internal)
  environment:
    - ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}
    - ASPNETCORE_HTTP_PORT=8080
    - ASPNETCORE_GRPC_PORT=5002
    - ConnectionStrings__DefaultConnection=${[SERVICE]_DB_CONNECTION}
    - Logging__Seq__Url=${SEQ_URL}
  healthcheck:
    test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
    interval: 30s
    timeout: 10s
    retries: 3
    start_period: 40s
```

**.env template:**
```env
[SERVICE]_CONTAINER_NAME=[service]-api
[SERVICE]_SERVICE_URL=http://[service]-api:8080
[SERVICE]_HTTP_PORT=8080
[SERVICE]_GRPC_PORT=5002
[SERVICE]_DB_CONNECTION=Server=pg;Port=5432;Database=[service]_db;User Id=postgres;Password=${POSTGRES_PASSWORD};
```

Pull `POSTGRES_PASSWORD` (and any other credential) from the existing `.env` variable rather than hardcoding it per-service — see [ENV_CONFIGURATION.md](ENV_CONFIGURATION.md).
