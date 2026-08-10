---
name: align
description: Bring existing NovaCore code (AI-generated, legacy, or inconsistent) back to production standards — structure only, never business behavior
---

## Purpose
Restore an already-working piece of code to this project's official production standards — naming, layer responsibilities, method organization, logging/exception style — without changing what it does. Intended target: AI-generated code, legacy code, or anything that's drifted from convention, not a general-purpose refactoring tool for otherwise-fine code.

## Supported Commands
`/align <target> [Name]` where `<target>` ∈ `api`, `flow`, `handler`, `entity`, `aggregate`, `persistence`, `repository`, `dto`, `validator`, `mapping`, `consumer`, `saga`, `module`.

`[Name]` is optional for a single-file-scoped target (`handler`, `api`, `persistence`, ...) — when omitted, the target is the code currently in view/discussion in this session, not a repo-wide sweep; if there's no unambiguous current context, that's a Failure Condition, not a license to scan broadly. `[Name]` is required for `flow` and `module` (e.g. `/align flow Checkout`, `/align module Warehouse`) since those span multiple files and need an explicit boundary.

Examples: `/align handler`, `/align api`, `/align persistence`, `/align flow Checkout`.

## Reading Contract
| `<target>` | Required | Forbidden |
|---|---|---|
| `api`, `handler` | `docs/04-coding-rules.md`, `docs/conventions/application-coding-conventions.md`, target service's `docs/services/*.md`, `docs/workflows/refactor-existing-code.md` | other services, unrelated Features |
| `entity`, `aggregate` | `docs/02-architecture-rules.md`, `docs/conventions/domain-coding-conventions.md`, `docs/workflows/refactor-existing-code.md` | Persistence/API/Infrastructure source |
| `persistence`, `repository` | `docs/conventions/persistence-coding-conventions.md`, `docs/04-coding-rules.md` (Repository/Transaction sections), `docs/workflows/refactor-existing-code.md` | Domain business rules beyond public surface, API/UI |
| `dto`, `validator`, `mapping` | `docs/04-coding-rules.md` (Naming/Validation/Mapping sections) | — |
| `consumer` | `docs/reference/events.md` | business-logic invention inside the consumer |
| `saga` | `docs/reference/saga.md` | — |
| `flow <Name>` | `docs/01-architecture-map.md`, each involved service's `docs/services/*.md`, `docs/02-architecture-rules.md` | services outside the named flow |
| `module <Name>` | the feature folder's own layer conventions (whichever of domain/application/persistence conventions apply), `docs/workflows/refactor-existing-code.md` | other features |

Also see `../../framework/boundaries.md` for the general per-layer table these rows specialize.

## Responsibilities
- Bring the named target's structure, naming, and style in line with the conventions above.
- Reorganize methods, extract responsibilities, fix logging/exception style, fix layer misplacement.

Explicitly **not** this skill's responsibility: writing new features (`/implement`), finishing incomplete code (`/complete`), catching up to a pattern that's changed since the code was written (`/sync` — the distinction: `/align` fixes drift from *already-current* standards; `/sync` catches up to standards that moved).

## Execution Workflow
1. Resolve the target: parse `<target>` and `[Name]`. If `[Name]` is required and missing, or the current-context target is ambiguous, stop.
2. Load the matching Reading Contract row.
3. Locate the target's actual source via targeted search — never a full-repo scan, even for `module`.
4. Diff current implementation against the loaded convention doc(s); list every structural violation found.
5. Refactor to close each violation. Every change must be one of: method organization, naming, layer responsibility, method extraction, readability, consistency, logging style, exception style, coding convention alignment. Anything else — a new architecture, an unrelated improvement, a business-behavior change, a full feature rewrite — is out of scope; skip it and note it as out of scope in the output rather than doing it anyway.
6. Run `docs/workflows/refactor-existing-code.md`'s existing Safety / SOLID / Reuse / Regression checklists as the validation gate — reused, not recreated.
7. Self-review per the checklist below.
8. Return the diff, the violation list (mapped to which convention each violated), and the four checklist results.

## Self-Review
Before returning: every change is traceable to a specific violation found in step 4; no business-observable behavior changed (same inputs → same outputs, same exceptions thrown for the same conditions); no file outside the resolved target's scope was touched; naming/layer/pattern/architecture consistency achieved per `docs/workflows/refactor-existing-code.md`'s checklists.

## Rules
- A change that alters what the code *does* (not just how it's organized) is not an alignment — that's a Failure Condition, escalate rather than perform it.
- "Module" scope still means surgical, per-violation fixes — not a rewrite of the feature, even if a rewrite would arguably be cleaner.
- If aligning would require introducing a pattern not already established elsewhere in the codebase, that's out of scope — flag it as a candidate for a deliberate architecture decision, don't introduce it unilaterally.

## Boundaries
- Never introduces a new architectural pattern.
- Never optimizes code unrelated to the resolved target, even adjacent code in the same file.
- Never changes business behavior.
- Never rewrites an entire feature — every change maps to a specific, named violation.
- Never invokes `/implement`, `/complete`, `/sync`, `/review`, `/verify`, `/cleanup`, `/commit`, or `/prune`. No hidden pipeline, no autonomous chaining.

## Limitations
- "No behavior change" is verified by review, not by an automated equivalence proof — a refactor that subtly changes behavior (e.g. reordering operations that have a side-effect-order dependency) is a real risk this skill mitigates via the Regression checklist, not eliminates.
- Current-context resolution (when `[Name]` is omitted) depends on there being an unambiguous file/code already in view — in a fresh session with no prior context, this always requires an explicit target.
- `flow` and `module` scope both require the invoker to know service/feature boundaries well enough to name them correctly — a wrong or too-broad `[Name]` risks touching more than intended; the skill checks against `docs/01-architecture-map.md`/`boundaries.md` but can't catch every mis-scoping.

## Expected Result
Diff (changed code only) + a violation list (each mapped to the convention doc/section it violated) + the four `refactor-existing-code.md` checklist results (pass/fail per item) + a note of anything found but explicitly left out of scope.

## Failure Conditions
- `[Name]` required but missing, and no unambiguous current-context target exists.
- The fix would require touching a layer outside the target's Reading Contract row (e.g. an `entity` alignment that seems to need a Persistence change too) — report that the target needs a broader/different `/align` invocation, don't silently expand.
- A found issue can't be fixed without changing observable behavior — report it, don't fix it under this command.
- The target is already fully compliant — report that plainly, make no changes.

## Success Criteria
- [ ] Every change maps to a specific, named violation.
- [ ] No business-observable behavior changed.
- [ ] Only the resolved target's files were touched.
- [ ] All four `refactor-existing-code.md` checklists pass.
- [ ] No other Skill was invoked.

## Examples

**Correct usage — single handler:**
```
/align handler
(current context: CreateWarehouseHandler.cs, previously AI-generated with logic inlined in the
endpoint instead of properly extracted)

→ Violations found: business logic partially living in the endpoint instead of the handler
  (application-coding-conventions.md Handler Philosophy), inconsistent exception type for a
  not-found case (should be NotFoundException per reference/exceptions.md, was a raw
  InvalidOperationException).
→ Fixes: moves the misplaced logic into the handler, corrects the exception type. Does not
  change what happens when a warehouse isn't found — same HTTP status, same message shape.
→ Checklists: Safety ✓ SOLID ✓ Reuse ✓ Regression ✓
```

**Correct usage — flow:**
```
/align flow Checkout

→ Loads 01-architecture-map.md + services/{order,inventory,product}.md for the services in
  the Checkout flow.
→ Finds inconsistent logging style between Order's and Inventory's consumers for the same
  event type; aligns Inventory's to match the established style.
→ Does not touch Product service — not part of the Checkout flow's boundary.
```

**Incorrect usage — asking for behavior change:**
```
User: /align persistence, and also make it retry on transient failures
→ "Adding retry logic is a behavior change, not an alignment — /align only restructures,
   it doesn't add capability. That'd need /implement or a deliberate change; not doing it here."
```

**Edge case — already compliant:**
```
/align repository
(current context: a repository that already matches the empty-marker-interface convention
exactly)

→ "No violations found — this repository already matches the documented convention. No
   changes made."
```

**Edge case — ambiguous scope with no current context:**
```
/align handler
(fresh session, no prior file discussed)

→ "No current context to resolve 'handler' to a specific file, and no name was given —
   which handler?" (asks, does not guess or scan broadly)
```

## Future Extension Notes
New alignable targets are added the same way as `/implement`'s — add a Reading Contract row here pointing at the relevant convention doc(s); reuse `docs/workflows/refactor-existing-code.md`'s checklists as the validation gate rather than inventing a new one per target.
