# Workflow: Add New API (endpoint on an existing service)

**Read first:** [02-architecture-rules.md](../02-architecture-rules.md), [04-coding-rules.md](../04-coding-rules.md), target service doc, [06-implementation-templates.md](../06-implementation-templates.md) (Command/Query + Endpoint templates).

## Steps

1. **Decide Command vs Query.** Mutates state → Command. Reads only → Query.
2. **Create the feature folder** under `{Service}.Application/Features/{Feature}/{Commands|Queries}/{Verb}/` if it doesn't exist, following [04-coding-rules.md](../04-coding-rules.md#folder-structure-per-feature).
3. **Write the Command/Query + Result record** from the template.
4. **Write the Handler.** Inject the repository interface (specific if one exists, otherwise `IRepository<T>` — see [04-coding-rules.md](../04-coding-rules.md#repository--unit-of-work)). Throw `BuildingBlock.Application.Exceptions.*` for failures — never a raw BCL exception (see [02-architecture-rules.md](../02-architecture-rules.md#exception-rule)).
5. **Write the Validator** if the command has non-trivial input — FluentValidation, same folder, auto-registered by assembly scan. Don't manually register it.
6. **Write the Carter endpoint** in `{Service}.API/Endpoints/{Verb}{Entity}.cs` from the template — bind request, build command, `sender.Send`, return `ApiResponse<T>.Ok(...)`. No business logic here.
7. **Decide auth.** Default requires authentication (via `AddCommonAuthorizationPolicies`); call `.AllowAnonymous()` only if this endpoint must be public. If it needs a specific role, see [reference/authorization.md](../reference/authorization.md).
8. **If this raises a same-service side effect** (e.g. "after this happens, also do X"), use an Internal event, not a direct call — see [reference/events.md](../reference/events.md).
9. **If this needs to notify another service**, publish an Integration event instead — see [workflows/add-integration-event.md](add-integration-event.md).

## Checklist

- [ ] Command/Query implements `ICommand<T>`/`IQuery<T>` from `BuildingBlock.Application.Abstractions.CQRS` (not raw MediatR)
- [ ] Handler throws Application/Domain exceptions only, never raw BCL exceptions
- [ ] Validator (if any) is auto-discovered — no manual registration added
- [ ] Endpoint file is named after the action, contains no business logic
- [ ] `ct` threaded through with `= default`
- [ ] Response wrapped in `ApiResponse<T>`
- [ ] Swagger `.WithSummary()`/`.WithDescription()`/`.Produces<T>()` set (matches existing endpoints in the same service)
- [ ] Updated the target service doc under `docs/services/` if this adds a new route (keep the route table current)

## Testing

Add a Handler test (`{Verb}HandlerTests`) covering the success path plus every validation/business-exception branch, in `{Service}.Application.Tests` (create the project if this is the first handler test for the service — see [testing/TestingArchitecture.md](../testing/TestingArchitecture.md) for the project shape). Mock only the repository/`IUnitOfWork`/external service dependencies via NSubstitute — never the Command/Result records or any domain object. See [testing/TestingGuidelines.md](../testing/TestingGuidelines.md).

Then verify manually: build the affected project, run the service via Docker Compose (see [setup/docker.md](../setup/docker.md)), exercise the endpoint through the Gateway or directly against the service's exposed port in dev.
