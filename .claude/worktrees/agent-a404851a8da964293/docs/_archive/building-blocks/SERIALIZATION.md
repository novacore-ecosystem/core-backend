# Shared JSON Serialization Configuration

## Overview

`JsonSerializerConfiguration` provides a centralized, reusable JSON serialization setup for all services in the application.

## Location

`src/BuildingBlocks/BuildingBlock.SharedKernel/Serialization/JsonSerializerConfiguration.cs`

## Usage

### In Caching Service

```csharp
// Before (local configuration)
private readonly JsonSerializerOptions _jsonSerializerOptions = new()
{
    PropertyNameCaseInsensitive = true,
    WriteIndented = false
};

// After (shared configuration)
JsonSerializer.Serialize(value, JsonSerializerConfiguration.Default);
```

### In Any Service

```csharp
using BuildingBlock.SharedKernel.Serialization;
// Use the shared configuration
var json = JsonSerializer.Serialize(myObject, JsonSerializerConfiguration.Default);
var obj = JsonSerializer.Deserialize<MyType>(json, JsonSerializerConfiguration.Default);
```

### Create New Instance (if needed)

```csharp
var options = JsonSerializerConfiguration.Create();
```

## Configuration Details

The shared configuration includes:

- **PropertyNameCaseInsensitive**: true (matches JSON property names regardless of case)
- **WriteIndented**: false (compact output)
- **DefaultIgnoreCondition**: WhenWritingNull (omit null values from serialized output)

## Benefits

✅ **Consistency**: All services use the same serialization rules  
✅ **Centralized**: Single place to update JSON behavior  
✅ **Reusable**: No need to create JsonSerializerOptions in every service  
✅ **Maintainable**: Changes apply across the entire application  
✅ **Performance**: Reuses cached options instead of creating new ones

## When to Use

Use `JsonSerializerConfiguration.Default` in:

- Caching services
- API serialization
- Message queue serialization
- Database JSON fields
- Any other JSON serialization needs

## Example: Cache Service

```csharp
public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
{
    var db = _connectionMultiplexer.GetDatabase();
    var serialized = JsonSerializer.Serialize(value, JsonSerializerConfiguration.Default);

    if (expiration.HasValue)
        await db.StringSetAsync(key, serialized, expiration.Value);
    else
        await db.StringSetAsync(key, serialized);
}
```

## Extending Configuration

To add more options:

1. Update `JsonSerializerConfiguration.CreateOptions()`
2. Add the new property to `JsonSerializerOptions`
3. All services automatically get the new configuration

Example:

```csharp
private static JsonSerializerOptions CreateOptions()
{
    return new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase  // New option
    };
}
```

## Thread Safety

The shared configuration is thread-safe. `JsonSerializerOptions` is immutable and safe for concurrent use across all services.
