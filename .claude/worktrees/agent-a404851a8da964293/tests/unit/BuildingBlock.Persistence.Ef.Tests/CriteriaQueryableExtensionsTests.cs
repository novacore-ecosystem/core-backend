using System.Text.Json;

using NovaCore.BuildingBlock.Criteria.Definition;
using NovaCore.BuildingBlock.Criteria.Enums;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.BuildingBlock.Persistence.Ef.Criteria;

using Microsoft.EntityFrameworkCore;

namespace NovaCore.BuildingBlock.Persistence.Ef.Tests;

public class CriteriaQueryableExtensionsTests
{
    private static readonly CriteriaDefinition<TestUser> Definition = CriteriaDefinition<TestUser>.Create()
        .Field(x => x.Email).String().Sortable().KeywordSearchable()
        .Build();

    [Fact]
    public async Task ApplyCriteria_FiltersAndPagesAgainstRealDbContext()
    {
        await using var context = CreateContext();
        context.Users.AddRange(
            new TestUser { Id = Guid.NewGuid(), Email = "alex@example.com" },
            new TestUser { Id = Guid.NewGuid(), Email = "jun@example.com" },
            new TestUser { Id = Guid.NewGuid(), Email = "june@example.com" });
        await context.SaveChangesAsync();

        var request = new CriteriaRequest
        {
            Filters = [new CriteriaFilter("email", CriteriaOperator.EndsWith, JsonSerializer.SerializeToElement("@example.com"))],
            Sorts = [new CriteriaSort("email", SortDirection.Asc)],
            Page = 1,
            PageSize = 2,
        };

        var result = await context.Users
            .ApplyCriteria(Definition, request)
            .ToCriteriaPagedResultAsync(request);

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(["alex@example.com", "jun@example.com"], result.Items.Select(u => u.Email));
    }

    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }
}
