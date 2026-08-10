using NovaCore.Product.Persistence.Engine;

namespace NovaCore.Product.Persistence.Storage.Seeders;

public sealed class ProductSeeder(ProductDbContext context)
{
    public async Task SeedAsync()
    {
        await SeedCategoriesAsync();
        await SeedSpecificationsAsync();
        await SeedOptionsAsync();
    }

    private async Task SeedCategoriesAsync()
    {
        if (await context.ProductCategories.AnyAsync())
            return;

        var uncategorized = ProductCategory.Create(
            Guid.CreateVersion7(),
            CategoryCode.Create("UNCATEGORIZED"),
            "Uncategorized",
            "Default category for products without an assigned category.");

        context.ProductCategories.Add(uncategorized);
        await context.SaveChangesAsync();
    }

    // ============================================================================
    // Specifications
    // Seeds the system-managed SpecificationGroup/SpecificationDefinition catalog -
    // generic, industry-agnostic groups and fields that products across many
    // categories reuse to describe themselves. Stable, deterministic GUIDs so the
    // seed is idempotent and referenceable across environments.
    // ============================================================================

    private async Task SeedSpecificationsAsync()
    {
        if (await context.SpecificationGroups.AnyAsync())
            return;

        var groups = new[]
        {
            SpecificationGroup.Create(GroupId(1), "GENERAL", 1),
            SpecificationGroup.Create(GroupId(2), "DIMENSIONS", 2),
            SpecificationGroup.Create(GroupId(3), "DISPLAY", 3),
            SpecificationGroup.Create(GroupId(4), "PERFORMANCE", 4),
            SpecificationGroup.Create(GroupId(5), "MEMORY", 5),
            SpecificationGroup.Create(GroupId(6), "STORAGE", 6),
            SpecificationGroup.Create(GroupId(7), "GRAPHICS", 7),
            SpecificationGroup.Create(GroupId(8), "CAMERA", 8),
            SpecificationGroup.Create(GroupId(9), "AUDIO", 9),
            SpecificationGroup.Create(GroupId(10), "BATTERY", 10),
            SpecificationGroup.Create(GroupId(11), "CONNECTIVITY", 11),
            SpecificationGroup.Create(GroupId(12), "PORTS", 12),
            SpecificationGroup.Create(GroupId(13), "MATERIAL", 13),
            SpecificationGroup.Create(GroupId(14), "FEATURES", 14),
            SpecificationGroup.Create(GroupId(15), "COMPATIBILITY", 15),
            SpecificationGroup.Create(GroupId(16), "PACKAGE", 16),
            SpecificationGroup.Create(GroupId(17), "WARRANTY", 17),
            SpecificationGroup.Create(GroupId(18), "CERTIFICATION", 18),
        };
        context.SpecificationGroups.AddRange(groups);

        var definitions = new[]
        {
            // General
            Definition(1, GroupId(1), "BRAND", AttributeDataType.Text, 1),
            Definition(2, GroupId(1), "MODEL", AttributeDataType.Text, 2),
            Definition(3, GroupId(1), "COUNTRY_OF_ORIGIN", AttributeDataType.Text, 3),

            // Dimensions
            Definition(4, GroupId(2), "WEIGHT", AttributeDataType.Decimal, 1, "kg"),
            Definition(5, GroupId(2), "LENGTH", AttributeDataType.Decimal, 2, "cm"),
            Definition(6, GroupId(2), "WIDTH", AttributeDataType.Decimal, 3, "cm"),
            Definition(7, GroupId(2), "HEIGHT", AttributeDataType.Decimal, 4, "cm"),

            // Display
            Definition(8, GroupId(3), "SCREEN_SIZE", AttributeDataType.Decimal, 1, "in"),
            Definition(9, GroupId(3), "RESOLUTION", AttributeDataType.Text, 2),
            Definition(10, GroupId(3), "REFRESH_RATE", AttributeDataType.Number, 3, "Hz"),
            Definition(11, GroupId(3), "PANEL_TYPE", AttributeDataType.Text, 4),

            // Performance
            Definition(12, GroupId(4), "CPU", AttributeDataType.Text, 1),
            Definition(13, GroupId(4), "CHIPSET", AttributeDataType.Text, 2),
            Definition(14, GroupId(4), "CORE_COUNT", AttributeDataType.Number, 3),

            // Memory
            Definition(15, GroupId(5), "RAM", AttributeDataType.Text, 1),

            // Storage
            Definition(16, GroupId(6), "INTERNAL_STORAGE", AttributeDataType.Text, 1),
            Definition(17, GroupId(6), "EXPANDABLE_STORAGE", AttributeDataType.Text, 2),

            // Graphics
            Definition(18, GroupId(7), "GPU", AttributeDataType.Text, 1),
            Definition(19, GroupId(7), "VIDEO_MEMORY", AttributeDataType.Text, 2),

            // Camera
            Definition(20, GroupId(8), "REAR_CAMERA", AttributeDataType.Text, 1),
            Definition(21, GroupId(8), "FRONT_CAMERA", AttributeDataType.Text, 2),

            // Audio
            Definition(22, GroupId(9), "SPEAKER", AttributeDataType.Text, 1),
            Definition(23, GroupId(9), "MICROPHONE", AttributeDataType.Text, 2),

            // Battery
            Definition(24, GroupId(10), "BATTERY_CAPACITY", AttributeDataType.Number, 1, "mAh"),
            Definition(25, GroupId(10), "CHARGING_POWER", AttributeDataType.Number, 2, "W"),

            // Connectivity
            Definition(26, GroupId(11), "WIFI", AttributeDataType.Text, 1),
            Definition(27, GroupId(11), "BLUETOOTH", AttributeDataType.Text, 2),
            Definition(28, GroupId(11), "NFC", AttributeDataType.Boolean, 3),

            // Ports
            Definition(29, GroupId(12), "USB_TYPE", AttributeDataType.Text, 1),
            Definition(30, GroupId(12), "HDMI", AttributeDataType.Boolean, 2),
            Definition(31, GroupId(12), "PORT_COUNT", AttributeDataType.Number, 3),

            // Material
            Definition(32, GroupId(13), "PRIMARY_MATERIAL", AttributeDataType.Text, 1),
            Definition(33, GroupId(13), "FINISH", AttributeDataType.Text, 2),

            // Features
            Definition(34, GroupId(14), "KEY_FEATURES", AttributeDataType.Text, 1),
            Definition(35, GroupId(14), "WATER_RESISTANCE_RATING", AttributeDataType.Text, 2),

            // Compatibility
            Definition(36, GroupId(15), "COMPATIBLE_DEVICES", AttributeDataType.Text, 1),
            Definition(37, GroupId(15), "OPERATING_SYSTEM", AttributeDataType.Text, 2),

            // Package
            Definition(38, GroupId(16), "PACKAGE_CONTENTS", AttributeDataType.Text, 1),
            Definition(39, GroupId(16), "PACKAGE_WEIGHT", AttributeDataType.Decimal, 2, "kg"),

            // Warranty
            Definition(40, GroupId(17), "WARRANTY_PERIOD", AttributeDataType.Number, 1, "months"),

            // Certification
            Definition(41, GroupId(18), "CE", AttributeDataType.Boolean, 1),
            Definition(42, GroupId(18), "FCC", AttributeDataType.Boolean, 2),
            Definition(43, GroupId(18), "ROHS", AttributeDataType.Boolean, 3),
        };
        context.SpecificationDefinitions.AddRange(definitions);

        await context.SaveChangesAsync();
    }

    private static SpecificationDefinition Definition(
        int seed,
        Guid groupId,
        string code,
        AttributeDataType dataType,
        int displayOrder,
        string? defaultUnit = null)
        => SpecificationDefinition.Create(DefinitionId(seed), groupId, code, dataType, defaultUnit, displayOrder);

    private static Guid GroupId(int seed) => DeterministicId(1, seed);

    private static Guid DefinitionId(int seed) => DeterministicId(2, seed);

    // ============================================================================
    // Options
    // Seeds the reusable ProductOptionDefinition/ProductOptionValueDefinition
    // catalog - global option dimensions (Color, Size, ...) and their common
    // values, shared across every Product that opts into them via ProductOption.
    // Stable, deterministic GUIDs so the seed is idempotent and referenceable
    // across environments. Options without a widely-agreed value set (Style,
    // Pattern, Length, ...) are seeded with no values.
    // ============================================================================

    private async Task SeedOptionsAsync()
    {
        if (await context.ProductOptionDefinitions.AnyAsync())
            return;

        var color = ProductOptionDefinition.Create(OptionDefinitionId(1), "COLOR", 1);
        color.AddValue("BLACK", ValueDefinitionId(1), 1);
        color.AddValue("WHITE", ValueDefinitionId(2), 2);
        color.AddValue("RED", ValueDefinitionId(3), 3);
        color.AddValue("BLUE", ValueDefinitionId(4), 4);
        color.AddValue("GREEN", ValueDefinitionId(5), 5);
        color.AddValue("YELLOW", ValueDefinitionId(6), 6);
        color.AddValue("GRAY", ValueDefinitionId(7), 7);

        var size = ProductOptionDefinition.Create(OptionDefinitionId(2), "SIZE", 2);
        size.AddValue("XS", ValueDefinitionId(8), 1);
        size.AddValue("S", ValueDefinitionId(9), 2);
        size.AddValue("M", ValueDefinitionId(10), 3);
        size.AddValue("L", ValueDefinitionId(11), 4);
        size.AddValue("XL", ValueDefinitionId(12), 5);
        size.AddValue("XXL", ValueDefinitionId(13), 6);

        var storage = ProductOptionDefinition.Create(OptionDefinitionId(3), "STORAGE", 3);
        storage.AddValue("64GB", ValueDefinitionId(14), 1);
        storage.AddValue("128GB", ValueDefinitionId(15), 2);
        storage.AddValue("256GB", ValueDefinitionId(16), 3);
        storage.AddValue("512GB", ValueDefinitionId(17), 4);
        storage.AddValue("1TB", ValueDefinitionId(18), 5);

        var material = ProductOptionDefinition.Create(OptionDefinitionId(4), "MATERIAL", 4);
        material.AddValue("COTTON", ValueDefinitionId(19), 1);
        material.AddValue("POLYESTER", ValueDefinitionId(20), 2);
        material.AddValue("LEATHER", ValueDefinitionId(21), 3);
        material.AddValue("WOOD", ValueDefinitionId(22), 4);
        material.AddValue("METAL", ValueDefinitionId(23), 5);
        material.AddValue("PLASTIC", ValueDefinitionId(24), 6);
        material.AddValue("GLASS", ValueDefinitionId(25), 7);

        var capacity = ProductOptionDefinition.Create(OptionDefinitionId(5), "CAPACITY", 5);
        capacity.AddValue("250ML", ValueDefinitionId(26), 1);
        capacity.AddValue("500ML", ValueDefinitionId(27), 2);
        capacity.AddValue("1L", ValueDefinitionId(28), 3);
        capacity.AddValue("2L", ValueDefinitionId(29), 4);

        var version = ProductOptionDefinition.Create(OptionDefinitionId(7), "VERSION", 7);
        version.AddValue("STANDARD", ValueDefinitionId(30), 1);
        version.AddValue("PLUS", ValueDefinitionId(31), 2);
        version.AddValue("PRO", ValueDefinitionId(32), 3);
        version.AddValue("ULTRA", ValueDefinitionId(33), 4);

        var style = ProductOptionDefinition.Create(OptionDefinitionId(6), "STYLE", 6);
        var pattern = ProductOptionDefinition.Create(OptionDefinitionId(8), "PATTERN", 8);
        var flavor = ProductOptionDefinition.Create(OptionDefinitionId(9), "FLAVOR", 9);
        var scent = ProductOptionDefinition.Create(OptionDefinitionId(10), "SCENT", 10);
        var length = ProductOptionDefinition.Create(OptionDefinitionId(11), "LENGTH", 11);
        var width = ProductOptionDefinition.Create(OptionDefinitionId(12), "WIDTH", 12);
        var height = ProductOptionDefinition.Create(OptionDefinitionId(13), "HEIGHT", 13);
        var volume = ProductOptionDefinition.Create(OptionDefinitionId(14), "VOLUME", 14);
        var finish = ProductOptionDefinition.Create(OptionDefinitionId(15), "FINISH", 15);

        context.ProductOptionDefinitions.AddRange(
            color, size, storage, material, capacity, version,
            style, pattern, flavor, scent, length, width, height, volume, finish);

        await context.SaveChangesAsync();
    }

    private static Guid OptionDefinitionId(int seed) => DeterministicId(3, seed);

    private static Guid ValueDefinitionId(int seed) => DeterministicId(4, seed);

    // ============================================================================
    // Deterministic IDs
    // Every seeded row needs a stable ID across environments and re-seeds, but an
    // obviously sequential "00000000-...-000001" pattern is easy to guess or collide
    // with real data. SeedPrefix is the first three groups of a real Guid.CreateVersion7()
    // value captured once and hardcoded here; category and seed are injected straight
    // into the remaining two groups, so the result still reads as a plausible v7 GUID.
    // ============================================================================

    private const string SeedPrefix = "019fc751-491f-7d48";

    private static Guid DeterministicId(int category, int seed)
        => Guid.Parse($"{SeedPrefix}-8{category:x3}-{seed:x12}");
}
