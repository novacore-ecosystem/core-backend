using NovaCore.BuildingBlock.Application.Exceptions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using Npgsql;

namespace NovaCore.BuildingBlock.Persistence.Ef.Tests;

/// <summary>
/// Regression coverage for the AddVariation TOCTOU race (B1): two concurrent requests can both
/// pass SkuExistsAsync and then collide on the DB's unique index during SaveChanges. Before this
/// fix that raised a raw DbUpdateException/PostgresException (500); it should now surface as the
/// same ConflictException (409) the pre-check would have thrown had it lost the race instead.
/// </summary>
public sealed class EfUnitOfWorkTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            // The InMemory provider neither supports real transactions nor enforces unique
            // indexes, so it can't organically reproduce a unique-violation. This suppresses its
            // "transactions ignored" warning so BeginTransactionAsync succeeds as a no-op, letting
            // the test drive ExecuteTransactionAsync's exception-translation logic directly.
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new TestDbContext(options);
    }

    private static PostgresException UniqueViolation() => new(
        messageText: "duplicate key value violates unique constraint \"ix_variants_sku\"",
        severity: "ERROR",
        invariantSeverity: "ERROR",
        sqlState: PostgresErrorCodes.UniqueViolation);

    [Fact]
    public async Task ExecuteTransactionAsync_TranslatesUniqueViolation_ToConflictException()
    {
        await using var context = CreateContext();
        var uow = new TestUnitOfWork(context);

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            uow.ExecuteTransactionAsync(() => throw new DbUpdateException("insert failed", UniqueViolation())));

        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task ExecuteTransactionAsync_RethrowsDbUpdateException_ForNonUniqueViolations()
    {
        await using var context = CreateContext();
        var uow = new TestUnitOfWork(context);

        var otherError = new PostgresException(
            messageText: "some other constraint failure",
            severity: "ERROR",
            invariantSeverity: "ERROR",
            sqlState: "23514"); // check_violation, not unique_violation

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            uow.ExecuteTransactionAsync(() => throw new DbUpdateException("insert failed", otherError)));
    }
}
