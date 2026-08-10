# Exception Architecture - Detailed Guide

## Overview

The exception system is organized into **two distinct layers** to support clean architecture and proper separation of concerns:

1. **Domain Layer Exceptions** - Business rules and constraints
2. **Application Layer Exceptions** - HTTP/API concerns

---

## Domain Layer Exceptions

Located in: `BuildingBlock.Domain.Exceptions`

### Purpose
- Represent business rule violations
- Thrown by domain entities and domain services
- Independent of HTTP/API concerns
- Can be used in non-HTTP contexts (background jobs, console apps, etc.)

### Base Class: DomainException
```csharp
public abstract class DomainException : Exception
{
    public MessageCode MessageCode { get; }      // Enum for translation
    public string? SystemMessage { get; }        // Detailed logging
}
```

### Available Domain Exceptions

#### 1. InvalidArgumentException
For invalid method parameters or domain validation.

```csharp
// In an entity or domain service
public class Order
{
    public void AddQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new InvalidArgumentException("quantity", quantity, 
                "Quantity must be greater than zero");
    }
}
```

#### 2. BusinessRuleException
For business logic violations.

```csharp
// In an entity
public class User
{
    public void SetEmail(string email)
    {
        if (email == this.Email)
            throw new BusinessRuleException("email-unchanged",
                "New email must be different from current email");
    }
}
```

#### 3. InsufficientAmountException
For stock, balance, or resource constraints.

```csharp
// In domain service
public class InventoryService
{
    public void Reserve(string productId, int quantity)
    {
        var available = GetAvailableQuantity(productId);
        if (available < quantity)
            throw new InsufficientAmountException("inventory",
                available: available, required: quantity);
    }
}
```

#### 4. AuthException
For authentication/authorization domain logic.

```csharp
// In domain service
public class UserAuthService
{
    public User Authenticate(string username, string password)
    {
        var user = repository.FindByUsername(username);
        if (!user.IsActive)
            throw new AuthException(MessageCode.AccountLocked,
                "User account is locked by administrator");
        // ... more checks
    }
}
```

---

## Application Layer Exceptions

Located in: `BuildingBlock.Application.Exceptions`

### Purpose
- Represent HTTP/API-level concerns
- Include HTTP status codes
- Caught/thrown by application services and endpoints
- Converted to API responses by GlobalExceptionHandler

### Base Class: ApplicationException
```csharp
public abstract class ApplicationException : Exception
{
    public MessageCode MessageCode { get; }      // Enum for translation
    public string? SystemMessage { get; }        // Detailed logging
    public int StatusCode { get; }               // HTTP status code (400, 404, etc.)
}
```

### Available Application Exceptions

#### 1. ValidationException (HTTP 400)
For form/request validation errors.

```csharp
public class CreateUserService
{
    public async Task<User> CreateAsync(CreateUserRequest request)
    {
        var errors = new List<ValidationError>();
        
        if (string.IsNullOrWhiteSpace(request.Email))
            errors.Add(new("Email", "Email is required"));
        
        if (request.Password.Length < 8)
            errors.Add(new("Password", "Password must be at least 8 characters"));
        
        if (errors.Any())
            throw new ValidationException(errors);
        
        // ... create user
    }
}
```

#### 2. NotFoundException (HTTP 404)
For missing resources.

```csharp
public async Task<Product> GetProductAsync(int id)
{
    var product = await repository.FindAsync(id);
    if (product == null)
        throw new NotFoundException("Product", id);  // Auto-generates message
    return product;
}
```

#### 3. BadRequestException (HTTP 400)
For general bad requests.

```csharp
if (request.Price < 0)
    throw new BadRequestException("Price cannot be negative");
```

#### 4. UnauthorizedException (HTTP 401)
For unauthenticated requests.

```csharp
if (user == null)
    throw new UnauthorizedException("User credentials are invalid");
```

#### 5. ForbiddenException (HTTP 403)
For insufficient permissions.

```csharp
if (!user.HasRole("admin"))
    throw new ForbiddenException("Only administrators can access this resource");
```

#### 6. ConflictException (HTTP 409)
For resource conflicts or invalid state transitions.

```csharp
if (order.Status != "pending")
    throw new ConflictException("Cannot cancel an order that is already processing");
```

---

## Exception Flow & Handling

### Flow Diagram

```
┌─────────────────┐
│  Domain Layer   │
│  (Entities)     │
└────────┬────────┘
         │
         ├─→ Throws: InvalidArgumentException
         │                BusinessRuleException
         │                InsufficientAmountException
         │                AuthException
         │
┌────────▼────────────────┐
│ Application Layer       │
│ (Application Services)  │
└────────┬────────────────┘
         │
         ├─→ Catches domain exceptions (optional)
         │
         ├─→ Throws: ValidationException
         │           NotFoundException
         │           BadRequestException
         │           UnauthorizedException
         │           ForbiddenException
         │           ConflictException
         │
         ├─→ Lets domain exceptions bubble
         │
┌────────▼──────────────────────────────┐
│ HTTP Layer (GlobalExceptionHandler)   │
│ (Middleware)                          │
└────────┬──────────────────────────────┘
         │
         ├─→ Catches ANY exception
         │
         ├─→ Calls: ExceptionHandlerHelper.HandleException()
         │
         ├─→ Converts to: ApiResponse<T>
         │
         └─→ Returns appropriate HTTP status
```

### Pattern 1: Domain Exception Propagates

```csharp
// Domain Layer (Product.cs)
public class Product
{
    public static Product Create(string name, decimal price)
    {
        if (price < 0)
            throw new InvalidArgumentException("price", price);  // Domain level
        return new Product { Name = name, Price = price };
    }
}

// Application Layer (CreateProductService.cs)
public class CreateProductService
{
    public async Task<Product> ExecuteAsync(CreateProductCommand cmd)
    {
        // Domain exception bubbles up (not caught here)
        var product = Product.Create(cmd.Name, cmd.Price);  // May throw
        await repository.AddAsync(product);
        return product;
    }
}

// HTTP Layer (GlobalExceptionHandler)
// Catches InvalidArgumentException → 400 Bad Request response
```

**Response:**
```json
{
  "success": false,
  "message": "Invalid argument: price = '-100'",
  "messageCode": "102",
  "data": null,
  "details": {
    "argument": "price",
    "value": -100
  }
}
```

### Pattern 2: Domain Exception Caught & Converted

```csharp
// Application Layer (PlaceOrderService.cs)
public class PlaceOrderService
{
    public async Task<Order> ExecuteAsync(PlaceOrderCommand cmd)
    {
        try
        {
            var order = Order.Create(cmd.Items);
            order.Reserve(inventoryService);  // May throw InsufficientAmountException
            await repository.AddAsync(order);
            return order;
        }
        catch (InsufficientAmountException domainEx)
        {
            // Convert to application exception with API context
            throw new ConflictException(
                "Cannot place order: insufficient inventory",
                systemMessage: domainEx.SystemMessage);
        }
    }
}

// HTTP Layer (GlobalExceptionHandler)
// Catches ConflictException → 409 Conflict response
```

### Pattern 3: Application Layer Validation

```csharp
// Application Layer (CreateProductService.cs)
public class CreateProductService
{
    public async Task<Product> ExecuteAsync(CreateProductCommand cmd)
    {
        // Request validation at application layer
        var errors = ValidateRequest(cmd);
        if (errors.Any())
            throw new ValidationException(errors, "Request validation failed");
        
        // Domain logic
        var product = Product.Create(cmd.Name, cmd.Price);
        await repository.AddAsync(product);
        return product;
    }
    
    private List<ValidationError> ValidateRequest(CreateProductCommand cmd)
    {
        var errors = new List<ValidationError>();
        if (string.IsNullOrWhiteSpace(cmd.Name))
            errors.Add(new("Name", "Product name is required"));
        if (cmd.Price < 0)
            errors.Add(new("Price", "Price cannot be negative"));
        return errors;
    }
}

// HTTP Layer (GlobalExceptionHandler)
// Catches ValidationException → 400 Bad Request response
```

**Response:**
```json
{
  "success": false,
  "message": "Validation failed",
  "messageCode": "101",
  "data": null,
  "details": {
    "errors": [
      { "property": "Name", "message": "Product name is required" },
      { "property": "Price", "message": "Price cannot be negative" }
    ]
  }
}
```

---

## Response Examples

### Domain Exception Response
```json
{
  "success": false,
  "message": "Insufficient inventory: available 5, required 10",
  "messageCode": "551",
  "data": null,
  "details": {
    "resource": "inventory",
    "available": 5,
    "required": 10
  }
}
```

### Application Validation Response
```json
{
  "success": false,
  "message": "Validation failed",
  "messageCode": "101",
  "data": null,
  "details": {
    "errors": [
      { "property": "Email", "message": "Invalid email format" },
      { "property": "Password", "message": "Too weak" }
    ]
  }
}
```

### Not Found Response
```json
{
  "success": false,
  "message": "The Product (123) is not found.",
  "messageCode": "602",
  "data": null,
  "details": {
    "entity": "Product",
    "value": 123
  }
}
```

---

## Best Practices

### ✅ DO

1. **Throw domain exceptions from domain logic**
   ```csharp
   // In domain entity
   if (!IsValidPrice(price))
       throw new InvalidArgumentException("price", price);
   ```

2. **Throw application exceptions from services/endpoints**
   ```csharp
   // In application service
   if (request.Email == null)
       throw new ValidationException(errors);
   ```

3. **Let domain exceptions bubble up**
   ```csharp
   // Domain exception propagates to HTTP handler
   var product = Product.Create(cmd.Name, cmd.Price);
   ```

4. **Use specific exception types**
   ```csharp
   throw new InsufficientAmountException("inventory", 5, 10);
   throw new BusinessRuleException("duplicate-email");
   ```

### ❌ DON'T

1. **Throw generic exceptions**
   ```csharp
   // Bad
   throw new Exception("Price is invalid");
   
   // Good
   throw new InvalidArgumentException("price", -100);
   ```

2. **Catch domain exceptions unnecessarily**
   ```csharp
   // Bad - swallows domain exception
   try {
       var product = Product.Create(name, price);
   }
   catch (Exception) { }
   
   // Good - let it bubble
   var product = Product.Create(name, price);
   ```

3. **Mix HTTP and domain concerns**
   ```csharp
   // Bad - domain knows about HTTP
   throw new UnauthorizedException();  // From domain layer
   
   // Good - domain throws domain exception
   throw new AuthException(MessageCode.InvalidCredentials);
   ```

4. **Create custom exception types for every case**
   ```csharp
   // Bad - too many types
   throw new InvalidEmailException();
   throw new InvalidPhoneException();
   
   // Good - use specific types
   throw new InvalidArgumentException("email", email);
   throw new InvalidArgumentException("phone", phone);
   ```

---

## Summary

| Layer | Exceptions | Purpose | HTTP Status |
|-------|-----------|---------|-------------|
| **Domain** | `InvalidArgumentException`, `BusinessRuleException`, `InsufficientAmountException`, `AuthException` | Business rules, constraints | Varies (handled at HTTP layer) |
| **Application** | `ValidationException`, `NotFoundException`, `BadRequestException`, `UnauthorizedException`, `ForbiddenException`, `ConflictException` | API concerns, HTTP semantics | 400, 404, 401, 403, 409 |
| **HTTP** | GlobalExceptionHandler | Convert all exceptions to JSON responses | Various |

---

## Decision Tree

```
Is it a domain rule?
├─ YES → Use ExceptionFactory (see EXCEPTION_PATTERNS.md)
│        ├─ State/Status? → InvalidState / InvalidStatus
│        ├─ Empty? → EmptyItems / EmptyCollection
│        ├─ Related entity missing? → EntityNotFound
│        ├─ Insufficient amount? → InsufficientStock / Balance / Quota
│        ├─ Duplicate value? → Duplicate / UniqueConstraintViolation
│        └─ Invalid value? → InvalidEnumValue / InvalidRange / etc.
│
└─ NO → Use ApplicationException
   ├─ Form validation? → ValidationException
   ├─ API resource not found? → NotFoundException
   ├─ Generic bad request? → BadRequestException
   ├─ Not authenticated? → UnauthorizedException
   ├─ No permission? → ForbiddenException
   └─ State conflict? → ConflictException
```

---

## Setting Up GlobalExceptionHandler in a New Service

Each service implements its own `GlobalExceptionHandler`, which delegates the actual mapping to the shared `ExceptionHandlerHelper` in `BuildingBlock.Infrastructure`:

```csharp
// {Service}.Infrastructure/ExceptionHandling/GlobalExceptionHandler.cs
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Infrastructure.ExceptionHandling;
using Microsoft.AspNetCore.Diagnostics;

namespace YourService.Infrastructure.ExceptionHandling;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IAppLogger appLogger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        appLogger.LogError(exception.Message, exception);

        var (statusCode, response) = ExceptionHandlerHelper.HandleException(exception);

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }
}
```

Register in `Program.cs`:

```csharp
builder.Services.AddExceptionHandler<YourService.Infrastructure.ExceptionHandling.GlobalExceptionHandler>();

var app = builder.Build();
app.UseExceptionHandler();
```

---

## Creating a New Exception

If none of the existing exceptions fit, add one — but update `ExceptionHandlerHelper` in the same change, or the new type won't map to the right HTTP status.

### Application Layer (HTTP concerns)

```csharp
using BuildingBlock.Application.Exceptions;

namespace Auth.Application.Exceptions;

public sealed class TokenExpiredException(string? systemMessage = null)
    : ApplicationException(MessageCodeEnum.TokenExpired, systemMessage, statusCode: 401);
```

### Domain Layer (business logic)

```csharp
using BuildingBlock.Domain.Exceptions;

namespace Auth.Domain.Exceptions;

public sealed class InvalidRefreshTokenException(string message = "The refresh token is invalid")
    : DomainException(message);
```

### Maintenance Checklist

1. Add the exception class in the correct layer (`Application.Exceptions` or `Domain.Exceptions`), inheriting the right base class
2. Add a `MessageCode` enum value in `BuildingBlock.Domain/Enums/MessageCode.cs`
3. **Application exceptions**: no `ExceptionHandlerHelper` change needed — the `StatusCode` on the exception itself is used automatically
4. **Domain exceptions**: add a case to the switch in `ExceptionHandlerHelper.HandleDomainException()` mapping the new type to an HTTP status — this is the step people forget
5. Add a unit test asserting the handler throws/maps the new exception correctly

`ExceptionHandlerHelper` location: `src/BuildingBlocks/BuildingBlock.Infrastructure/ExceptionHandling/ExceptionHandlerHelper.cs`

---

## History

The exception system replaced ad-hoc `throw new Exception(...)` calls and an earlier `ApiResponse<T>.Errors` property (removed in favor of the more general `Details` object, which can carry validation errors, entity lookup info, or resource constraints depending on the exception type).
