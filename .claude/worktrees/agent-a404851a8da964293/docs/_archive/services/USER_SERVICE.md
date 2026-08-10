# User Service - Complete Configuration Guide

## 📋 Overview

The User Service handles user profile management in NovaCore with:
- ✅ Swagger UI with JWT authentication
- ✅ CORS policies
- ✅ User CRUD operations
- ✅ Carter endpoints
- ✅ Health checks
- ✅ Exception handling
- ✅ Logging

---

## 🏗️ Architecture

### Layered Structure

```
User.API (Presentation)
├── Endpoints/
│   ├── CreateUserEndpoint.cs
│   ├── GetUserEndpoint.cs
│   └── UpdateUserEndpoint.cs
├── DependencyInjection.cs
├── ApplicationPipeline.cs
└── Program.cs

User.Application (Business Logic)
├── Features/
│   └── Users/
│       ├── Commands/
│       │   ├── CreateUser/
│       │   └── UpdateUser/
│       └── Queries/
│           └── GetUser/
└── DependencyInjection.cs

User.Infrastructure (External Services)
└── DependencyInjection.cs

User.Persistence (Data Access)
├── Configurations/
│   └── UserConfiguration.cs
├── Seeders/
│   └── UserSeeder.cs
├── UserDbContext.cs
└── DependencyInjection.cs

User.Domain (Business Entities)
├── Entities/
│   └── User.cs
└── Enums/
    └── UserStatus.cs
```

---

## 🔧 Configuration Details

### 1. **Swagger UI Configuration**

#### Features:
- Auto-discovery at root path (`/`)
- Service metadata
- Interactive API testing

#### Access:
```
http://localhost:8080/
http://localhost:8080/swagger
```

---

### 2. **CORS Configuration**

#### Policy: AllowAll (Development)
```csharp
.AllowAnyOrigin()
.AllowAnyMethod()
.AllowAnyHeader()
```

#### Usage in Program.cs:
```csharp
app.UseCors("AllowAll");
```

---

### 3. **Carter Endpoints**

#### Available Routes:

```
POST   /users              # Create new user
GET    /users/{userId}     # Get user by ID
PUT    /users/{userId}     # Update user profile
GET    /health             # Health check
```

#### Endpoint Examples:

**Create User:**
```bash
curl -X POST http://localhost:8080/users \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "userName": "johndoe",
    "phoneNumber": "1234567890",
    "firstName": "John",
    "lastName": "Doe"
  }'
```

**Get User:**
```bash
curl http://localhost:8080/users/550e8400-e29b-41d4-a716-446655440000
```

**Update User:**
```bash
curl -X PUT http://localhost:8080/users/550e8400-e29b-41d4-a716-446655440000 \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Smith",
    "phoneNumber": "0987654321"
  }'
```

---

### 4. **Health Checks**

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

---

## 🚀 Running the Service

### Local Development

```bash
cd src/Services/User/User.API
dotnet run
```

Access:
- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/`
- Health: `http://localhost:8080/health`

### Docker

```bash
docker-compose up -d user-api
```

### Verify Running

```bash
# Check health
curl http://localhost:8080/health

# Check Swagger
curl -s http://localhost:8080/ | head -20

# Test create user endpoint
curl -X POST http://localhost:8080/users \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","userName":"user","phoneNumber":"1234567890","firstName":"Test","lastName":"User"}'
```

---

## 📊 Database Schema

### User Table

| Column | Type | Constraints |
|--------|------|-------------|
| Id | UUID | PK, V7 |
| Email | VARCHAR(256) | NOT NULL, UNIQUE |
| UserName | VARCHAR(256) | NOT NULL, UNIQUE |
| PhoneNumber | VARCHAR(20) | NOT NULL |
| FirstName | VARCHAR(256) | NOT NULL |
| LastName | VARCHAR(256) | NOT NULL |
| Status | INT | NOT NULL (1=Active, 2=Inactive, 3=Suspended) |
| CreatedAt | TIMESTAMP | NOT NULL, DEFAULT now() |
| UpdatedAt | TIMESTAMP | NOT NULL, DEFAULT now() |

### Indexes

- `Email` (UNIQUE)
- `UserName` (UNIQUE)
- `Status` (for filtering)

---

## 🔐 Domain Model

### User Entity

**Properties:**
- `Id` (Guid) - User identifier
- `Email` (string) - Unique email address
- `UserName` (string) - Unique username
- `PhoneNumber` (string) - Phone number
- `FirstName` (string) - First name
- `LastName` (string) - Last name
- `Status` (UserStatus) - Active/Inactive/Suspended
- `CreatedAt` (DateTime) - Creation timestamp
- `UpdatedAt` (DateTime) - Last update timestamp

**Methods:**
- `Create()` - Factory method for creating users
- `UpdateProfile()` - Update user information
- `Deactivate()` - Deactivate user
- `Activate()` - Activate user
- `Suspend()` - Suspend user
- `Touch()` - Update timestamp

**UserStatus Enum:**
```csharp
Active = 1
Inactive = 2
Suspended = 3
```

---

## 📝 Environment Variables

```env
# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_HTTP_PORT=8080
ASPNETCORE_GRPC_PORT=5002

# Database
ConnectionStrings__DefaultConnection=Server=postgres;Port=5432;Database=user_db;User Id=postgres;Password=postgres;

# Logging
Logging__Seq__Url=http://seq:5341
```

---

## 🧪 Testing

### Test with cURL

```bash
# Health check
curl http://localhost:8080/health

# Create user
curl -X POST http://localhost:8080/users \
  -H "Content-Type: application/json" \
  -d '{
    "email":"test@example.com",
    "userName":"testuser",
    "phoneNumber":"1234567890",
    "firstName":"Test",
    "lastName":"User"
  }'

# Get user (replace with actual ID)
curl http://localhost:8080/users/{userId}

# Update user
curl -X PUT http://localhost:8080/users/{userId} \
  -H "Content-Type: application/json" \
  -d '{
    "firstName":"Updated",
    "lastName":"Name",
    "phoneNumber":"0987654321"
  }'
```

### Test with Swagger UI

1. Open `http://localhost:8080/`
2. Explore available endpoints
3. Click "Try it out" to test endpoints
4. View responses in real-time

### Test with Postman

1. Import User Service endpoints
2. Set variable: `{{base_url}}` = `http://localhost:8080`
3. Set variable: `{{user_id}}` = User ID from create response
4. Test all endpoints

---

## 📋 Request/Response Models

### Create User Request

```json
{
  "email": "string",
  "userName": "string",
  "phoneNumber": "string",
  "firstName": "string",
  "lastName": "string"
}
```

**Validation Rules:**
- Email: Required, valid email format
- UserName: Required, 3-50 characters
- PhoneNumber: Required, at least 10 digits
- FirstName: Required, 1-50 characters
- LastName: Required, 1-50 characters

### Create User Response

```json
{
  "success": true,
  "message": "Success",
  "data": {
    "userId": "uuid"
  }
}
```

### Get User Response

```json
{
  "success": true,
  "message": "Success",
  "data": {
    "id": "uuid",
    "email": "string",
    "userName": "string",
    "phoneNumber": "string",
    "firstName": "string",
    "lastName": "string",
    "status": 1,
    "createdAt": "2026-07-09T10:00:00Z",
    "updatedAt": "2026-07-09T10:00:00Z"
  }
}
```

### Update User Request

```json
{
  "firstName": "string",
  "lastName": "string",
  "phoneNumber": "string"
}
```

**Validation Rules:**
- FirstName: Required, 1-50 characters
- LastName: Required, 1-50 characters
- PhoneNumber: Required, at least 10 digits

### Update User Response

```json
{
  "success": true,
  "message": "Success",
  "data": {
    "userId": "uuid"
  }
}
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

**Error:**
```
Access to XMLHttpRequest blocked by CORS policy
```

**Solution:**
Change CORS policy in `DependencyInjection.cs` to restricted origins if needed.

### Database connection errors

**Error:**
```
Npgsql.NpgsqlException: The server version is 'X.Y.Z'
```

**Solution:**
1. Verify PostgreSQL is running
2. Check `ConnectionStrings__DefaultConnection` is correct
3. Verify database exists and user has permissions

### Port already in use

```bash
# Find process using port 8080
lsof -i :8080

# Kill process
kill -9 <PID>

# Or use different port
ASPNETCORE_HTTP_PORT=5110 dotnet run
```

### User not found

**Error:**
```
User with ID {userId} not found
```

**Solution:**
1. Verify user ID is correct
2. Check user exists in database
3. Ensure create endpoint was called first

---

## 📚 Related Documentation

- [Service Template](../guides/SERVICE_TEMPLATE.md)
- [Development Criteria](../guides/DEVELOPMENT_CRITERIA.md)
- [Auth Service Reference](AUTH_CONFIG.md)

---

## ✅ Deployment Checklist

- [ ] All layers created and compiled
- [ ] Database migrations created and tested
- [ ] CQRS handlers implemented
- [ ] Validators in place
- [ ] Swagger documentation complete
- [ ] Health checks responding
- [ ] Environment variables configured
- [ ] Docker build successful
- [ ] Docker compose networking configured
- [ ] CORS policies configured
- [ ] Exception handling in place
- [ ] Logging to Seq working

All checks passed? ✅ **Ready for deployment!**

---

**Version:** 1.0  
**Last Updated:** 2026-07-09  
**Reference Implementation:** Auth Service
