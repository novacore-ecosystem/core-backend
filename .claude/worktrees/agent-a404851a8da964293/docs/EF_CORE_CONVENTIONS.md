# EF Core Configuration Conventions

This document defines the standards for Entity Framework Core configurations across all services in the SmartCommerce solution.

## Overview

EF Core configurations serve two purposes:
1. **Technical**: Define the mapping between domain entities and database schema
2. **Documentation**: Make the database schema self-documenting through explicit configuration

The guiding principle is: **Configuration should describe the schema clearly without restating EF Core defaults.**

---

## Value Object Mapping

### Single-Value Value Objects

Use `HasConversion()` for Value Objects that wrap a single primitive value.

**Pattern:**
```csharp
builder.Property(x => x.Sku)
    .HasConversion(x => x.Value, x => Sku.Create(x))
    .HasMaxLength(100)
    .IsRequired();
```

**Examples of single-value Value Objects:**
- Sku
- Quantity
- Price
- Email
- Phone
- Money
- ProductCode
- CurrencyCode
- Percentage
- TaxRate

**Rationale:**
- Simpler mappings
- More convenient in queries (direct column reference)
- Reduces nesting complexity
- Behaves like a strongly-typed primitive from EF's perspective

### Multi-Value Value Objects

Use `OwnsOne()` only when the Value Object groups multiple persisted fields into a richer domain concept.

**Pattern:**
```csharp
builder.OwnsOne(x => x.Address);

// Or with explicit nested configuration if needed:
builder.OwnsOne(x => x.Address, address =>
{
    address.Property(a => a.Street).HasMaxLength(255).IsRequired();
    address.Property(a => a.City).HasMaxLength(100).IsRequired();
    // ... other properties
});
```

**Examples of multi-value Value Objects:**
- Address
- GeoLocation
- ContactInfo
- WarehouseCapacity
- TemperatureRange
- HumidityRange
- PaymentInformation
- CustomerInformation

**Rationale:**
- Represents a domain concept composed of related fields
- Keeps related data grouped logically
- Allows encapsulation of validation rules
- Clearer intent than individual scalar columns

---

## Numeric Types

### Integer/Bigint/Smallint

**Do not** explicitly configure SQL types for standard numeric properties:

**❌ Avoid:**
```csharp
builder.Property(x => x.Priority)
    .HasColumnType("integer");

builder.Property(x => x.Status)
    .HasColumnType("smallint")
    .HasConversion<short>();
```

**✅ Correct:**
```csharp
builder.Property(x => x.Priority);

builder.Property(x => x.Status)
    .HasConversion<short>();  // Only the conversion hint needed
```

EF Core correctly infers:
- `int` → `integer` (or database equivalent)
- `short` → `smallint` (or database equivalent)
- `long` → `bigint` (or database equivalent)

### Decimal

**Always** explicitly configure precision for decimal values:

**✅ Correct:**
```csharp
builder.Property(x => x.MaxWeight)
    .HasPrecision(10, 2);  // 10 digits total, 2 decimal places

builder.Property(x => x.Latitude)
    .HasPrecision(10, 8);  // Geographic precision
```

Decimal is the critical exception because:
- Precision varies by business domain
- Different databases have different defaults
- Prevents data loss or rounding errors

### Guid

**Do not** manually configure UUID/GUID column types:

**❌ Avoid:**
```csharp
builder.Property(x => x.WarehouseId)
    .HasColumnType("uuid");
```

**✅ Correct:**
```csharp
builder.Property(x => x.WarehouseId).IsRequired();
```

EF Core correctly infers `Guid` → provider-specific UUID type (e.g., PostgreSQL's `uuid`).

---

## String Types

### String Length Configuration

Always configure `HasMaxLength()` for string properties:

**✅ Correct:**
```csharp
builder.Property(x => x.Code).HasMaxLength(50);
builder.Property(x => x.Name).HasMaxLength(200);
builder.Property(x => x.Description).HasMaxLength(1000);
```

### SQL Type Specification

**Do not** specify SQL types like `character varying` unless intentionally changing storage behavior:

**❌ Avoid:**
```csharp
builder.Property(x => x.Name)
    .HasColumnType("character varying")
    .HasMaxLength(200);
```

**✅ Correct:**
```csharp
builder.Property(x => x.Name).HasMaxLength(200);
```

**Exception - Use SQL types for TEXT fields:**

```csharp
builder.Property(x => x.Metadata)
    .HasColumnType("jsonb")
    .IsRequired();

builder.Property(x => x.LargePayload)
    .HasColumnType("text");  // Explicitly unlimited
```

### Recommended String Lengths

Use these lengths as defaults for common scenarios:
- Code fields: 50
- Name fields: 100–200
- Email: 255
- URL: 2000
- Description: 500–1000
- Short comments: 500
- Long text: 2000–4000
- Unlimited: `text` column type

---

## Default Values

### When to Use Defaults

Configure `HasDefaultValue()` for fields where database-level defaults make sense:

**✅ Correct:**
```csharp
builder.Property(x => x.Status)
    .HasConversion<short>()
    .HasDefaultValue(InventoryStatus.Active);

builder.Property(x => x.Priority)
    .HasDefaultValue(0);

builder.Property(x => x.IsActive)
    .HasDefaultValue(true);

builder.Property(x => x.Notes)
    .HasDefaultValue(string.Empty);
```

### Purpose of Explicit Defaults

**Not** for technical necessity (the application should always provide values).

**For:**
- Schema clarity: Someone reading the migration knows the default
- Legacy data imports: Scripts can rely on documented defaults
- Raw SQL queries: Default behavior is obvious
- Debugging: Database directly reflects domain defaults

---

## Index Configuration

### Do Not Use Manual Index Names

**❌ Avoid:**
```csharp
builder.HasIndex(x => x.Code)
    .IsUnique()
    .HasDatabaseName("idx_warehouse_code");

builder.HasIndex(x => x.Status)
    .HasDatabaseName("idx_warehouse_status");
```

**✅ Correct:**
```csharp
builder.HasIndex(x => x.Code).IsUnique();
builder.HasIndex(x => x.Status);
```

EF Core generates consistent index names automatically.

Manually naming every index:
- Adds unnecessary maintenance overhead
- Forces future developers to remember naming conventions
- Provides no technical value

### When to Manually Name Indexes

Only name indexes when there is a **genuine interoperability requirement**:
- Cross-tool queries expect specific names
- Database compatibility scripts reference names
- External systems depend on index naming

---

## Audit Metadata vs. Business Audit

These are two separate concerns. **Do not couple them.**

### Database Audit Metadata

**Purpose:** Internal persistence concern—know when records were created/modified.

**Fields:**
- `CreatedAt`
- `UpdatedAt`

**Configuration:**
```csharp
builder.ConfigureAuditFields();  // No IAuditable interface required
```

**Applies to:** Nearly every entity, regardless of business audit logging.

### Business Audit Logging

**Purpose:** Produce audit events, send to Audit Service, sync with Elasticsearch, track user activity.

**Implementation:** Use the project's centralized audit infrastructure independently.

**Note:** An entity can have database audit metadata without business audit logging, and vice versa.

---

## EntityTypeBuilder Extensions

Reusable configuration is in: `BuildingBlock.Persistence.Ef.Configurations.EntityTypeBuilderExtensions`

### Available Extensions

```csharp
// Configure audit fields only
builder.ConfigureAuditFields();

// Configure optimistic concurrency (xmin row version)
builder.ConfigureConcurrency();

// Configure both audit fields and concurrency
builder.ConfigureCommonFields();
```

### Usage

```csharp
public sealed class OrderConfig : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        // ... other configuration ...
        
        // For mutable aggregates
        builder.ConfigureCommonFields();
        
        // For append-only logs
        // builder.ConfigureAuditFields();  // No concurrency needed
    }
}
```

---

## Configuration File Layout

Every entity configuration must follow this order:

### 1. Table and Key
```csharp
builder.ToTable("orders");
builder.HasKey(x => x.Id);
```

### 2. Fields
```csharp
builder.Property(x => x.OrderNumber).IsRequired().HasMaxLength(50);
builder.Property(x => x.Status).HasConversion<short>();
builder.Property(x => x.Total).HasPrecision(12, 2);
```

### 3. Value Objects
```csharp
builder.OwnsOne(x => x.ShippingAddress);
```

### 4. Relationships
```csharp
builder.HasOne(x => x.Customer)
    .WithMany()
    .HasForeignKey(x => x.CustomerId)
    .OnDelete(DeleteBehavior.Restrict);
```

### 5. Indexes
```csharp
builder.HasIndex(x => x.OrderNumber).IsUnique();
builder.HasIndex(x => x.Status);
```

### 6. Audit & Concurrency
```csharp
builder.ConfigureCommonFields();  // or ConfigureAuditFields()
```

---

## One Entity Per Configuration File

**Rule:** One configuration class per entity.

**✅ Correct:**
```
Configs/
  OrderConfig.cs
  OrderItemConfig.cs
  CustomerConfig.cs
  WarehouseConfig.cs
  WarehouseZoneConfig.cs
```

**❌ Avoid:**
```
Configs/
  OrderAndItemConfig.cs  // Multiple entities in one file
  CustomerAndAddressConfig.cs
```

**Rationale:**
- Easier to locate and maintain configurations
- Follows single-responsibility principle
- Clearer intent

---

## Metadata Value Object Mapping

### The Pattern

All metadata objects inherit from `MetadataBase`, which provides centralized serialization/deserialization.

**Never instantiate metadata objects directly in EF configuration.**

❌ **Prohibited:**
```csharp
builder.Property(x => x.Metadata)
    .HasConversion(x => x.Metadata, x => new InventoryMetadata { Metadata = x })
    .HasColumnType("jsonb")
    .IsRequired();
```

✅ **Correct:**
```csharp
builder
    .Property(x => x.Metadata)
    .HasConversion(x => x.ToJson(), x => InventoryMetadata.FromJson<InventoryMetadata>(x))
    .HasColumnType("jsonb")
    .IsRequired();
```

### Why This Matters

- `MetadataBase` provides `ToJson()` and `FromJson<T>()` methods
- These methods handle serialization/deserialization consistently across all services
- Centralizing this logic makes future changes to serialization behavior require modifications in only one place
- Direct object instantiation bypasses this abstraction and creates duplication

### The Pattern in Detail

```csharp
// Serialize: use ToJson() to get the JSON string
x => x.ToJson()

// Deserialize: use FromJson<T>() to restore from JSON
x => MetadataType.FromJson<MetadataType>(x)
```

Example:
```csharp
builder
    .Property(x => x.InventoryMetadata)
    .HasConversion(
        x => x.ToJson(),
        x => InventoryMetadata.FromJson<InventoryMetadata>(x))
    .HasColumnType("jsonb")
    .IsRequired();
```

---

## Fluent Configuration Formatting

### The Standard

Each fluent API call must occupy its own line, even if there is only one additional call.

**❌ Avoid (single-line chains):**
```csharp
builder.Property(x => x.Name).IsRequired();
builder.HasIndex(x => x.Code).IsUnique();
builder.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).IsRequired();
```

**✅ Correct (multiline chains):**
```csharp
builder
    .Property(x => x.Name)
    .IsRequired();

builder
    .HasIndex(x => x.Code)
    .IsUnique();

builder
    .HasOne(x => x.Warehouse)
    .WithMany()
    .HasForeignKey(x => x.WarehouseId)
    .IsRequired();
```

### Why This Matters

This is not a formatting preference; it's a **maintainability principle**.

When adding a new configuration later:
- Single-line chains show every line as modified (even unchanged parts)
- Multiline chains show only the newly added line as modified

**Example: Adding `.IsRequired()` to an existing property:**

With single-line formatting:
```diff
- builder.Property(x => x.Name).HasMaxLength(100);
+ builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
```

The entire line shows as changed, making the PR harder to review.

With multiline formatting:
```diff
  builder
      .Property(x => x.Name)
      .HasMaxLength(100)
+     .IsRequired();
```

Only the new line shows as changed, providing a cleaner diff.

**Benefits:**
- Cleaner pull requests
- Easier code review
- Fewer merge conflicts
- Clearer change history
- Better git blame tracking

### Consistent Style

The **first fluent call** should remain on the same line as `builder`. Subsequent calls go on new lines:

```csharp
// Relationships
builder.HasOne(x => x.Warehouse)
    .WithMany()
    .HasForeignKey(x => x.WarehouseId)
    .OnDelete(DeleteBehavior.Restrict)
    .IsRequired();

// Complex properties with multiple configurations
builder.Property(x => x.Notes)
    .HasMaxLength(500)
    .IsRequired(false)
    .HasDefaultValue(string.Empty);

// Even single configurations
builder.Property(x => x.Id)
    .IsRequired();

builder.HasIndex(x => x.Code)
    .IsUnique();
```

### Why This Specific Format

This format produces **cleaner Git history** when adding new fluent calls.

**Example scenario: Adding `.HasDefaultValue(...)` to an existing property**

With this format:
```diff
  builder.Property(x => x.Name)
      .HasMaxLength(100)
+     .HasDefaultValue(string.Empty);
```

Only the new line appears as changed.

If the first call were on a new line:
```diff
- builder
+ builder
      .Property(x => x.Name)
      .HasMaxLength(100)
+     .HasDefaultValue(string.Empty);
```

Unrelated lines would appear as modified, cluttering the diff and making code reviews harder.

**Benefits:**
- Minimal diffs when extending configurations
- Clearer pull requests
- Easier merge conflict resolution
- Better git blame tracking

---

## Summary of Key Principles

1. **Value Objects**: Use `HasConversion()` for single-value; `OwnsOne()` for multi-value
2. **Metadata Objects**: Always use `MetadataBase.ToJson()` and `FromJson<T>()` for serialization
3. **SQL Types**: Remove unnecessary type declarations; keep only when intentional
4. **Decimals**: Always configure precision
5. **Defaults**: Explicit defaults for clarity, not necessity
6. **Indexes**: Let EF Core name them automatically
7. **Audit Metadata**: Separate concern from business audit logging
8. **Layout**: Follow the standard configuration order
9. **Fluent Formatting**: Each method call on its own line for maintainability
10. **One File Per Entity**: Strict separation of concerns

---

## Future Updates

When adding new entities, apply these conventions automatically:
- Review this document before writing any configuration
- Use the extension methods for common patterns
- Follow the layout order exactly
- Ensure one configuration file per entity

Any deviations from these standards should be documented and justified.
