using NovaCore.BuildingBlock.Criteria.Validation;

using FluentValidation;

using NovaCore.User.Application.Features.Users.Search;

namespace NovaCore.User.Application.Features.Users.Queries.SearchUsers;

public sealed class SearchUsersValidator : AbstractValidator<SearchUsersQuery>
{
    public SearchUsersValidator()
    {
        RuleFor(x => x.Criteria).Custom((criteria, context) =>
        {
            var errors = CriteriaRequestValidator<UserReadModel>.Validate(UserCriteriaDefinition.Instance, criteria);
            foreach (var error in errors)
                context.AddFailure(error);
        });
    }
}
