# Auth Service - Complete Configuration Guide

## 📋 Overview

The Auth Service is configured with:
- Swagger UI with JWT authentication
- CORS policies
- JWT Bearer authentication + authorization middleware
- Carter endpoints
- Health checks, exception handling, logging

**Ports**: internally binds to `8080` (via `ASPNETCORE_HTTP_PORT`, same as every other service). In Docker Compose it is not exposed to the host directly — reach it through the gateway at `localhost:5000/api/auth/...`. For local `dotnet run` outside Docker, `http://localhost:8080` works directly. See [NETWORK_ARCHITECTURE.md](../architecture/NETWORK.md) for the full port scheme.

---

## 🔧 Configuration Details

### 1. **Swagger UI Configuration**

#### Features:
- Auto-discovery at root path (`/`)
- JWT Bearer token input
- Service metadata
- Interactive API testing

#### Access:
```
http://localhost:8080/
http://localhost:8080/swagger
```

#### Example Usage in Swagger:
1. Click "Authorize" button
2. Paste token: `Bearer {your_jwt_token}`
3. Test endpoints

---

### 2. **CORS Configuration**

#### Policies Configured:

**AllowAll** (Development only)
```csharp
.AllowAnyOrigin()
.AllowAnyMethod()
.AllowAnyHeader()
```

**AllowGateway** (Default)
```csharp
.WithOrigins("http://localhost:5000", "http://localhost:5001")
.AllowAnyMethod()
.AllowAnyHeader()
.AllowCredentials()
```

#### Usage:
```csharp
// To change CORS policy
app.UseCors("AllowAll");  // Development
app.UseCors("AllowGateway");  // Default (Recommended)
```

---

### 3. **Authentication & Authorization**

#### JWT Configuration:
```csharp
// Automatically configured in AddInfrastructure()
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(...)
```

#### Middleware Order:
```csharp
app.UseAuthentication();    // Validates JWT token
app.UseAuthorization();     // Checks permissions
```

#### Protecting Endpoints:
```csharp
app.MapPost("/protected", handler)
    .RequireAuthorization();  // Requires valid JWT
```

---

### 4. **Carter Endpoints**

#### Automatically Discovered Modules:
```
✅ Endpoints/Login.cs
✅ Endpoints/Register.cs
✅ Endpoints/RefreshToken.cs
✅ Endpoints/Logout.cs
```

#### Endpoint Structure:
```csharp
public sealed class Login : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/login", Handle)
            .AllowAnonymous()
            .WithName("Login")
            .WithOpenApi();
    }
}
```

#### Available Routes:
```
POST   /login
POST   /register
POST   /refresh-token
POST   /logout
GET    /health
```

---

### 5. **Health Checks**

#### Endpoint:
```
GET http://localhost:8080/health
```

#### Response:
```json
{
  "status": "Healthy",
  "results": {}
}
```

#### Used by:
- Docker health checks
- Kubernetes probes
- Load balancers
- Monitoring systems

---

## 🚀 Running the Service

### Local Development
```bash
cd src/Services/Auth/Auth.API
dotnet run
```

Access:
- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/`
- Health: `http://localhost:8080/health`

### Docker
```bash
docker-compose up -d auth-api
```

### Verify Running
```bash
# Check health
curl http://localhost:8080/health

# Check Swagger
curl -s http://localhost:8080/ | head -20

# Test login endpoint
curl -X POST http://localhost:8080/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password123"}'
```

---

## 🔐 Authentication Flow

### 1. **Register** (Optional)
```bash
POST /register
Content-Type: application/json

{
  "email": "user@example.com",
  "username": "user",
  "password": "password123"
}
```

### 2. **Login**
```bash
POST /login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}

Response:
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "ABC123..."
}
```

### 3. **Use Token**
```bash
GET /api/product/list
Authorization: Bearer eyJhbGc...
```

### 4. **Refresh Token**
```bash
POST /refresh-token
Content-Type: application/json

{
  "refreshToken": "ABC123..."
}
```

### 5. **Logout**
```bash
POST /logout
Authorization: Bearer eyJhbGc...
```

---

## 📊 Middleware Pipeline

The complete middleware pipeline in order:

```
1. Exception Handling (DeveloperExceptionPage in dev)
   ↓
2. Swagger Documentation & UI
   ↓
3. CORS Policy (AllowGateway)
   ↓
4. HSTS & HTTPS Redirect
   ↓
5. Authentication (JWT validation)
   ↓
6. Authorization (Permission checks)
   ↓
7. Routing
   ├─ /swagger → Swagger UI
   ├─ /health → Health checks
   ├─ /login → Login endpoint
   ├─ /register → Register endpoint
   ├─ /refresh-token → Token refresh
   └─ /logout → Logout endpoint
```

---

## ⚙️ Configuration Files

### Program.cs
```csharp
// Service registration
builder.Services.AddPersistence(...)
builder.Services.AddInfrastructure(...)
builder.Services.AddApplication(...)
builder.Services.AddPresentation()  // Includes Swagger, CORS, Carter

// Health checks
builder.Services.AddHealthChecks()

// Middleware pipeline
app.UsePresentation()  // Handles all middleware
app.UseApplication()   // Custom business logic
```

### DependencyInjection.cs
```csharp
// Services
AddEndpointsApiExplorer()
AddSwaggerGen()
AddCors()
AddCarter()
AddHealthChecks()

// Middleware
UseSwagger()
UseSwaggerUI()
UseCors()
UseAuthentication()
UseAuthorization()
MapCarter()
MapHealthChecks()
```

---

## 🧪 Testing

### Test with cURL
```bash
# Health check
curl http://localhost:8080/health

# Login
curl -X POST http://localhost:8080/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","password":"admin123"}'

# Protected endpoint with token
curl http://localhost:8080/protected \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### Test with Postman
1. Import Auth Service endpoints
2. Set variable: `{{base_url}}` = `http://localhost:8080`
3. Set variable: `{{token}}` = JWT from login response
4. Use `{{token}}` in Authorization headers

### Test with Swagger UI
1. Open `http://localhost:8080/`
2. Click "Authorize"
3. Paste: `Bearer {token}`
4. Use "Try it out" on endpoints

---

## 🔒 Security Considerations

### Development vs Production

**Development (.env)**
```env
ASPNETCORE_ENVIRONMENT=Development
SEQ_FIRSTRUN_NOAUTHENTICATION=true
```

**Production (.env.prod)**
```env
ASPNETCORE_ENVIRONMENT=Production
# Use strong passwords
# Enable HTTPS only
# Restrict CORS origins
# Enable logging
```

### CORS Policy Change
```csharp
// In Program.cs, change:
app.UseCors("AllowAll");  // ❌ Development only

// To production-safe:
app.UseCors("AllowGateway");  // ✅ Restricted origins
```

---

## 📝 Environment Variables

```env
# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Docker
ASPNETCORE_URLS=http://+:5000
ASPNETCORE_HTTP_PORT=5000
ASPNETCORE_HTTPS_PORT=5001

# JWT (from Auth.Infrastructure)
Jwt__SecretKey=your-secret-key
Jwt__Issuer=NovaCore.Auth
Jwt__Audience=NovaCore.API

# Database
ConnectionStrings__DefaultConnection=Server=postgres;...

# Logging
SEQ_URL=http://seq:5341
```

---

## 🆘 Troubleshooting

### Swagger not loading
```bash
# Check service is running
curl http://localhost:8080/health

# Verify Swagger endpoint
curl http://localhost:8080/swagger/v1/swagger.json
```

### CORS errors
```
Access to XMLHttpRequest blocked by CORS policy
```

**Solution:**
```csharp
// In DependencyInjection.cs, update CORS policy:
options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyMethod());
```

### Authentication errors
```
401 Unauthorized: Invalid token
```

**Solution:**
1. Ensure token is valid and not expired
2. Token format: `Bearer {token}` (with space)
3. Check JWT secret matches Auth.Infrastructure config

### Port already in use
```bash
# Find process using port 8080
lsof -i :8080

# Kill process
kill -9 <PID>

# Or use different port
ASPNETCORE_HTTP_PORT=5110 dotnet run
```

---

## Registration Flow (Event-Driven, Async Rollback)

`POST /register` creates the Auth account, generates tokens, and — via events — asks the User service to create the matching profile. If profile creation fails, the Auth account is rolled back.

```
RegisterHandler (Auth.Application)
  ├─ Create UserAccount
  ├─ Publish OnUserRegisteredEvent (MediatR, in-process)
  │  └─ OnUserRegisteredHandler
  │     ├─ Call IUserProfileService.CreateUserProfileAsync (gRPC, or StubUserProfileService if Grpc:Enabled=false)
  │     ├─ Success → done
  │     └─ Failure → Publish OnUserDeletionEvent
  │        └─ OnUserDeletionHandler (Auth.Application)
  │           └─ Delete UserAccount (sync)
  ├─ Generate access + refresh tokens → HTTP-only cookies
  └─ Return ApiResponse<object>.Ok(MessageCode.Created)  — no tokens in the response body
```

`OnUserRegisteredEvent`/`OnUserDeletionEvent` are Application-layer events (MediatR, in-process, same service) — see [EVENT_ARCHITECTURE.md](../architecture/EVENT_ARCHITECTURE.md) for how that differs from a Domain event or a cross-service Integration event.

**Known gap**: the original design also intended an async resilience leg — publish `UserAccountDeletionIntegrationEvent` to Kafka so a queue consumer could re-publish `OnUserDeletionEvent` if the synchronous deletion somehow didn't run. `UserAccountDeletionIntegrationEvent` (the contract) still exists, but nothing currently implements `IIntegrationEventConsumer` for it — the resilience leg is not wired up. The synchronous `OnUserDeletionHandler` deletion still works; only the async fallback is missing. See [EVENT_MESSAGING_REFACTOR.md](../decisions/EVENT_MESSAGING_REFACTOR.md).

### Key Files

- `Auth.Application/Features/Auth/Events/OnUserRegistered/` — event + handler
- `Auth.Application/Features/Auth/Events/OnUserDeletion/` — event + handler
- `Auth.Infrastructure/Services/StubUserProfileService.cs` — used when `Grpc:Enabled=false`
- `BuildingBlock.Application/Abstractions/Common/GrpcServiceResult.cs` — generic gRPC response wrapper, reusable across services

### DI Registration (current)

```csharp
services
    .AddAppLogger()
    .AddCurrentUserServices()
    .AddRedisCache(configuration)
    .AddRoleCaching(configuration)
    .AddBackgroundJobs(configuration)
    .AddSecurityServices(configuration)
    .AddDomainEventPublisher()
    .AddApplicationEventDispatcher()
    .AddKafkaMessaging(configuration, "auth-service")
    .AddGrpcClients(configuration)
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddHttpContextAccessor();
```

## Enabling gRPC (currently disabled)

Auth currently talks to the User service through `StubUserProfileService` (always succeeds, no real call). To switch to the real gRPC client:

1. **Config**: set `"Grpc": { "Enabled": true, "UserService": { "Url": "http://user-api:8080" } }` in `appsettings.json`
2. **DI**: in `Auth.Infrastructure/DependencyInjection.cs`, `AddGrpcClients` already branches on `Grpc:Enabled` — uncomment the real registration block:
   ```csharp
   services.AddGrpcClient<UserService.UserServiceClient>(o => o.Address = new Uri(userServiceUrl));
   services.AddScoped<IUserProfileService, UserProfileServiceClient>();
   ```
3. **Client**: rename `Auth.Infrastructure/GrpcClients/UserProfileServiceClient.cs.bak` → `.cs`. It already returns `GrpcServiceResult<UserProfileData>`, so no handler changes are needed.
4. **Proto/project refs**: add the `GrpcClient` proto reference to `Auth.Application.csproj` and the contracts project reference to `Auth.Infrastructure.csproj` if not already present.

No changes needed in `OnUserRegisteredHandler` or `RegisterHandler` — they already depend on the `IUserProfileService` abstraction.

**Rollback if gRPC misbehaves**: set `Grpc:Enabled=false` and restart — DI automatically falls back to `StubUserProfileService`, no code changes.

## 📚 Related Documentation

- [CREDENTIALS.md](../setup/CREDENTIALS.md) — default credentials
- [GATEWAY.md](GATEWAY.md) — gateway configuration
- [EVENT_ARCHITECTURE.md](../architecture/EVENT_ARCHITECTURE.md) — domain/application/integration event patterns
- [troubleshooting/SEQ.md](../troubleshooting/SEQ.md) — logging issues

---

## ✅ Checklist - Ready to Deploy

- [ ] Swagger UI accessible at `/`
- [ ] JWT authentication working
- [ ] CORS policies configured
- [ ] Health check responds
- [ ] All endpoints registered
- [ ] Database connections working
- [ ] Logging to Seq working
- [ ] Can login and get token
- [ ] Protected endpoints enforce auth
- [ ] Token refresh working
