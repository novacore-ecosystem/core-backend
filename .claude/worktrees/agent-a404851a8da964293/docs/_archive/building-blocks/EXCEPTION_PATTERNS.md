# Domain Exception Usage Guide

Quick reference for throwing domain exceptions using `ExceptionFactory`.

## Invalid State/Status

### Invalid Entity State
```csharp
// Cannot cancel an order that is already completed
throw ExceptionFactory.InvalidState("Order", "completed", "cancel");
// Message: "Cannot cancel Order in completed state"
```

### Invalid Status Transition
```csharp
// Cannot change order status from completed to pending
throw ExceptionFactory.InvalidStatus("Order", "completed", "pending");
// Message: "Cannot change Order status from completed to pending"
```

---

## Empty Collections & Items

### Empty Collection
```csharp
// Order items list is empty
throw ExceptionFactory.EmptyCollection("Items");
// Message: "Items cannot be empty"

// Or with custom message
throw ExceptionFactory.EmptyCollection("OrderItems");
// Message: "OrderItems cannot be empty"
```

### Empty Items Required
```csharp
// Order must have at least one item
throw ExceptionFactory.EmptyItems("Order");
// Message: "Order must have at least one item"
```

---

## Not Found / Entity Lookup

### Generic Entity Not Found
```csharp
// Product with ID 123 not found (domain level)
throw ExceptionFactory.EntityNotFound("Product", 123);
// Message: "Related Product with id 123 not found"

// Using generic method
throw ExceptionFactory.EntityNotFound<Product>(123);
// Message: "Related Product with id 123 not found"
```

---

## Insufficient Amount

### Insufficient Stock/Inventory
```csharp
// Product "Laptop" has 5 in stock but 10 required
throw ExceptionFactory.InsufficientStock("Laptop", available: 5, required: 10);
// Message: "Insufficient stock: Laptop (available: 5, required: 10)"
// Details: { resource: "inventory for Laptop", available: 5, required: 10 }
```

### Insufficient Balance
```csharp
// Account has $100 but $250 required
throw ExceptionFactory.InsufficientBalance(available: 100, required: 250);
// Message: "Insufficient balance (available: 100, required: 250)"
// Details: { resource: "balance", available: 100, required: 250 }
```

### Insufficient Quota/Limit
```csharp
// Monthly upload quota exceeded
throw ExceptionFactory.InsufficientQuota("upload-quota", available: 1000, required: 5000);
// Message: "Insufficient upload-quota (available: 1000, required: 5000)"
// Details: { resource: "upload-quota", available: 1000, required: 5000 }
```

---

## Duplicate/Conflict Values

### Duplicate Field Value
```csharp
// Email already exists
throw ExceptionFactory.Duplicate("email", "john@example.com");
// Message: "email 'john@example.com' already exists"
// Rule name: "duplicate-email"

throw ExceptionFactory.Duplicate("username", "john123");
// Message: "username 'john123' already exists"
// Rule name: "duplicate-username"
```

### Unique Constraint Violation
```csharp
// More explicit with entity name
throw ExceptionFactory.UniqueConstraintViolation(
    entityName: "User",
    fieldName: "email",
    value: "john@example.com");
// Message: "A User with email 'john@example.com' already exists"
```

---

## Invalid Values

### Invalid Enum Value
```csharp
throw ExceptionFactory.InvalidEnumValue("OrderStatus", "invalid_status");
// Message: "Invalid OrderStatus: invalid_status"
```

### Invalid Range
```csharp
// Price must be between 0 and 1000000
throw ExceptionFactory.InvalidRange("price", -100, minValue: 0, maxValue: 1000000);
// Message: "price must be between 0 and 1000000"
// Details: { argument: "price", value: -100 }
```

### Value Too Small
```csharp
// Quantity must be at least 1
throw ExceptionFactory.ValueTooSmall("quantity", 0, minimumValue: 1);
// Message: "quantity must be at least 1"
```

### Value Too Large
```csharp
// Password cannot exceed 100 characters
throw ExceptionFactory.ValueTooLarge("password", 150, maximumValue: 100);
// Message: "password must not exceed 100"
```

---

## Invalid Format

### Invalid Format
```csharp
throw ExceptionFactory.InvalidFormat("email", "example@domain.com", "invalid@");
// Message: "Invalid email: 'invalid@' (expected: example@domain.com)"

// Without showing value
throw ExceptionFactory.InvalidFormat("date", "YYYY-MM-DD");
// Message: "Invalid date format (expected: YYYY-MM-DD)"
```

---

## Required Fields

### Required Field Missing
```csharp
throw ExceptionFactory.RequiredField("email");
// Message: "email is required"
```

### Required Field Not Empty
```csharp
throw ExceptionFactory.RequiredNotEmpty("productName");
// Message: "productName cannot be empty"
```

---

## Real-World Examples

### Order Entity
```csharp
public class Order
{
    public void AddItems(List<OrderItem> items)
    {
        if (!items.Any())
            throw ExceptionFactory.EmptyItems("Order");
    }

    public void Cancel()
    {
        if (Status != OrderStatus.Pending)
            throw ExceptionFactory.InvalidState("Order", Status.ToString(), "cancel");
    }

    public void ChangeStatus(OrderStatus newStatus)
    {
        if (!CanTransitionTo(newStatus))
            throw ExceptionFactory.InvalidStatus("Order", Status.ToString(), newStatus.ToString());
        Status = newStatus;
    }
}
```

### User Registration Service
```csharp
public async Task<User> RegisterAsync(RegisterCommand cmd)
{
    // Required fields
    if (string.IsNullOrWhiteSpace(cmd.Email))
        throw ExceptionFactory.RequiredField("email");

    if (string.IsNullOrWhiteSpace(cmd.Password))
        throw ExceptionFactory.RequiredNotEmpty("password");

    // Format validation
    if (!IsValidEmail(cmd.Email))
        throw ExceptionFactory.InvalidFormat("email", "user@example.com", cmd.Email);

    // Range validation
    if (cmd.Password.Length < 8)
        throw ExceptionFactory.ValueTooSmall("password", cmd.Password, minimumValue: 8);

    // Duplicate check
    var existing = await userRepository.FindByEmailAsync(cmd.Email);
    if (existing != null)
        throw ExceptionFactory.Duplicate("email", cmd.Email);

    return await CreateUserAsync(cmd);
}
```

### Product Stock Management
```csharp
public class InventoryService
{
    public void ReserveStock(Product product, int quantity)
    {
        if (product.StockQuantity < quantity)
            throw ExceptionFactory.InsufficientStock(
                product.Name,
                available: product.StockQuantity,
                required: quantity);
    }

    public void ValidateProduct(int productId)
    {
        var product = repository.Find(productId);
        if (product == null)
            throw ExceptionFactory.EntityNotFound("Product", productId);
    }
}
```

### Payment Processing
```csharp
public class PaymentService
{
    public void ProcessPayment(Account account, decimal amount)
    {
        if (account.Balance < amount)
            throw ExceptionFactory.InsufficientBalance(
                available: account.Balance,
                required: amount);
    }

    public void ValidatePaymentMethod(PaymentMethod method)
    {
        if (!IsValidPaymentMethod(method))
            throw ExceptionFactory.InvalidEnumValue("PaymentMethod", method.ToString());
    }
}
```

---

## Exception Response Examples

### Invalid State Response
```json
{
  "success": false,
  "message": "Cannot cancel Order in completed state",
  "messageCode": "102",
  "data": null,
  "details": {
    "entity": "Order",
    "currentState": "completed",
    "attemptedAction": "cancel"
  }
}
```

### Insufficient Stock Response
```json
{
  "success": false,
  "message": "Insufficient stock: Laptop (available: 5, required: 10)",
  "messageCode": "551",
  "data": null,
  "details": {
    "resource": "inventory for Laptop",
    "available": 5,
    "required": 10
  }
}
```

### Duplicate Email Response
```json
{
  "success": false,
  "message": "email 'john@example.com' already exists",
  "messageCode": "400",
  "data": null,
  "details": {
    "rule": "duplicate-email"
  }
}
```

---

## Summary

| Scenario | Factory Method | Usage |
|----------|---|---|
| Invalid state | `InvalidState(entity, state, action)` | Entity not in correct state for operation |
| Invalid status | `InvalidStatus(entity, current, invalid)` | Cannot transition to status |
| Empty collection | `EmptyCollection(name)` | Collection required but empty |
| Empty items | `EmptyItems(entity)` | Entity requires items |
| Entity not found | `EntityNotFound(entity, id)` | Related entity missing |
| Insufficient stock | `InsufficientStock(name, avail, req)` | Not enough inventory |
| Insufficient balance | `InsufficientBalance(avail, req)` | Not enough balance |
| Insufficient quota | `InsufficientQuota(quota, avail, req)` | Quota exceeded |
| Duplicate value | `Duplicate(field, value)` | Value already exists |
| Unique constraint | `UniqueConstraintViolation(entity, field, value)` | Explicit constraint violation |
| Invalid enum | `InvalidEnumValue(enum, value)` | Invalid enum value |
| Invalid range | `InvalidRange(field, value, min, max)` | Value outside range |
| Value too small | `ValueTooSmall(field, value, min)` | Below minimum |
| Value too large | `ValueTooLarge(field, value, max)` | Above maximum |
| Invalid format | `InvalidFormat(field, format, value?)` | Format mismatch |
| Required field | `RequiredField(field)` | Field is required |
| Required not empty | `RequiredNotEmpty(field)` | Field cannot be empty |
