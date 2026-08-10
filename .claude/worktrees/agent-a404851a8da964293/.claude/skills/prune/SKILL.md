---
name: prune
description: Detect and safely remove obsolete implementation leftovers from NovaCore's current working changes before commit
---

## Purpose
Catch dead code left behind by the current change set — before it gets committed — without performing a full repository audit or risking removal of anything whose safety isn't certain.

## Responsibilities
- Inspect the current working changes (staged + unstaged) and their directly related references.
- Classify every candidate dead-code finding by confidence: SAFE, LOW, MEDIUM, HIGH, UNKNOWN.
- Automatically remove SAFE items only.
- Report every other confidence level for manual confirmation.

Explicitly **not** this skill's responsibility: a full-repository dead-code audit, judging code *quality* beyond dead-code detection (`/review`), grouping/committing (`/cleanup`, `/commit`).

## Input
`/prune` — no arguments. Always scoped to the current working changes (staged + unstaged), never to an arbitrary target path.

## Reading Contract
- **Required:** `git diff` (staged + unstaged, to identify what the current change actually introduced or left behind), `git diff --stat` (to bound scope), `../../framework/boundaries.md`, `docs/04-coding-rules.md` (DI registration section — Scrutor auto-scan vs. explicit registration changes what "no direct reference" means)
- **Optional (conditional):**
  - Changed files include Domain entities/interfaces → `docs/conventions/domain-coding-conventions.md` (to recognize deliberately-empty marker types, see Rules)
  - Changed files include Persistence repositories → `docs/06-implementation-templates.md` — "Repository + Read/Write persistence service" section (same marker-interface exclusion)
  - Changed files include event consumers/publishers → `docs/reference/events.md` (topic-string/reflection-based wiring isn't visible to a plain reference search)
- **Forbidden:** any file outside the changed files' own project and its immediate direct dependents/dependencies — no whole-repository scan, no scanning services unrelated to the changed project, no frontend repo

## Execution Rules
1. Run `git diff --stat` (staged + unstaged) to enumerate changed files and bound the search scope to those files' own project(s) plus whatever directly implements/consumes a changed public contract.
2. For each changed file, identify newly-introduced or now-orphaned symbols: methods (including private), classes, interfaces, DTOs, validators, mapping code, service/repository types, endpoints, DI registrations, `using` directives.
3. For each candidate, search for references **scoped** to: the same file, the same project, and — only for symbols with `public`/`internal` visibility that could cross a project boundary — direct consumers already identifiable from the changed files themselves (e.g. an interface's known implementers in the same project). Do not chase references into services or projects the current change didn't touch.
4. Check exclusions in Rules before classifying anything as dead — a symbol matching an exclusion is never flagged, regardless of reference count.
5. Classify every remaining candidate per the Confidence Levels table in Rules.
6. Remove SAFE items directly (this is the one class of change this skill performs, not just reports).
7. Report every LOW/MEDIUM/HIGH/UNKNOWN item via `ReportFindings` — file, line, symbol, confidence, and the specific search scope that produced that confidence level (so the human can judge false-negative risk).
8. State explicitly what was removed and what needs confirmation — never blur the two into one list.

## Rules

### Confidence levels
| Level | Meaning | Auto-remove? |
|---|---|---|
| **SAFE** | Unused `using` directives only — compiler-verifiable, zero semantic risk, no cross-file search needed | Yes |
| **LOW** | A reference was found, but only in a place that makes it ambiguous whether it's real usage (e.g. test-only reference, or a string-keyed/reflection-based lookup) | No |
| **MEDIUM** | No reference found within the changed project, but the symbol is public/exported and could plausibly be referenced by a service or project outside this skill's scoped search | No |
| **HIGH** | No reference found anywhere within the full scoped search, private/internal visibility, not attached to DI/reflection/serialization | No — still requires confirmation, because the search is intentionally scoped, not exhaustive |
| **UNKNOWN** | Reference status genuinely couldn't be determined (dynamic dispatch, Scrutor-style convention-based DI scanning, config-key-based lookup) | No |

### Exclusions — never flag these, at any confidence level
- Empty repository marker interfaces (`I{Entity}Repository` with the "Leave empty for now... Reserved for future scaling" convention from `docs/06-implementation-templates.md`) — absence of members or usages is the deliberate, documented shape, not dead code.
- EF Core's required private parameterless constructor on entities.
- Types registered via Scrutor's convention-based scan (`AddScopedByInterface`, `AddScopedByInterfaceAndConcrete`) — a lack of a direct `new TypeName()` or explicit `AddScoped<T>()` reference is expected for these, not evidence of being unused; check `docs/04-coding-rules.md`'s DI section before flagging any type that looks unregistered.
- Interface members required by the interface's contract, even if the currently-changed implementation doesn't yet exercise every member.
- Anything outside the current change's scope, even if it looks dead while scanning nearby code — that's a `/prune`-worthy finding for a *future* invocation when that code is actually part of a change, not this one.

### Duplicate helper detection
Scoped to the same file/class/feature folder as a changed helper method — compare a newly-added or newly-modified helper against existing helpers already in that scope. Do not compare against helpers in unrelated features or services.

## Boundaries
- Never performs a full-repository audit — scope is always the current change plus its directly identifiable references.
- Never auto-removes anything above SAFE, regardless of how confident the detection logic is.
- Never modifies logic to "fix" a finding — only removes code, never rewrites it.
- Never invokes `/review`, `/cleanup`, `/commit`, or `/verify` itself. No hidden pipeline, no autonomous chaining — it reports what those commands might additionally catch, it doesn't run them.

## Limitations
- A scoped reference search can produce false HIGH/UNKNOWN-should-be-LOW results when a symbol is consumed by a service this invocation didn't scan — this is why even HIGH confidence never auto-removes.
- Dynamic/reflection-based usage (attribute-driven serialization, convention-based DI, string-keyed config lookups) is inherently hard to detect by static reference search; when in doubt, the skill must classify UNKNOWN rather than guess HIGH.
- Duplicate-helper detection is heuristic (name/signature/body similarity within scope), not a semantic equivalence prover — it flags candidates for a human to judge, it does not assert two methods are provably identical.

## Expected Result
Two-part output:
1. **Removed (SAFE only):** a list of unused `using` directives actually deleted, file by file.
2. **Findings requiring confirmation:** via `ReportFindings`, one entry per LOW/MEDIUM/HIGH/UNKNOWN candidate — category (`unused-method`, `unused-class`, `unused-interface`, `unused-dto`, `unused-validator`, `unused-mapping`, `unused-service`, `unused-repository`, `unused-endpoint`, `unused-registration`, `duplicate-helper`, `dead-implementation`), confidence level, and the search scope used.

If nothing was found at any level, state that plainly — no removed items, empty findings.

## Failure Conditions
- No staged or unstaged changes at all — nothing to scope the search to; stop and report.
- The changed set is so broad (e.g. a mass rename touching dozens of files across services) that "directly related references" can no longer be scoped narrowly — stop and report that the change is too broad for a targeted prune, rather than silently falling back to a wider scan.
- A candidate can't be confidently placed in a confidence bucket even at UNKNOWN's loose bar (contradictory signals) — report it as UNKNOWN with the contradiction stated, don't drop it from the report.

## Success Criteria
- [ ] No file outside the changed project(s) and their directly identifiable consumers was scanned.
- [ ] Every SAFE removal is exclusively an unused `using` directive.
- [ ] No exclusion-listed symbol appears anywhere in the findings.
- [ ] Every non-SAFE finding states its confidence level and the specific scope of the search that produced it.
- [ ] Removed items and reported-for-confirmation items are presented as clearly separate lists.

## Examples

**Correct usage — clean result:**
```
Changed: Order.Application/Features/Orders/Commands/CancelOrder/CancelOrderHandler.cs
         (added a call to a new private helper, removed an old one that's now unused)

→ Removed (SAFE): Order.Application/.../CancelOrderHandler.cs — removed unused
  `using Order.Domain.ValueObjects;` (no longer referenced after the refactor)

→ Findings requiring confirmation:
  - HIGH: private method `ValidateLegacyCancelWindow` in CancelOrderHandler.cs:42 — zero
    references found within Order.Application after this change; not attached to DI/reflection.
    Scope: same file + same project.
```

**Incorrect usage — asking it to remove everything found:**
```
User: /prune and remove all of it
→ "Only SAFE items (unused usings) are auto-removed. Everything else is listed for your
   confirmation — re-run the removal yourself once you've reviewed the HIGH/MEDIUM/LOW/UNKNOWN
   list; /prune won't remove those on your say-so alone in a single command, since each one
   needs individual confirmation, not a blanket approval."
```

**Edge case — marker interface correctly excluded:**
```
Changed: Inventory.Persistence/Warehouses/Repositories/IWarehouseRepository.cs (new, empty,
per the standard marker-interface template)

→ No finding raised for IWarehouseRepository — matches the documented empty-marker-interface
  exclusion. (If the skill had flagged this, that would be a bug in the exclusion check.)
```

**Edge case — Scrutor-registered type looks "unused":**
```
Changed: adds a new IRecurringJob implementation, no explicit `AddScoped<...>()` call anywhere
in the diff.

→ Not flagged as unused-registration — docs/04-coding-rules.md confirms IRecurringJob
  implementations are picked up by convention-based scan, absence of an explicit registration
  call is expected, not a leftover.
```

**Failure — change too broad:**
```
git diff --stat shows 47 files changed across Order, Product, and Inventory services (a
namespace-wide rename).

→ "This change set is too broad to scope a targeted reference search — /prune needs a change
   focused enough that 'directly related references' has a real boundary. Not run."
```

## Testing Strategy
- **Positive:** a change that leaves one now-unused `using` and one now-orphaned private method → verify the using is removed automatically and the method is reported as a separate HIGH-confidence finding, not auto-removed.
- **Positive:** a change with genuinely no dead code → verify an empty result on both removed and findings, not a false positive.
- **Negative:** no changes staged or unstaged → verify it stops without attempting a search.
- **Negative:** an empty marker repository interface in the diff → verify zero findings for it.
- **Boundary:** a public interface with no in-project consumer but plausible cross-service usage → verify MEDIUM, not HIGH, and that the reasoning names the scope limitation.
- **Boundary:** a duplicate helper introduced in the same file as an existing one with near-identical logic → verify it's flagged as `duplicate-helper` at an appropriate confidence, not silently merged or auto-removed.
- **Failure recovery:** a change set spanning many unrelated services → verify it declines to run rather than silently narrowing to an arbitrary subset.

## Future Extension Notes
If a future need arises for a broader (opt-in, explicitly requested) full-repository dead-code sweep, that is a **different, new Skill** — not an expansion of `/prune`'s default scope, which is deliberately bounded to the current change per this task's explicit constraint. If the SAFE bucket ever needs to grow beyond unused-usings (e.g. a genuinely compiler-verifiable case emerges), that's a change to the Confidence Levels table here, and should be argued for explicitly rather than crept into silently — the narrowness of SAFE is a safety property, not an oversight.
