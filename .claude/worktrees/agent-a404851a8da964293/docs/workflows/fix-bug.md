# Workflow: Fix Bug

**Read first:** [02-architecture-rules.md](../02-architecture-rules.md#exception-rule), the affected service doc under `services/`.

## Before changing code

1. **Reproduce first.** Identify the exact request/input that triggers the bug and the exact observed vs expected behavior. Don't fix based on a guess at the cause.
2. **Locate the layer.** Check the symptom against [02-architecture-rules.md](../02-architecture-rules.md#layer-responsibilities) — a wrong HTTP status usually means an exception-handling issue ([reference/exceptions.md](../reference/exceptions.md)); wrong data usually means a query/repository issue; a missed side effect usually means an event-wiring issue ([reference/events.md](../reference/events.md)).
3. **Check for a known issue first.** The affected service's doc has a "Known issues" section — confirm you're not re-discovering (or worse, re-fixing incompatibly) something already tracked there.

## Locating root cause

- Trace the call path top-down: endpoint → command/query → handler → repository/external call. Most bugs in this codebase are handler-level (wrong exception type, missed validation, wrong repository call) — check there before assuming an infrastructure issue.
- If the bug is a wrong/missing HTTP status code, check whether the thrown exception type is recognized by `ExceptionHandlerHelper` (`BuildingBlock.Infrastructure/ExceptionHandling/ExceptionHandlerHelper.cs`) — see [reference/exceptions.md](../reference/exceptions.md). Raw BCL exceptions (`InvalidOperationException`, etc.) are the most common cause of this class of bug — see the known example in [services/user-service.md](../services/user-service.md#known-issues).
- If the bug is cross-service (event never arrives, wrong data in a consumer), check topic naming (`{serviceName}.{eventType}` lowercased) and consumer registration order — see [workflows/add-integration-event.md](add-integration-event.md).

## Avoiding regressions

- Fix the root cause in the layer that owns it — don't patch a symptom in the endpoint if the actual bug is in the handler or repository.
- If you fix a systemic issue (e.g. a wrong exception type), grep for the same anti-pattern elsewhere in the same service before considering the fix complete — it's rarely isolated to one handler.
- Check whether the fix changes a documented contract (route shape, exception→status mapping, event schema) — if so, update the owning doc (see [05-context-loading-map.md](../05-context-loading-map.md#by-document-what-triggers-reading-it) to find it) in the same change.

## Verification

**Write a failing test that reproduces the bug before fixing it**, in the test project matching the layer that owns the root cause (`{Service}.Domain.Tests` / `{Service}.Application.Tests` / the matching `BuildingBlock.*.Tests`) — see [testing/TestingGuidelines.md](../testing/TestingGuidelines.md#bug-fixes). Name it after the scenario, not the ticket number. Once it fails for the right reason, apply the fix and confirm it passes; the test stays in the suite as a regression guard.

Then verify manually against the exact repro from step 1, and exercise the surrounding endpoints/flows that share the changed code path (e.g. if you fixed a handler's exception type, hit both the failure case and the success case).
