# Development Criteria & Process Guidelines

**Last Updated:** 2026-07-09  
**Purpose:** Establish consistent, measurable criteria for service development quality  
**Applies to:** All microservice development in NovaCore

---

## 🎯 Core Quality Criteria

### 1. Architecture Compliance

#### Domain Layer
- ✅ All entities implement `IEntity` interface
- ✅ Factory methods (`Create()`) for entity construction
- ✅ Entities are immutable (private setters)
- ✅ Business logic methods (no anemic entities)
- ✅ Enum definitions for status, types, etc.
- ✅ No direct DbContext references in domain

**Verification:**
```bash
# Check for IEntity implementation
grep -r "IEntity" src/Services/[Service]/[Service].Domain/

# Check for private setters
grep -r "public.*{.*get.*set" src/Services/[Service]/[Service].Domain/
```

#### Persistence Layer
- ✅ DbContext inherits from `DbContext`
- ✅ Entity configurations via `IEntityTypeConfiguration<T>`
- ✅ All entities configured (no implicit conversions)
- ✅ Indexes on frequently queried columns
- ✅ Foreign keys properly configured
- ✅ Seeders for development data
- ✅ Migrations committed to repo

**Verification:**
```bash
# Check DbContext structure
grep -r "DbSet<" src/Services/[Service]/[Service].Persistence/

# Check for ApplyConfigurationsFromAssembly
grep -r "ApplyConfigurationsFromAssembly" src/Services/[Service]/[Service].Persistence/
```

#### Application Layer
- ✅ CQRS pattern (Commands/Queries separated)
- ✅ All handlers implement `IRequestHandler<T, R>`
- ✅ Validators for all commands/queries
- ✅ Mapster configurations for DTOs
- ✅ MediatR pipeline behaviors applied
- ✅ No domain entity leaking to endpoints

**Verification:**
```bash
# Check for handlers
find src/Services/[Service]/[Service].Application -name "*Handler.cs"

# Check for validators
find src/Services/[Service]/[Service].Application -name "*Validator.cs"
```

#### Infrastructure Layer
- ✅ No direct references from Application
- ✅ All implementations registered via DI
- ✅ Marker interfaces for auto-discovery
- ✅ Extension methods for clean DI API
- ✅ Service implementations sealed (unless inherited)

**Verification:**
```bash
# Check for marker interface implementations
grep -r "IMarker" src/Services/[Service]/[Service].Infrastructure/

# Check no Application references Infrastructure
grep -r "Infrastructure" src/Services/[Service]/[Service].Application/
```

#### API Layer
- ✅ Carter endpoints for routing
- ✅ OpenAPI/Swagger documentation
- ✅ Health checks configured
- ✅ CORS policies applied
- ✅ Exception handling middleware
- ✅ Proper request/response models

**Verification:**
```bash
# Check for Carter modules
grep -r "ICarterModule" src/Services/[Service]/[Service].API/

# Check Swagger setup
grep -r "AddSwaggerGen" src/Services/[Service]/[Service].API/
```

---

### 2. Code Quality Standards

#### C# Style Compliance
- ✅ File-scoped namespaces (not block-scoped)
- ✅ Sealed classes by default
- ✅ Primary constructors for single-constructor classes
- ✅ No unnecessary backing fields
- ✅ No abbreviations in names (`repo` → `repository`)
- ✅ Proper indentation (4 spaces, no tabs)

**Verification:**
```bash
# Check for block-scoped namespaces (should be 0 matches)
grep -r "^namespace.*{$" src/Services/[Service]/

# Check for abbreviated names
grep -r "\b[a-z]{2,3}[A-Z]" src/Services/[Service]/ | grep -v "IEntity"
```

#### Documentation Standards
- ✅ No unnecessary comments (code is self-documenting)
- ✅ XML comments only for public API surfaces
- ✅ Method names clearly express intent
- ✅ Complex logic explained with "WHY" comments

**Verification:**
```bash
# Check for excessive comments
find src/Services/[Service] -name "*.cs" -exec grep -l "^.*//.*comment" {} \;
```

#### Performance Standards
- ✅ `AsNoTracking()` for read-only queries
- ✅ `AsSplitQuery()` for multiple collections
- ✅ Projection via Mapster instead of loading full entities
- ✅ No N+1 queries (eager load relationships)
- ✅ Minimal allocations in hot paths

**Verification:**
```bash
# Check for AsNoTracking usage
grep -r "AsNoTracking" src/Services/[Service]/[Service].Persistence/

# Check for projection
grep -r "ProjectToType" src/Services/[Service]/[Service].Application/
```

---

### 3. Data Access Patterns

#### Repository Pattern
- ✅ Repository for each aggregate root
- ✅ All queries through repositories
- ✅ No direct DbContext access from application
- ✅ Consistent method naming (Get, Add, Update, Remove)
- ✅ Cancellation token support in all async methods

**Verification:**
```bash
# Check for repository implementations
find src/Services/[Service] -name "*Repository.cs"

# Check for CancellationToken in async methods
grep -r "async.*CancellationToken" src/Services/[Service]/
```

#### Entity Framework Usage
- ✅ Configurations in separate config files
- ✅ DbContext models explicitly configured
- ✅ No shadow properties
- ✅ Proper value conversions
- ✅ Migrations tracked in version control

**Verification:**
```bash
# Check for OnModelCreating configurations
grep -r "IEntityTypeConfiguration" src/Services/[Service]/[Service].Persistence/

# Check migrations exist
ls -la src/Services/[Service]/[Service].Persistence/Migrations/
```

---

### 4. API Endpoint Standards

#### Carter Endpoints
- ✅ Request model as record
- ✅ Response wrapped in `ApiResponse<T>`
- ✅ Proper HTTP status codes (201 for Created, etc.)
- ✅ OpenAPI documentation with `WithOpenApi()`
- ✅ Authorization attributes applied
- ✅ Input validation via validators

**Verification:**
```bash
# Check for proper response wrapping
grep -r "ApiResponse<" src/Services/[Service]/[Service].API/

# Check for WithOpenApi()
grep -r "WithOpenApi" src/Services/[Service]/[Service].API/
```

#### Request Validation
- ✅ All request models have validators
- ✅ Email format validation
- ✅ Required field validation
- ✅ String length constraints
- ✅ Business rule validation

**Verification:**
```bash
# List all endpoint request models
find src/Services/[Service]/[Service].API/Endpoints -name "*.cs" -exec grep -l "record.*Request" {} \;

# Verify validators exist for each
find src/Services/[Service]/[Service].Application -name "*Validator.cs" | wc -l
```

---

### 5. Security & Configuration

#### Environment Variables
- ✅ No hardcoded secrets
- ✅ All config from environment
- ✅ `.env` file for local development
- ✅ `docker-compose.yml` uses `${VARIABLE}` syntax
- ✅ Sensitive values excluded from git

**Verification:**
```bash
# Check for hardcoded connection strings
grep -r "Server=\|Host=" src/Services/[Service]/ --include="*.cs"

# Check docker-compose uses variables
grep -E "['\"]Server=" docker-compose.yml
```

#### JWT & Authentication
- ✅ JWT bearer token support in Swagger
- ✅ Protected endpoints require auth
- ✅ CORS policies configured
- ✅ Credential handling via HTTP-only cookies (if applicable)
- ✅ Exception handling for auth errors

**Verification:**
```bash
# Check JWT configuration
grep -r "Bearer" src/Services/[Service]/[Service].API/

# Check endpoint authorization
grep -r "RequireAuthorization\|AllowAnonymous" src/Services/[Service]/[Service].API/
```

---

### 6. Error Handling & Logging

#### Exception Architecture
- ✅ Custom domain exceptions for business rules
- ✅ All exceptions handled globally
- ✅ Proper logging with context
- ✅ User-friendly error responses
- ✅ Exception info in structured logs

**Verification:**
```bash
# Check for exception handlers
grep -r "ExceptionHandler\|try.*catch" src/Services/[Service]/

# Check structured logging
grep -r "logger.Log" src/Services/[Service]/ | head -5
```

#### Validation Errors
- ✅ Fluent validation on all commands
- ✅ Validation errors returned with 400 Bad Request
- ✅ Error messages are user-friendly
- ✅ Field-level error details provided

**Verification:**
```bash
# Check validators registered
grep -r "AddValidatorsFromAssembly" src/Services/[Service]/

# Check error response format
grep -r "ValidationFailure\|BadRequest" src/Services/[Service]/
```

---

### 7. Documentation Requirements

#### Service Documentation
- ✅ Service overview (purpose, responsibilities)
- ✅ Configuration guide (env vars, settings)
- ✅ API endpoints with examples (cURL, Postman)
- ✅ Database schema explanation
- ✅ Running the service (local, Docker)
- ✅ Troubleshooting section

**Verification:**
```bash
# Check service doc exists
ls docs/services/[SERVICE_NAME].md

# Check doc covers endpoints
grep -c "POST\|GET\|PUT\|DELETE" docs/services/[SERVICE_NAME].md
```

#### Code Documentation
- ✅ Public APIs have XML comments
- ✅ Complex logic has explanatory comments
- ✅ Database schema in configs is clear
- ✅ CQRS handler intent is obvious
- ✅ Extension methods are self-explanatory

**Verification:**
```bash
# Check for public API documentation
grep -r "^.*public.*{$" src/Services/[Service]/ | wc -l
grep -r "/// " src/Services/[Service]/ | wc -l  # Should be significant
```

---

### 8. Testing Standards

#### Unit Tests
- ✅ Validators tested with valid/invalid inputs
- ✅ Command handlers tested in isolation
- ✅ Domain entity factory methods tested
- ✅ Edge cases covered

#### Integration Tests
- ✅ Database interactions tested
- ✅ API endpoints tested with payloads
- ✅ Error scenarios tested
- ✅ Health checks verified

**Verification:**
```bash
# Check test projects exist
ls src/Services/[Service]/[Service].Tests/

# Check test coverage
find src/Services/[Service]/[Service].Tests -name "*.cs" | wc -l
```

---

### 9. Deployment Readiness

#### Docker Configuration
- ✅ Multi-stage Dockerfile
- ✅ Non-root user in container
- ✅ Health checks configured
- ✅ Ports exposed correctly
- ✅ Environment variables passed

**Verification:**
```bash
# Check Dockerfile
cat src/Services/[Service]/Dockerfile

# Check docker-compose health check
grep -A 5 "healthcheck:" docker-compose.yml
```

#### Database Migrations
- ✅ Migrations created and tested
- ✅ Rollback-safe migrations
- ✅ Naming follows convention: `YYYYMMDDHHMMSS_Description`
- ✅ Migrations in version control

**Verification:**
```bash
# Check migrations
ls -la src/Services/[Service]/[Service].Persistence/Migrations/

# Check migration names
ls src/Services/[Service]/[Service].Persistence/Migrations/ | grep "^20[0-9]"
```

---

## 📊 Development Workflow

### Phase 1: Planning (Before coding)
- [ ] Read `SERVICE_TEMPLATE.md`
- [ ] Identify domain entities and bounded context
- [ ] Sketch CQRS command/query structure
- [ ] Plan database schema and relationships
- [ ] List required external dependencies

### Phase 2: Domain (First layer)
- [ ] Define entities with factory methods
- [ ] Create value objects as needed
- [ ] Define enums for status/types
- [ ] Implement business logic methods
- [ ] Write domain tests

### Phase 3: Persistence (Second layer)
- [ ] Create DbContext
- [ ] Write entity configurations
- [ ] Create seeders for development
- [ ] Generate migrations
- [ ] Test database operations

### Phase 4: Application (Third layer)
- [ ] Define CQRS commands/queries
- [ ] Implement handlers
- [ ] Create validators
- [ ] Setup Mapster configurations
- [ ] Write handler tests

### Phase 5: Infrastructure (Fourth layer)
- [ ] Implement domain services
- [ ] Setup caching (if needed)
- [ ] Configure security services
- [ ] Setup background jobs (if needed)
- [ ] Register via DI

### Phase 6: API (Fifth layer)
- [ ] Create Carter endpoints
- [ ] Setup Swagger documentation
- [ ] Configure CORS and auth
- [ ] Add health checks
- [ ] Write endpoint tests

### Phase 7: Configuration & Docs
- [ ] Add environment variables
- [ ] Update docker-compose.yml
- [ ] Create service documentation
- [ ] Add code documentation
- [ ] Final testing

---

## 🔍 Code Review Checklist

Before submitting PR, verify:

### Architecture
- [ ] All 5 layers present
- [ ] No cross-layer violations
- [ ] DI properly configured
- [ ] No circular dependencies

### Code Quality
- [ ] File-scoped namespaces
- [ ] Sealed classes
- [ ] Primary constructors used
- [ ] No abbreviations
- [ ] Consistent formatting

### Functionality
- [ ] CQRS handlers implemented
- [ ] Validators created and working
- [ ] Endpoints functional
- [ ] Migrations created
- [ ] Health checks respond

### Testing
- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Manual testing done
- [ ] Edge cases verified

### Documentation
- [ ] Service doc written
- [ ] Code is self-documenting
- [ ] Complex logic has "WHY" comments
- [ ] README updated if needed

---

## 🚀 Performance Benchmarks

Target metrics for production readiness:

| Metric | Target | Method |
|--------|--------|--------|
| API Response Time | < 200ms | Load test with 100 concurrent users |
| Database Query | < 50ms | EXPLAIN ANALYZE on migrations |
| Memory Usage | < 256MB | Docker stats |
| Startup Time | < 5s | Time from container start to health check pass |
| Error Rate | < 0.1% | Monitor in production |

---

## 📋 Pre-Deployment Checklist

```
Architecture & Layers
  [ ] All 5 layers implemented
  [ ] No cross-layer dependencies
  [ ] DI registration complete
  [ ] No BuildingBlocks violations

Code Quality
  [ ] Passes code-review
  [ ] Follows C# standards
  [ ] No hardcoded secrets
  [ ] No TODO comments left

Functionality
  [ ] CQRS handlers working
  [ ] Validators in place
  [ ] Endpoints returning correct responses
  [ ] Migrations tested
  [ ] Seeders working

Security
  [ ] JWT authentication configured
  [ ] CORS properly restricted
  [ ] Environment variables used
  [ ] No secrets in code

Documentation
  [ ] Service documentation complete
  [ ] API endpoints documented
  [ ] Configuration documented
  [ ] Troubleshooting guide included

Database
  [ ] Migrations committed
  [ ] Indexes created on key fields
  [ ] Foreign keys configured
  [ ] Seeders provide test data

Testing
  [ ] Unit tests pass
  [ ] Integration tests pass
  [ ] Manual testing completed
  [ ] Edge cases verified

Deployment
  [ ] Dockerfile builds successfully
  [ ] Docker-compose configuration updated
  [ ] Environment variables set
  [ ] Health check responds

  ✅ Ready for deployment
```

---

## 🔗 Related Documentation

- [Service Template](SERVICE_TEMPLATE.md)
- [Exception Handling Guide](../building-blocks/EXCEPTIONS.md)
- [Auth Service Reference](../services/AUTH_CONFIG.md)

---

**Version:** 1.0  
**Last Updated:** 2026-07-09  
**Maintained by:** Development Team
