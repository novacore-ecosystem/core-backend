---
name: verify
description: Production readiness verification for NovaCore — deterministic rule/architecture/pattern violation detection with severity classification, across any target from a single handler to the whole solution
---

## Purpose
Answer, with evidence: is this implementation aligned with project architecture, consistent with project patterns, free of rule violations, and safe to deploy? Verify checks **compliance** against documented standards — it does not judge subjective engineering quality (that's `/review`) and it does not fix anything (that's `/align`/`/sync`/`/implement`, run separately by the developer).

## Supported Commands
`/verify <target> [Name]` where `<target>` ∈ `api`, `flow`, `handler`, `entity`, `aggregate`, `value-object`, `repository`, `persistence`, `consumer`, `saga`, `module`, `feature`, `solution`.

`[Name]` follows the same rule as `/align`/`/sync`: optional for single-file-scoped targets (uses current context if omitted), required for `flow`/`module`/`feature`. `solution` takes no name — it means the whole repository, and is the one target where broad reading is legitimate because it was explicitly requested by name, not assumed.

## Reading Contract
| `<target>` | Required | Forbidden |
|---|---|---|
| `api`, `handler` | `docs/02-architecture-rules.md`, `docs/04-coding-rules.md`, `docs/conventions/application-coding-conventions.md`, `../../framework/pattern-library.md` (CQRS/Endpoint entries), target service's `docs/services/*.md` | other services, unrelated Features |
| `entity`, `aggregate`, `value-object` | `docs/02-architecture-rules.md`, `docs/conventions/domain-coding-conventions.md`, `../../framework/pattern-library.md` (matching entry) | Persistence/API/Infrastructure source |
| `repository`, `persistence` | `docs/conventions/persistence-coding-conventions.md`, `docs/04-coding-rules.md` (Repository/Transaction sections), `../../framework/pattern-library.md` (Repository/Persistence Service entries) | Domain business rules beyond public surface, API/UI |
| `consumer` | `docs/reference/events.md`, `../../framework/pattern-library.md` (Consumer entry) | business-logic judgment beyond adapter correctness |
| `saga` | `docs/reference/saga.md`, `docs/reference/create-order-saga.md` | — |
| `flow <Name>` | `docs/01-architecture-map.md`, each involved service's `docs/services/*.md` | services outside the named flow |
| `module <Name>` / `feature <Name>` | the feature folder's own layer conventions (domain/application/persistence as applicable), `docs/02-architecture-rules.md` | other features |
| `solution` | `docs/01-architecture-map.md`, `docs/02-architecture-rules.md` — then, per service, the same per-target rows above, iterated one service at a time | frontend repository |

`../../framework/inspection-output-standard.md`, `../../framework/rules-library.md`, `../../framework/boundaries.md` are Required for every target, no exception.

## Responsibilities
- Compile-check the affected code, scoped to the target (never a full-solution build unless `<target>` is literally `solution`).
- Detect and classify every rule/architecture/pattern violation in scope, per `inspection-output-standard.md`.
- State plainly whether the target is safe to deploy as-is.

Explicitly **not** this skill's responsibility: fixing anything found (`/align`, `/sync`, `/implement`), judging subjective quality beyond compliance (`/review`), a single-aspect deep dive (`/inspect`).

## Inspection Workflow
1. Resolve `<target>`/`[Name]`, same ambiguity rule as `/align`/`/sync`. For `solution`, no resolution needed — scope is everything, but still processed per-service (step 3), never as one undifferentiated pass.
2. Load the matching Reading Contract row(s). For `solution`, iterate service by service, loading only that service's relevant rows per iteration.
3. **Build check:** run a scoped `dotnet build` for the target's affected project(s) (full solution build only for the `solution` target itself). If it fails, that's an automatic **Critical** finding — report it first, and note that findings below the build are best-effort since non-compiling code can hide other issues.
4. **Violation scan**, checked against every category the loaded docs cover:
   - Architecture violations (dependency direction, layer placement)
   - Rule violations (`rules-library.md` entries for this target's layer)
   - Pattern violations (`pattern-library.md` entry mismatch)
   - Dependency/layer violations (`boundaries.md`)
   - Incorrect abstractions/responsibilities (wrong construct for the job — e.g. logic in an Endpoint that belongs in a Handler)
   - Missing validation
   - Incorrect transaction boundaries (per persistence conventions — transaction owned by the caller, not the Write service)
   - Incorrect repository usage (bypassing Read/Write services, direct `DbContext` access from Application)
   - Incorrect mapping (dropped fields, wrong direction)
   - Incorrect exception handling (wrong type per `docs/reference/exceptions.md`)
   - Incorrect logging
   - Incorrect caching (per `docs/reference/caching.md`)
   - Incorrect event publishing (direct publish instead of Outbox)
   - Incorrect DI registration
   - Naming inconsistencies
   - Folder organization issues
   - Method organization issues
   - Code duplication
   - Dead branches
   - Readability/maintainability problems that rise to a documented rule, not just taste
   - Production risks, technical debt
5. Classify every finding per `inspection-output-standard.md`'s severity model (Critical/High/Medium/Low/Suggestion) — never assign Critical/High without a cited Rule Reference.
6. Report via `ReportFindings`, one entry per finding, most-severe first; encode severity as a `[Severity]` prefix in `short_summary`/`category` since the tool has no dedicated severity field. Then compose the full narrative report (Overview/Summary/Detected Issues/Conclusion) per `inspection-output-standard.md` around those findings.
7. State the deploy verdict plainly in the Conclusion: ready, ready-with-caveats, or not ready — never a vague "looks mostly fine."

## Rules
- No finding is Critical or High without a citable Rule Reference (`inspection-output-standard.md`'s own rule) — anything else caps at Suggestion.
- Build failures always outrank every other finding.
- `solution`-scope findings must still be reported per-service, not merged into one undifferentiated list — a developer working on Order shouldn't have to parse Inventory findings to find theirs.

## Boundaries
- Never modifies code — findings and recommendations only.
- Never executes `/align`, `/sync`, `/implement`, `/complete`, `/review`, `/inspect`, `/cleanup`, `/commit`, or `/prune`. No hidden pipeline, no autonomous chaining.
- Never runs a full-solution build except for the explicit `solution` target.
- Never invents a rule to justify a finding — a suspicion with no documented backing is, at most, a Suggestion.

## Limitations
- `solution`-scope verification is inherently the slowest and most expensive invocation — legitimate given it's explicitly requested, but the per-service iteration in step 2 is a mitigation, not a full fix, for the context-minimization principle this framework otherwise enforces everywhere else.
- Detecting "incorrect abstraction/responsibility" requires the loaded convention docs to actually state what the correct one is — for the documented pattern-library gaps (Domain Service, Search), this check is inherently softer.
- Duplication/dead-branch detection is scoped to the target, same limitation as `/prune`'s scoped search — not a full-repo dedup analysis.

## Expected Result
A report per `../../framework/inspection-output-standard.md`: Overview, Summary, Detected Issues (via `ReportFindings`, most-severe first), Conclusion with an explicit deploy verdict. Build result reported first if it failed.

## Failure Conditions
- Ambiguous target/name with no resolvable current context.
- Build fails — report as Critical and continue with best-effort static findings, clearly labeled as such.
- No documented rule/pattern covers the target's construct at all (a true pattern-library gap with zero real example) — report that as a finding-worthy gap itself, not silence.

## Success Criteria
- [ ] Every Critical/High finding cites a real Rule Reference.
- [ ] Findings are ordered most-severe first.
- [ ] Build result (if applicable) is reported before static findings.
- [ ] `solution`-scope output is segmented per service.
- [ ] Conclusion states an explicit, unambiguous deploy verdict.
- [ ] No code was modified, no other Skill was invoked.

## Severity Model
See `../../framework/inspection-output-standard.md` — this skill uses it exactly as defined, no local variant.

## Examples

**Correct usage — single handler:**
```
/verify handler
(current context: CreateWarehouseHandler.cs)

→ Build: pass.
→ [High] Handler calls Inventory.Persistence's DbContext directly instead of
  IWarehouseWriteService. Rule Reference: conventions/persistence-coding-conventions.md
  (Read/Write service pattern). Recommendation: route through IWarehouseWriteService per
  pattern-library.md's Persistence Service entry. Expected benefit: restores the abstraction
  boundary that lets persistence implementation change without touching Application.
→ Conclusion: not ready — one High finding blocks; otherwise compliant.
```

**Correct usage — solution scope:**
```
/verify solution
→ Full solution build: pass.
→ Iterates Auth, User, Product, Inventory, Order, Audit, Notification, Gateway one at a time,
  each with only that service's relevant docs loaded.
→ Reports findings grouped under per-service headings.
```

**Edge case — no rule covers the construct:**
```
/verify persistence
(target uses the Search pattern, which is a documented pattern-library gap)

→ "Search has no literal template/full rule doc yet (pattern-library.md gap) — flagging the
   implementation as generally consistent with the one cited example in reference/search.md,
   but noting this verification is softer than for a fully-documented construct."
```

## Future Extension Notes
New verifiable targets are added the same way as `/implement`'s and `/align`'s — a Reading Contract row here, sourced from `pattern-library.md`/`rules-library.md`. If a new violation category is needed, add it to the Inspection Workflow step 4 list; severity assignment logic doesn't change, since it's already rule-citation-driven rather than category-specific.
