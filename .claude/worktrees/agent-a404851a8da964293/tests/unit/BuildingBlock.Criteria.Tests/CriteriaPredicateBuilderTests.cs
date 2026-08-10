using System.Text.Json;

using NovaCore.BuildingBlock.Criteria.Building;
using NovaCore.BuildingBlock.Criteria.Definition;
using NovaCore.BuildingBlock.Criteria.Enums;
using NovaCore.BuildingBlock.Criteria.Requests;

using Shouldly;

namespace NovaCore.BuildingBlock.Criteria.Tests;

public class CriteriaPredicateBuilderTests
{
    private static readonly CriteriaDefinition<CriteriaTestEntity> Definition = CriteriaDefinition<CriteriaTestEntity>.Create()
        .Field(x => x.Name).String().Sortable().KeywordSearchable()
        .Field(x => x.Age).Number()
        .Field(x => x.Status).Enum()
        .Build();

    private static readonly IQueryable<CriteriaTestEntity> Entities = new List<CriteriaTestEntity>
    {
        new() { Name = "Jun", Age = 20, Status = CriteriaTestStatus.Active },
        new() { Name = "Alex", Age = 30, Status = CriteriaTestStatus.Inactive },
        new() { Name = "June", Age = 40, Status = CriteriaTestStatus.Active },
    }.AsQueryable();

    [Fact]
    public void Eq_MatchesExactValue()
    {
        var request = new CriteriaRequest
        {
            Filters = [new CriteriaFilter("age", CriteriaOperator.Eq, JsonSerializer.SerializeToElement(30))],
        };

        var predicate = CriteriaPredicateBuilder<CriteriaTestEntity>.Build(Definition, request)!;

        Entities.Where(predicate).Select(x => x.Name).ShouldBe(["Alex"]);
    }

    [Fact]
    public void In_MatchesAnyValueInArray()
    {
        var value = JsonSerializer.SerializeToElement(new[] { "Active" });
        var request = new CriteriaRequest
        {
            Filters = [new CriteriaFilter("status", CriteriaOperator.In, value)],
        };

        var predicate = CriteriaPredicateBuilder<CriteriaTestEntity>.Build(Definition, request)!;

        Entities.Where(predicate).Select(x => x.Name).ShouldBe(["Jun", "June"], ignoreOrder: true);
    }

    [Fact]
    public void Gte_MatchesValuesAboveThreshold()
    {
        var request = new CriteriaRequest
        {
            Filters = [new CriteriaFilter("age", CriteriaOperator.Gte, JsonSerializer.SerializeToElement(30))],
        };

        var predicate = CriteriaPredicateBuilder<CriteriaTestEntity>.Build(Definition, request)!;

        Entities.Where(predicate).Select(x => x.Name).ShouldBe(["Alex", "June"], ignoreOrder: true);
    }

    [Fact]
    public void Keyword_MatchesAnyKeywordSearchableField()
    {
        var request = new CriteriaRequest { Keyword = "Jun" };

        var predicate = CriteriaPredicateBuilder<CriteriaTestEntity>.Build(Definition, request)!;

        Entities.Where(predicate).Select(x => x.Name).ShouldBe(["Jun", "June"], ignoreOrder: true);
    }

    [Fact]
    public void NoFiltersOrKeyword_ReturnsNullPredicate()
    {
        var predicate = CriteriaPredicateBuilder<CriteriaTestEntity>.Build(Definition, new CriteriaRequest());

        predicate.ShouldBeNull();
    }
}
