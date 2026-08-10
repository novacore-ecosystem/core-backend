# NovaCore API Gateway

A centralized YARP-based API Gateway for managing microservices with JWT authentication, route aggregation, and unified Swagger documentation.

## Features

- **Route Aggregation** - Configure multiple microservices with dynamic routing
- **JWT Authentication** - Built-in JWT bearer token validation
- **HTTP-Only Cookies** - Secure token storage via HTTP-only cookies
- **Swagger Aggregation** - View all service APIs from a single endpoint
- **Per-Service Authorization** - Configure which services require authentication
- **Scalable Architecture** - Easy to add new services with configuration

## Architecture

### Gateway Flow

```
Client Request
    ↓
Authentication (JWT from cookies)
    ↓
Authorization Check (per service)
    ↓
YARP Reverse Proxy
    ↓
Microservice
```

### Key Components

1. **GatewayOptions** - Configuration model for services and JWT
2. **DependencyInjection** - Registers authentication and reverse proxy
3. **AuthorizationMiddleware** - Enforces per-service auth requirements
4. **SwaggerAggregator** - Collects and displays all service Swagger UIs

## Configuration

Services are configured in `appsettings.json` under the `Gateway` section:

```json
{
  "Gateway": {
    "Services": {
      "ServiceKey": {
        "Url": "http://localhost:5001",
        "Name": "Service Display Name",
        "Path": "/api/service/",
        "SwaggerUrl": "http://localhost:5001/swagger/v1/swagger.json",
        "RequireAuth": true
      }
    },
    "Jwt": {
      "SecretKey": "your-secret-key-min-32-characters",
      "Issuer": "NovaCore.Auth",
      "Audience": "NovaCore.API"
    }
  }
}
```

### Service Configuration Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Url` | string | ✓ | Service base URL |
| `Name` | string | ✓ | Display name for UI and logs |
| `Path` | string | ✓ | Gateway route prefix (e.g., `/api/auth/`) |
| `SwaggerUrl` | string | | Full URL to service's Swagger JSON |
| `RequireAuth` | bool | | Whether to enforce JWT authentication (default: true) |

## Endpoints

### Gateway Interface
- `GET /` - Gateway home page with service list
- `GET /swagger` - Aggregated Swagger UI for all services
- `GET /api/swagger/{serviceName}` - Individual service Swagger JSON

### Service Routes
- `GET /api/{service}/**` - Proxied to configured service URL

**Example:** A request to `GET /api/auth/login` is forwarded to `http://localhost:5001/login`

## Usage

### Adding a New Service

1. Update `appsettings.json`:
```json
{
  "Gateway": {
    "Services": {
      // ... existing services ...
      "NewService": {
        "Url": "http://localhost:5006",
        "Name": "New Service",
        "Path": "/api/newservice/",
        "SwaggerUrl": "http://localhost:5006/swagger/v1/swagger.json",
        "RequireAuth": true
      }
    }
  }
}
```

2. Restart the gateway - no code changes needed!

### Accessing APIs

**Public Service (Auth):**
```bash
curl http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password"}'
```

**Protected Service (Product):**
```bash
curl http://localhost:5000/api/product/list \
  -H "Authorization: Bearer {token}"
```

Or using HTTP-only cookies:
```bash
curl http://localhost:5000/api/product/list \
  --cookie "AccessToken={token}"
```

### Viewing API Documentation

- **All APIs:** Visit `http://localhost:5000/swagger`
- **Gateway Info:** Visit `http://localhost:5000/`

## Authentication Flow

1. **Login** - Call `/api/auth/login` to get tokens
2. **Token Storage** - Tokens are stored in HTTP-only cookies
3. **Automatic Auth** - Gateway automatically reads cookies for protected routes
4. **Token Validation** - JWT is validated against configured Issuer/Audience/Secret

## Security Features

- **HTTP-Only Cookies** - Prevents XSS attacks on token
- **Secure Flag** - Cookies only sent over HTTPS
- **SameSite Strict** - CSRF protection
- **JWT Validation** - Issuer, audience, and signature verification
- **Per-Service Auth** - Granular control over which routes need auth

## Development

### Local Testing

1. Start the gateway:
```bash
cd src/ApiGateways/YarpApiGateway
dotnet run
```

2. Start a service (e.g., Auth):
```bash
cd src/Services/Auth/Auth.API
dotnet run
```

3. Access the gateway: `http://localhost:5000`

### Logging

Configure logging in `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "YarpApiGateway": "Debug",
      "Yarp": "Warning"
    }
  }
}
```

## Troubleshooting

### Service Not Accessible
- Check service URL in configuration
- Verify service is running
- Check CORS settings on service

### Authentication Issues
- Verify JWT secret matches Auth service
- Check token expiration
- Ensure cookies are enabled in client

### Swagger Not Loading
- Verify SwaggerUrl in configuration
- Check service Swagger endpoint is accessible
- Review browser console for CORS errors

## Future Enhancements

- [ ] Rate limiting per service
- [ ] Request/response logging
- [ ] Distributed caching
- [ ] Health checks
- [ ] Circuit breaker pattern
- [ ] API versioning support
