---
name: sync
description: Synchronize an existing NovaCore implementation with the project's current patterns, templates, and architecture rules — updates only what's actually outdated
---

## Purpose
Catch up an implementation that was correct when written but has since fallen behind — because the project's pattern, template, or an active migration moved on, not because the original code drifted from an unchanged standard (that's `/align`). Never touches parts that are already current.

## Supported Commands
`/sync <target> [Name]` where `<target>` ∈ `entity`, `aggregate`, `persistence`, `repository`, `handler`, `api`, `consumer`, `saga`, `module`. `[Name]` optional for single-file-scoped targets (uses current context if omitted, same rule as `/align`); required for `module`.

Examples: `/sync entity`, `/sync persistence`, `/sync module Product`.

## Reading Contract
- **Required:** `../../framework/pattern-library.md` (entry matching `<target>`), `../../framework/template-library.md` (matching entry), `../../framework/rules-library.md` (relevant categories), `docs/02-architecture-rules.md`
- **Optional (conditional):**
  - `docs/refactoring/README.md` + the specific `docs/refactoring/*.md` tracker, if one exists whose scope covers the target/module — when a tracker exists, its target architecture is the most authoritative source and takes precedence over inferring currency from the libraries alone
  - `docs/tasks/PROGRESS.md` — to confirm this work isn't already tracked/in-progress elsewhere
- **Forbidden:** other services/modules not named, `docs/_archive/**`

## Responsibilities
- Determine what "current" actually means for the target right now (pattern/template/rules libraries, and an active tracker if one applies).
- Diff the existing implementation against that current standard.
- Update only the parts that are genuinely outdated.

Explicitly **not** this skill's responsibility: fixing drift from a standard that *hasn't* changed (`/align`), writing new capability (`/implement`), finishing incomplete code (`/complete`).

## Execution Workflow
1. Resolve `<target>` and `[Name]`/current-context, same ambiguity rule as `/align`.
2. Load the target's Pattern/Template/Rules entries — this is "current" by default.
3. Check `docs/refactoring/` for an active tracker covering this target. If one exists, its target-architecture section supersedes the library entries as the authoritative "current" for this specific migration's scope.
4. Check `docs/tasks/PROGRESS.md` to confirm this isn't duplicate work already in flight.
5. Diff the existing implementation against whichever "current" was resolved in steps 2–3.
6. Classify each difference:
   - **Outdated** — implements a pattern/shape that's since been superseded; needs updating.
   - **Compliant** — already matches current, even if it could be written differently; leave alone.
   - **Stylistic** — differs but is not actually non-compliant (a valid variation the pattern doc allows); leave alone.
7. Update only what's classified Outdated.
8. If a tracker was involved, update its checklist/risk register **in this same change**, per `docs/refactoring/README.md`'s own convention — not deferred.
9. Self-review per the checklist below.
10. Return the diff + what was updated and why + what was checked and left alone and why.

## Self-Review
Before returning: every change corresponds to an actual Outdated classification, not a stylistic preference; nothing classified Compliant or Stylistic was touched; if a tracker was involved, its checklist/risk register reflects the change in the same diff; no business behavior changed as a side effect of the pattern update (if the new pattern genuinely requires a behavior change, that's flagged explicitly, not silently bundled in).

## Rules
- "Already compliant" is the default assumption — a difference from the newest possible style is not automatically Outdated; it must actually violate the current Pattern/Template/Rules/tracker entry.
- When both a tracker and the general libraries apply and disagree, the tracker wins for its stated scope (it's the more specific, deliberately-scoped source) — but this disagreement itself is worth reporting, since it may mean the tracker's outcome hasn't been folded back into the libraries yet (see `docs/refactoring/README.md`'s "once complete, migrates to conventions/" lifecycle).
- One module/target per invocation — even if a sibling module has the same outdated pattern, sync it separately.

## Boundaries
- Never touches a module/target other than the one named.
- Never rewrites something already compliant, even if a rewrite would look "more current" stylistically.
- Never performs the tracker's own "migrate to conventions/" lifecycle step — only flags when a tracker looks complete for this target's portion.
- Never invokes `/implement`, `/complete`, `/align`, `/review`, `/verify`, `/cleanup`, `/commit`, or `/prune`. No hidden pipeline, no autonomous chaining.

## Limitations
- Currency is only as good as the Pattern/Template/Rules libraries and trackers being accurate and up to date — if a library entry has drifted from actual current practice (a documentation gap), this skill will sync *toward* the stale doc unless the gap is caught first; when the loaded pattern and the majority of similar real code disagree, that discrepancy should be reported rather than resolved silently in either direction.
- Distinguishing "outdated" from "a valid stylistic variation" requires the Pattern Library entry to actually state which variations are allowed — for gap entries (no template exists), this judgment is inherently softer and should be treated more conservatively (prefer leaving code alone when uncertain).

## Expected Result
Diff (only the Outdated parts changed) + updated tracker checklist/risk-register section (if a tracker applied) + an explicit list of what was checked and left alone with the reason (proves the "don't rewrite compliant code" rule was actually followed, not just assumed).

## Failure Conditions
- No active tracker and the Pattern/Template library entries show no meaningful difference from the current implementation — report "already current," make no changes.
- `[Name]`/module required but ambiguous or missing.
- A tracker's target architecture and the general library entries conflict in a way that can't be resolved by "tracker wins for its scope" (e.g. the tracker's scope doesn't clearly include this target) — report the conflict, don't guess which one applies.
- `docs/tasks/PROGRESS.md` shows this exact work already tracked as in-progress elsewhere — report the existing tracked item instead of duplicating effort.

## Success Criteria
- [ ] Every change maps to a specific Outdated classification against a named current source (library entry or tracker).
- [ ] Nothing Compliant or Stylistic was modified.
- [ ] Tracker state (if applicable) updated in the same diff.
- [ ] Report includes both what changed and what was checked-and-left-alone.
- [ ] No other Skill was invoked.

## Examples

**Correct usage — tracker-driven:**
```
/sync persistence
(current context: Inventory.Persistence, which still uses the old single repository-does-
everything shape; docs/refactoring/persistence-refactor-plan.md tracks the Read/Write service
split migration and lists Inventory as not-yet-migrated)

→ Tracker found, takes precedence. Diffs current repository against the tracker's target
  shape (Read/Write service split).
→ Updates: splits into IInventoryReadService/IInventoryWriteService per the tracker's target
  architecture and the Persistence Service pattern.
→ Updates persistence-refactor-plan.md's checklist: marks Inventory's row complete, in the
  same diff.
```

**Correct usage — library-driven, no tracker:**
```
/sync entity
(current context: an entity written before domain-coding-conventions.md's reusable Value
Object validation section was added; it still inline-validates an email field instead of
using the Email Value Object other entities now use)

→ No tracker applies. Library comparison shows this entity predates the now-standard Value
  Object pattern for validated primitives.
→ Updates the inline validation to use the Email Value Object, matching current sibling
  entities.
```

**Incorrect usage — asking it to also improve unrelated style:**
```
User: /sync repository, and clean up the naming while you're in there
→ "Naming that isn't actually outdated is /align's job, not /sync's — /sync only updates what
   changed. Run /align separately for naming."
```

**Edge case — already current:**
```
/sync handler
(current context: a handler already matching every current Pattern/Template/Rules entry)

→ "Already current — no tracker applies, and this handler matches every current library entry.
   No changes made."
```

**Edge case — tracker/library conflict:**
```
/sync module Warehouse
(refactoring/persistence-refactor-plan.md doesn't explicitly list Warehouse in its scope, but
the general Persistence Service pattern implies Warehouse should already be split)

→ "Warehouse isn't explicitly in persistence-refactor-plan.md's scope, but the general pattern
   suggests it should already comply — unclear whether this is an intentional scope gap in the
   tracker or an oversight. Reporting, not guessing which applies."
```

## Future Extension Notes
When a `docs/refactoring/*.md` tracker completes and its standard migrates into `docs/conventions/*.md` per that doc's own lifecycle, update the corresponding `pattern-library.md`/`rules-library.md` entry to point at the new conventions doc — after that, `/sync` for that module no longer needs the tracker-check step, the library entries alone are sufficient "current."
