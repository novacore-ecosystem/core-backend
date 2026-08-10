# Workflow: Refactor Existing Code

**Read first:** [04-coding-rules.md](../04-coding-rules.md), [07-solid-recommendations.md](../07-solid-recommendations.md) (check if the area you're touching is already flagged there).

## Safety checklist

- [ ] The refactor changes structure, not behavior — if you find yourself also changing what an endpoint returns, what an exception maps to, or an event's schema, that's two changes; do the behavior change separately and call it out explicitly.
- [ ] Confirm no other service depends on the thing you're changing (check `BuildingBlock.Contract` usage, gRPC proto consumers, Kafka topic consumers via [reference/events.md](../reference/events.md) before renaming/removing anything cross-service-visible).
- [ ] Confirm the change doesn't silently alter DI registration order — several services depend on it (e.g. `AddPersistence` before `AddInfrastructure`, consumers registered before `AddKafkaMessaging` — see [02-architecture-rules.md](../02-architecture-rules.md#composition-root-convention-per-service)).

## SOLID checklist

Check [07-solid-recommendations.md](../07-solid-recommendations.md) first — if the area is already documented as a known gap with a recommended direction, follow that direction rather than inventing a new one. Otherwise:

- [ ] Single Responsibility: does the class/method still do one thing after your change?
- [ ] Open/Closed: can new cases be added without modifying this code, or did you just add another one that will need the same treatment?
- [ ] Liskov: if this implements an interface, does every existing caller still get the behavior they expect?
- [ ] Interface Segregation: are you adding a method to a widely-implemented interface that only one implementer needs? Consider a narrower interface instead.
- [ ] Dependency Inversion: does the refactor keep the dependency direction in [02-architecture-rules.md](../02-architecture-rules.md#dependency-direction-must-never-be-violated)? (Most common violation: pulling an `Infrastructure`/`Web` type into `Application` "just this once.")

## Reuse checklist

- [ ] Before writing a new helper, check [03-building-blocks-reference.md](../03-building-blocks-reference.md) and [06-implementation-templates.md](../06-implementation-templates.md) — the thing you're about to write may already exist as a shared building block.
- [ ] If you're deduplicating logic that's currently copy-pasted across Auth and User, the target for the shared version is a `BuildingBlock.*` project matching its layer (Application logic → `BuildingBlock.Application`, infra → `BuildingBlock.Infrastructure`, API-layer → `BuildingBlock.Web`) — not a new per-service "Shared" folder.

## Regression checklist

- [ ] Re-read the affected service's "Known issues" section in its `services/*.md` doc — make sure your refactor doesn't reintroduce something already flagged as fixed, or fix a known issue as an unplanned side effect (note it if so, don't silently swallow it).
- [ ] No automated tests exist — manually exercise every code path you touched, not just the happy path.
- [ ] If you touched a DI registration, restart the affected service and confirm it still boots (a missing/misordered registration usually fails fast at startup, not silently).
