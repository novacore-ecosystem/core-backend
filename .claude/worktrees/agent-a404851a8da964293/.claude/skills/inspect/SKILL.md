---
name: inspect
description: Single-aspect deep-dive inspection of NovaCore code — performance, security, dependency, transaction, caching, architecture, cqrs, ddd, event, or persistence — current state, risks, recommendations
---

## Purpose
Answer a narrow, specific question in depth ("is the caching here correct?", "does this have a transaction boundary problem?") rather than the broad sweep `/verify` or `/review` perform. One aspect, thoroughly, with an honest account of what's actually known vs. a documented gap in this project's standards.

## Supported Commands
`/inspect <aspect> [target]` where `<aspect>` ∈ `performance`, `security`, `dependency`, `transaction`, `caching`, `architecture`, `cqrs`, `ddd`, `event`, `persistence`.

`[target]` optional — defaults to the code currently in view/discussion in this session; if there's no unambiguous current context and none was named, that's a Failure Condition.

Examples: `/inspect performance`, `/inspect security`, `/inspect dependency`, `/inspect transaction`, `/inspect caching`, `/inspect architecture`, `/inspect cqrs`, `/inspect ddd`, `/inspect event`, `/inspect persistence`.

## Reading Contract
| `<aspect>` | Required | Note |
|---|---|---|
| `performance` | `docs/workflows/performance-optimization.md` | `rules-library.md` gap — this is an investigation workflow, not a binding rules doc; findings here are diagnostic, not rule-violation citations |
| `security` | `docs/reference/authorization.md` | `rules-library.md` gap beyond authZ — findings outside authN/authZ policy have no dedicated doc to cite; say so rather than inventing a standard |
| `dependency` | `docs/02-architecture-rules.md`, `../../framework/boundaries.md` | — |
| `transaction` | `docs/conventions/persistence-coding-conventions.md`, `docs/04-coding-rules.md` (Transaction Management section) | — |
| `caching` | `docs/reference/caching.md`, `docs/04-coding-rules.md` (Caching section), `../../framework/pattern-library.md` (Caching entry) | — |
| `architecture` | `docs/01-architecture-map.md`, `docs/02-architecture-rules.md` | — |
| `cqrs` | `docs/conventions/application-coding-conventions.md`, `docs/04-coding-rules.md` (CQRS shape section), `../../framework/pattern-library.md` (CQRS entry) | — |
| `ddd` | `docs/conventions/domain-coding-conventions.md`, `../../framework/pattern-library.md` (Entity/Aggregate/Value Object entries) | — |
| `event` | `docs/reference/events.md`, `docs/reference/inbox-outbox-runtime.md` | — |
| `persistence` | `docs/conventions/persistence-coding-conventions.md`, `../../framework/pattern-library.md` (Repository/Persistence Service entries) | — |

`../../framework/inspection-output-standard.md` is Required for every aspect. **Forbidden, always:** any aspect other than the one requested (a `/inspect caching` invocation does not also comment on naming), other services/modules not part of the resolved target.

## Responsibilities
- Establish the current state of the target with respect to the one requested aspect.
- Detect issues and risks specific to that aspect.
- Recommend a specific fix path.

Explicitly **not** this skill's responsibility: a multi-aspect sweep (`/verify`), holistic quality scoring (`/review`), fixing anything.

## Inspection Workflow
1. Resolve `<aspect>` and `[target]` (or current context). If unresolvable, stop.
2. Load the matching Reading Contract row, plus `inspection-output-standard.md`.
3. Establish **Current State** — what the target actually does today with respect to this aspect, factually, before judging it.
4. Detect **Issues** — deviations from the loaded doc's stated rule/pattern for this aspect. If the aspect's row notes a `rules-library.md` gap (performance, most of security), issues here are framed as *diagnostic observations against the nearest partial coverage*, not as rule violations with a Rule Reference — be explicit about this distinction in the output.
5. Assess **Potential Risks** — what happens if this isn't addressed, stated concretely (not "could be a problem," but the specific scenario).
6. Produce **Recommendations** — the specific fix, referencing the established pattern where one exists.
7. Classify any issue found using `inspection-output-standard.md`'s severity model where a real Rule Reference exists; for gap-aspect findings, use the severity model's own rule — no Rule Reference means capped at Suggestion, stated plainly as "diagnostic, not a compliance finding."
8. Return the report per `../../templates/inspect-report-template.md`.

## Rules
- Stay inside the one requested aspect — a caching inspection that notices a naming issue notes it isn't in scope rather than reporting it (or, at most, appends it as a one-line aside clearly marked out-of-scope, never folded into the aspect's own findings).
- For gap aspects (performance, security-beyond-authZ), never assign Critical/High severity — the absence of a documented rule means, per `inspection-output-standard.md`'s own rule, the finding caps at Suggestion regardless of how convinced the inspection is.

## Boundaries
- Never modifies code.
- Never expands beyond the one requested aspect.
- Never invokes `/verify`, `/review`, `/align`, `/sync`, `/implement`, `/complete`, `/cleanup`, `/commit`, or `/prune`. No hidden pipeline, no autonomous chaining.

## Limitations
- For `performance` and `security`, the absence of a dedicated rules doc (per `rules-library.md`'s documented gaps) means this inspection's authority is inherently weaker than for a fully-documented aspect like `transaction` or `caching` — treat its findings as expert-informed observations, not binding standards.
- Single-aspect framing means real issues in an adjacent aspect go unreported by design — `/inspect caching` will not catch a transaction-boundary bug even if one exists in the same code; that's why `/verify` exists for full-target coverage.

## Severity Model
See `../../framework/inspection-output-standard.md` — used for any issue with a real Rule Reference; gap-aspect findings are explicitly capped at Suggestion per that doc's own rule.

## Expected Result
A report per `../../templates/inspect-report-template.md`: Current State, Detected Issues (severity-classified where applicable, explicitly marked diagnostic-only where not), Potential Risks, Recommendations.

## Failure Conditions
- No resolvable target (no `[target]` given, no unambiguous current context).
- Requested `<aspect>` isn't one of the ten supported — report the supported list rather than attempting a best-guess aspect.

## Success Criteria
- [ ] Findings stay within the one requested aspect.
- [ ] Gap-aspect findings are explicitly marked diagnostic, never given a fabricated Rule Reference.
- [ ] Current State is stated factually before any judgment.
- [ ] Recommendations are specific, not generic advice.
- [ ] No code was modified, no other Skill was invoked.

## Examples

**Correct usage — fully-documented aspect:**
```
/inspect transaction
(current context: CancelOrderHandler.cs)

→ Current State: the handler wraps the restock + status-update mutation in
  unitOfWork.ExecuteTransactionAsync at the handler level; the Write service methods
  themselves don't open transactions.
→ Issues: none — matches persistence-coding-conventions.md exactly.
→ Recommendations: none needed.
```

**Correct usage — gap aspect:**
```
/inspect performance
(current context: GetProductList query, no pagination)

→ Current State: query loads all products with no pagination or limit.
→ Issues (diagnostic, no rules-library.md rule to cite — performance-optimization.md is an
  investigation workflow, not a binding standard): unbounded result set is a plausible
  scalability risk as the catalog grows.
→ Potential Risks: response time degradation and memory pressure under a large catalog;
  concrete threshold unknown without a load test.
→ Recommendations: add pagination, following the shape already used in [cite an existing
  paginated query if one exists in this service].
→ Severity: Suggestion (capped — no dedicated performance rules doc exists to cite higher).
```

**Edge case — no resolvable target:**
```
/inspect caching
(fresh session, no target named, no prior context)

→ "No target to inspect — name a file/feature or run this while a specific implementation is
   in view." (asks, does not guess)
```

**Edge case — unsupported aspect:**
```
/inspect logging
→ "logging isn't one of the ten supported aspects (performance, security, dependency,
   transaction, caching, architecture, cqrs, ddd, event, persistence) — did you mean one of
   these, or is this a candidate for a new aspect?" (see Future Extension Notes)
```

## Future Extension Notes
A new aspect is added as one row in this file's Reading Contract table, pointing at the relevant `docs/` doc(s) — if the aspect has no dedicated rules doc yet, mark it a gap explicitly (matching the `performance`/`security` precedent) rather than silently treating it as fully authoritative. If `rules-library.md` later fills the Performance/Security/Logging/Concurrency gaps, remove the corresponding gap note here — the aspect's authority upgrades automatically, no other change needed.
