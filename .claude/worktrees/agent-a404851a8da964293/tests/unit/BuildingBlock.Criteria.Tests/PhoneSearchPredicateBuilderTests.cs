using System.Text.Json;

using NovaCore.BuildingBlock.Criteria.Building;
using NovaCore.BuildingBlock.Criteria.Definition;
using NovaCore.BuildingBlock.Criteria.Enums;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.BuildingBlock.Criteria.Strategies;
using NovaCore.BuildingBlock.SharedKernel.Text;

using Shouldly;

namespace NovaCore.BuildingBlock.Criteria.Tests;

/// <summary>Reproduces the task's own worked examples: `0901` (prefix) finds 0901234567/0901888888; `4567` (suffix) finds 0901234567/08124567 - both via PhoneSearch/PhoneReverse, never a `LIKE '%x%'` scan.</summary>
public class PhoneSearchPredicateBuilderTests
{
    private static readonly CriteriaDefinition<CriteriaTestEntity> Definition = CriteriaDefinition<CriteriaTestEntity>.Create()
        .Field(x => x.PhoneSearch, name: "phone").UsePhoneSearch(x => x.PhoneSearch, x => x.PhoneReverse)
        .Build();

    private static readonly IQueryable<CriteriaTestEntity> Users = BuildUsers("0901234567", "0901888888", "08124567", "0777777777")
        .AsQueryable();

    [Fact]
    public void PrefixSearch_MatchesNumbersStartingWithKeyword()
    {
        var request = new CriteriaRequest
        {
            Filters = [new CriteriaFilter("phone", CriteriaOperator.StartsWith, JsonSerializer.SerializeToElement("0901"))],
        };

        var predicate = CriteriaPredicateBuilder<CriteriaTestEntity>.Build(Definition, request)!;
        var result = Users.Where(predicate).Select(x => x.PhoneSearch).ToList();

        result.ShouldBe(["0901234567", "0901888888"], ignoreOrder: true);
    }

    [Fact]
    public void SuffixSearch_MatchesNumbersEndingWithKeyword()
    {
        var request = new CriteriaRequest
        {
            Filters = [new CriteriaFilter("phone", CriteriaOperator.EndsWith, JsonSerializer.SerializeToElement("4567"))],
        };

        var predicate = CriteriaPredicateBuilder<CriteriaTestEntity>.Build(Definition, request)!;
        var result = Users.Where(predicate).Select(x => x.PhoneSearch).ToList();

        result.ShouldBe(["0901234567", "08124567"], ignoreOrder: true);
    }

    [Fact]
    public void ContainsOperator_IsNotWhitelistedForPhoneField()
    {
        Definition.TryGetField("phone", out var field).ShouldBeTrue();

        field.AllowedOperators.ShouldNotContain(CriteriaOperator.Contains);
    }

    private static List<CriteriaTestEntity> BuildUsers(params string[] phoneNumbers)
        => phoneNumbers
            .Select(phone =>
            {
                var search = PhoneNormalizer.Normalize(phone);
                return new CriteriaTestEntity { PhoneSearch = search, PhoneReverse = PhoneNormalizer.Reverse(search) };
            })
            .ToList();
}
