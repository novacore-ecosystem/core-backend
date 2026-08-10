using System.Text.Json;

using NovaCore.BuildingBlock.Criteria.Definition;
using NovaCore.BuildingBlock.Criteria.Enums;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.BuildingBlock.Criteria.Validation;

using Shouldly;

using SortDirection = NovaCore.BuildingBlock.Criteria.Requests.SortDirection;

namespace NovaCore.BuildingBlock.Criteria.Tests;

public class CriteriaRequestValidatorTests
{
    private static readonly CriteriaDefinition<CriteriaTestEntity> Definition = CriteriaDefinition<CriteriaTestEntity>.Create()
        .Field(x => x.Name).String().Sortable().KeywordSearchable()
        .Field(x => x.Age).Number()
        .Field(x => x.Status).Enum()
        .Build();

    [Fact]
    public void Validate_UnknownField_ReturnsError()
    {
        var request = new CriteriaRequest
        {
            Filters = [new CriteriaFilter("unknown", CriteriaOperator.Eq, JsonSerializer.SerializeToElement("x"))],
        };

        var errors = CriteriaRequestValidator<CriteriaTestEntity>.Validate(Definition, request);

        errors.ShouldContain(e => e.Contains("Unknown field"));
    }

    [Fact]
    public void Validate_OperatorNotWhitelistedForField_ReturnsError()
    {
        // Age is Number - Contains ("c") is not in the default Number whitelist.
        var request = new CriteriaRequest
        {
            Filters = [new CriteriaFilter("age", CriteriaOperator.Contains, JsonSerializer.SerializeToElement("1"))],
        };

        var errors = CriteriaRequestValidator<CriteriaTestEntity>.Validate(Definition, request);

        errors.ShouldContain(e => e.Contains("not allowed"));
    }

    [Fact]
    public void Validate_InOperatorWithNonArrayValue_ReturnsError()
    {
        var request = new CriteriaRequest
        {
            Filters = [new CriteriaFilter("status", CriteriaOperator.In, JsonSerializer.SerializeToElement("Active"))],
        };

        var errors = CriteriaRequestValidator<CriteriaTestEntity>.Validate(Definition, request);

        errors.ShouldContain(e => e.Contains("non-empty array"));
    }

    [Fact]
    public void Validate_SortOnUnsortableField_ReturnsError()
    {
        var request = new CriteriaRequest { Sorts = [new CriteriaSort("age", SortDirection.Asc)] };

        var errors = CriteriaRequestValidator<CriteriaTestEntity>.Validate(Definition, request);

        errors.ShouldContain(e => e.Contains("not sortable"));
    }

    [Fact]
    public void Validate_WellFormedRequest_ReturnsNoErrors()
    {
        var request = new CriteriaRequest
        {
            Keyword = "jun",
            Filters = [new CriteriaFilter("age", CriteriaOperator.Gte, JsonSerializer.SerializeToElement(18))],
            Sorts = [new CriteriaSort("name", SortDirection.Desc)],
            Page = 1,
            PageSize = 20,
        };

        var errors = CriteriaRequestValidator<CriteriaTestEntity>.Validate(Definition, request);

        errors.ShouldBeEmpty();
    }

    [Fact]
    public void Validate_PageLessThanOne_ReturnsError()
    {
        var request = new CriteriaRequest { Page = 0 };

        var errors = CriteriaRequestValidator<CriteriaTestEntity>.Validate(Definition, request);

        errors.ShouldContain(e => e.Contains("Page"));
    }
}
