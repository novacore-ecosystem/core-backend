using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.User.Persistence.Configs;

public sealed class UserAvatarConfig : IEntityTypeConfiguration<UserAvatar>
{
    public void Configure(EntityTypeBuilder<UserAvatar> builder)
    {
        // Table
        builder.ToTable("user_avatars");

        // Properties
        // Shared primary key (1:1 with User) - no surrogate Id, exactly one avatar row per user.
        builder.HasKey(x => x.UserId);

        builder.Property(x => x.MediaId)
            .IsRequired();

        builder.Property(x => x.ThumbnailMediaId);

        builder.Property(x => x.DisplayMode)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(AvatarDisplayMode.Original);

        builder.Property(x => x.Version)
            .IsRequired()
            .HasDefaultValue(0);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
