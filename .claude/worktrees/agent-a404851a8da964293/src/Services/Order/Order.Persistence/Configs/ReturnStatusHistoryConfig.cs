using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Order.Persistence.Configs;

public sealed class ReturnStatusHistoryConfig : IEntityTypeConfiguration<ReturnStatusHistory>
{
    public void Configure(EntityTypeBuilder<ReturnStatusHistory> builder)
    {
        // Table
        builder.ToTable("return_status_histories");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReturnOrderId).IsRequired();
        builder.Property(x => x.PreviousStatus).HasConversion<int>().IsRequired();
        builder.Property(x => x.CurrentStatus).HasConversion<int>().IsRequired();
        builder.Property(x => x.ChangedByName).HasMaxLength(200);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.Comment).HasMaxLength(1000);

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");

        // Relationships
        // Shadow reference to ReturnOrder, same reasoning as OrderStatusHistory's shadow
        // reference to Order. Pure history, so deleting the return cascades to its history too.
        builder.HasOne<ReturnOrder>()
            .WithMany()
            .HasForeignKey(x => x.ReturnOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.ReturnOrderId, x.ChangedAt });
    }
}
