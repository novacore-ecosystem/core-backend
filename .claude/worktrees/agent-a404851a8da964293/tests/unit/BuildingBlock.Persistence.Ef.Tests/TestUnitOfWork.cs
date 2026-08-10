using NovaCore.BuildingBlock.Persistence.Ef.UnitOfWork;

namespace NovaCore.BuildingBlock.Persistence.Ef.Tests;

internal sealed class TestUnitOfWork(TestDbContext context) : EfUnitOfWork<TestDbContext>(context);
