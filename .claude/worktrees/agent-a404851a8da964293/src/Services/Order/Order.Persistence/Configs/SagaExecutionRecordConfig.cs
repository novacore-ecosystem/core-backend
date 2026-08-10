using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NovaCore.Order.Persistence.Reliability.Saga;

namespace NovaCore.Order.Persistence.Configs;

public sealed class SagaExecutionRecordConfig : IEntityTypeConfiguration<SagaExecutionRecordEntity>
{
    public void Configure(EntityTypeBuilder<SagaExecutionRecordEntity> builder)
    {
        builder.ToTable("saga_execution_records");
        builder.HasKey(x => x.SagaId);

        builder.Property(x => x.SagaId).HasMaxLength(200);
        builder.Property(x => x.SagaName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CompletedStepsJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.ContextDataJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(200);
        builder.Property(x => x.UserId).HasMaxLength(200);

        // GetHistoryAsync/GetFailedSagasAsync filter on SagaName/State and sort StartedAt DESC -
        // composites cover both the filter and the sort (and still serve filter-only queries via
        // their leftmost column, so the old single-column SagaName/State indexes were removed).
        builder.HasIndex(x => new { x.SagaName, x.StartedAt });
        builder.HasIndex(x => new { x.State, x.StartedAt });
    }
}
