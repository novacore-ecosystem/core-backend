---
name: implement
description: Implement complete, production-ready NovaCore features against existing architecture — CQRS, entities, endpoints, consumers, and more — TODO-marking only genuinely unknowable business rules
---

## Purpose
Generate a complete, production-quality implementation of the requested construct, matching this project's existing architecture, naming, and style exactly — as close to "written by this project's senior engineers" as possible. This is the highest-volume Skill in the framework; it must never degrade into boilerplate generation or tutorial-quality code.

## Supported Commands
`/implement <target> <Name>` where `<target>` is one of:

`api` · `cqrs` · `endpoint` · `handler` · `entity` · `aggregate` · `value-object` · `repository` · `persistence-service` · `dto` · `mapping` · `validator` · `background-job` · `consumer` · `saga` · `integration-event` · `domain-service` · `specification` · `configuration` · `caching` · `search`

Examples: `/implement api CreateCategory`, `/implement entity Product`, `/implement consumer InventoryReserved`.

`api` and `cqrs` are **composite targets** — see Execution Rules step 3.

## Reading Contract
Resolved per `<target>` against `../../framework/pattern-library.md` (always) — every target below has a pattern-library entry with its own Required/Optional/Forbidden. This table adds only what's specific to *generating* code for that target, on top of the pattern entry:

| `<target>` | Additional Required | Additional Optional |
|---|---|---|
| `api`, `cqrs`, `endpoint`, `handler` | `docs/conventions/application-coding-conventions.md`, target service's `docs/services/*.md` | `docs/reference/authorization.md` if the endpoint needs non-default auth |
| `entity`, `aggregate`, `value-object` | `docs/conventions/domain-coding-conventions.md` | `docs/workflows/add-new-domain-entity.md` if genuinely new |
| `repository`, `persistence-service`, `configuration` | `docs/conventions/persistence-coding-conventions.md` | `docs/workflows/add-new-repository.md` if genuinely new |
| `dto`, `mapping`, `validator` | `docs/04-coding-rules.md` | — |
| `background-job` | `docs/workflows/add-background-job.md` | `docs/services/auth-service.md` (Hangfire example) |
| `consumer`, `integration-event` | `docs/reference/events.md`, `docs/workflows/add-integration-event.md` | `docs/reference/inbox-outbox-runtime.md` |
| `saga` | `docs/reference/saga.md` | `docs/reference/create-order-saga.md` |
| `domain-service` | — (pattern entry already flags this as rarely-needed; confirm before implementing) | — |
| `specification` | — | **this is a stop condition, not an implementable target** — see Failure Conditions |
| `caching` | `docs/reference/caching.md`, `docs/04-coding-rules.md` (Caching section) | — |
| `search` | `docs/reference/search.md` | — |

**Always Forbidden:** the frontend repository (`NovaCoreUI`), any service other than the one named/inferable from `<Name>`'s context, `docs/_archive/**`.

## Execution Rules
1. **Resolve target and scope.** Parse `<target>` and `<Name>`; infer the target service from context (existing similar features, or ask if genuinely ambiguous).
2. **Load Rules.** Pull the relevant entries from `../../framework/rules-library.md` for every layer this target touches.
3. **Load Patterns.** Load the `<target>`'s entry (or entries, for composite targets) from `../../framework/pattern-library.md`. For `api`/`cqrs`, this means Endpoint + CQRS + Validator + Mapping + DTO patterns together — assemble the full vertical slice, not just one layer, but only create the layers that don't already exist (check Persistence first; don't regenerate an existing Read/Write service).
4. **Load Templates.** Pull the matching entries from `../../framework/template-library.md`. If a target is a documented gap (Value Object, Mapping, Saga, DTO, Configuration, Caching decorator, Search, Domain Service), open the real cited example file instead of a template.
5. **Inspect only related implementations.** Open the specific existing files the loaded pattern/template cite as ground truth (e.g. `Register.cs` for an endpoint) — never a broader source scan. This step exists to mirror actual current style (exact naming, exact exception types, exact logging calls), not to re-derive architecture from scratch.
6. **Implement.**
   - Generate full, working code for everything inferable from the target's name, the existing patterns, and the surrounding feature context: CRUD shape, standard validation (required fields, format), standard exception usage (`docs/reference/exceptions.md`), transaction wrapping, DI registration, mapping.
   - Leave a `// TODO: <specific question>` only for genuine business-rule decisions that cannot be inferred — a specific threshold, a domain-specific calculation, a business policy choice. Never a vague `// TODO: implement business logic` — state exactly what's undecided.
   - If the target is `specification`: stop per Failure Conditions instead of generating one.
   - If the target is `domain-service`: before generating, check whether the same logic fits as a method on an existing aggregate/entity instead (per the pattern entry) — if it does, implement it there and say so, don't create a Domain Service just because one was asked for.
7. **Self-review.** Run the checklist in "Self-Review" below before returning anything. Fix what it catches; never touch code outside what step 6 generated.
8. **Return the implementation** as a diff, listing every TODO left behind and exactly what decision each one is waiting on.

## Self-Review
Before returning, verify:
- [ ] **Naming** matches the exact conventions in the loaded pattern/rules docs and the real reference file — not a plausible-sounding alternative.
- [ ] **Layer consistency** — nothing crossed a boundary in `../../framework/boundaries.md`.
- [ ] **Pattern consistency** — the implementation matches the loaded Pattern Library entry, not an invented variant.
- [ ] **Architecture consistency** — dependency direction matches `docs/02-architecture-rules.md`.
- [ ] **Method organization** — matches the shape of the cited real reference file.
- [ ] **Readability** — no dead code, no commented-out alternatives, no unnecessary abstraction.
- [ ] **Production readiness** — every non-TODO path is actually complete, not a stub disguised as done.

## Boundaries
- Never executes `/commit`, `/cleanup`, `/prune`, `/verify`, or any other Skill — implementation only, the developer runs those separately.
- Never introduces a new architectural pattern, even a "better" one — always the established pattern from `pattern-library.md`.
- Never redesigns an existing system as a side effect of implementing something adjacent to it.
- Never implements a `specification` target literally — see Failure Conditions.
- Never guesses at a business rule — TODO it, with the specific open question stated.

## Limitations
- Composite targets (`api`, `cqrs`) require judgment about which sub-layers already exist vs. need creation — a misjudgment here either over-generates (recreating an existing Persistence Service) or under-generates (missing a needed one); step 3's "check Persistence first" mitigates but doesn't eliminate this.
- TODO-vs-implement judgment is inherently fuzzy at the edges — a "standard validation" for one feature may be a genuine business decision for another; when uncertain, prefer leaving a TODO over guessing, per this project's "AI should never guess" principle.
- `domain-service` and `search` have no real reference implementation in this codebase yet (per `pattern-library.md`'s gap notes) — generated code for these targets is necessarily more inferred-from-convention than mirrored-from-example, and should be reviewed more carefully than other targets.

## Expected Result
A diff containing the full generated implementation (all files created/modified), with every TODO explicitly listed alongside the specific decision it's blocked on. For `api`/`cqrs`, the diff spans every layer actually generated (only what didn't already exist).

## Failure Conditions
- `<target>` is `specification` — this project's `docs/conventions/domain-coding-conventions.md` explicitly rules out the Specification pattern. Report this, suggest expressing the filtering logic as a Read Service method instead, and do not generate a Spec-object type.
- `<target>` or `<Name>`'s owning service is ambiguous (matches more than one plausible service/feature).
- The pattern/template for `<target>` is a documented gap with no real reference example anywhere in the codebase (not even the ones with partial gap coverage) — report and ask for direction rather than inventing a shape wholesale.
- Implementing the request would require changing an already-established pattern (not just adding a new instance of it) — that's `/align` or a deliberate architecture decision, not `/implement`.

## Success Criteria
- [ ] Every generated file matches an existing pattern's Reading Contract — no file was written without first loading its Pattern/Template/Rules entries.
- [ ] No TODO is vague — each states the specific undecided question.
- [ ] Self-Review checklist passed with no unresolved item.
- [ ] No file outside the requested target's scope was touched.
- [ ] No other Skill was invoked.

## Examples

**Correct usage — full vertical slice:**
```
/implement api CreateCategory

→ Loads: Endpoint + CQRS + Validator + Mapping patterns, application-coding-conventions.md,
  Category service's docs/services/*.md, cites Auth's Register.cs-equivalent for this service.
→ Checks: no existing ICategoryWriteService — generates Command, Handler, Validator, Endpoint,
  Request/Result records, and the Write service interface/implementation (Persistence pattern).
→ Fully implements: field validation, transaction wrapping, DI registration, hand-mapping.
→ TODO left: "// TODO: should duplicate category names be rejected case-insensitively? No
  existing category feature clarifies this — confirm before removing this TODO."
```

**Correct usage — single-layer target:**
```
/implement entity Product

→ Loads Entity pattern, domain-coding-conventions.md, opens an existing entity as reference.
→ Generates the entity with private setters, static Create factory, behavior methods for every
  state change already implied by the surrounding feature context.
```

**Incorrect usage — asking for something this project doesn't do:**
```
/implement specification ActiveProductsSpec

→ "This project doesn't use the Specification pattern (see domain-coding-conventions.md) —
   not implementing one. If you need this filtering logic, it belongs as a method on
   IProductReadService, e.g. GetActiveProductsAsync(...). Want that instead?"
```

**Edge case — Domain Service redirected to an aggregate method:**
```
/implement domain-service OrderPricingCalculator

→ "The pricing logic described fits as a method on the Order aggregate itself
   (Order.CalculateTotal(...)) rather than a separate Domain Service, per this project's
   pragmatic-DDD convention of preferring aggregate methods first. Implementing it there
   instead — let me know if there's a cross-aggregate reason it must be a standalone service."
```

**Edge case — ambiguous target:**
```
/implement handler UpdateStatus

→ "UpdateStatus exists as a plausible command name in both Order and Inventory — which
   service?" (stops, does not guess)
```

## Future Extension Notes
New target types are added by first adding the construct to `../../framework/pattern-library.md` (with its own Reading Contract), then adding one row to this file's target table if it needs generation-specific reading beyond the pattern entry. If a target type consistently needs TODOs for the same reason across many invocations, that's a signal the underlying pattern/rules doc is missing the relevant rule — fix the doc, not this Skill's judgment logic.
