using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.User.Persistence.Configs;

public sealed class UserPreferenceConfig : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> builder)
    {
        // Table
        builder.ToTable("user_preferences");

        // Properties
        // Shared primary key (1:1 with User) - no surrogate Id, exactly one preference row per user.
        builder.HasKey(x => x.UserId);

        // Read-only public collections backed by private fields (_favoriteCategories, etc.) - EF's
        // default backing-field convention resolves the field automatically from the property
        // name, so these map like any other primitive collection to a native Postgres array.
        builder.Property(x => x.FavoriteCategories)
            .HasColumnType("uuid[]")
            .IsRequired();

        builder.Property(x => x.FavoriteBrands)
            .HasColumnType("uuid[]")
            .IsRequired();

        builder.Property(x => x.RecentlyViewedProducts)
            .HasColumnType("uuid[]")
            .IsRequired();

        builder.Property(x => x.SearchHistory)
            .HasColumnType("text[]")
            .IsRequired();

        builder.Property(x => x.PreferredWarehouseCode)
            .HasMaxLength(50);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
