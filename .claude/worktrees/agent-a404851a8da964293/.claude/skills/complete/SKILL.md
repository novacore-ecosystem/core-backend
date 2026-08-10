---
name: complete
description: Complete partially written NovaCore code — a file or a selection — using the matching project pattern and template, inferring only what's safe from convention
---

## Purpose
Finish code that's already started, matching the shape a project senior engineer would have finished it with — without inventing business behavior the surrounding code doesn't already imply.

## Supported Commands
`/complete selection` — complete only the currently selected code.
`/complete file` — complete the current file.

## Reading Contract
- **Required:** `../../framework/pattern-library.md` (entry matching the detected construct), `../../framework/template-library.md`, `../../framework/shared-rules.md` (Selection Mode contract, §7)
- **Optional (conditional on detected construct):**
  - Mapping code → `docs/04-coding-rules.md` (Mapping section)
  - Domain entity/Aggregate/Value Object → `docs/conventions/domain-coding-conventions.md`
  - Handler/Consumer → `docs/conventions/application-coding-conventions.md`
  - Repository/Read/Write service → `docs/conventions/persistence-coding-conventions.md`
- **Forbidden:** any file outside the target file/selection and its cited template/reference file; other services

## Responsibilities
- Detect what construct the target file/selection is (or is becoming).
- Load its Pattern Library entry (philosophy) and Template Library entry (shape).
- Fill in exactly what's missing — nothing already complete, nothing outside scope.

Explicitly **not** this skill's responsibility: writing net-new features from a bare command (`/implement`), refactoring already-complete code (`/align`), fixing outdated patterns in otherwise-complete code (`/sync`).

## Execution Workflow
1. **Selection mode:** if invoked as `/complete selection`, the selection boundary is absolute per `shared-rules.md` §7 — read it before anything else. Never modify or refactor anything outside the selection, even adjacent lines, even if they look wrong.
2. **File mode:** detect the file's construct from its path, namespace, base type, or implemented interface.
3. Load the matching `pattern-library.md` entry, then its `template-library.md` entry (or the cited real reference file, if the template is a documented gap).
4. If the cited reference file exists for this service, open it and mirror its actual current shape over the template's prose.
5. Fill in only what's missing. Infer:
   - Standard plumbing (DI-friendly constructor shape, standard exception usage, standard validation) — always safe to infer from convention.
   - Business behavior — only if the surrounding already-written code makes it unambiguous (e.g. a half-written handler whose first three lines already establish the business rule being applied to the fourth). If it's genuinely undecided, leave a specific `// TODO: <question>`, don't invent it.
6. If no template/pattern covers the detected construct, stop and report the gap rather than inventing a structure.
7. Self-review per the checklist below, touching nothing beyond what step 5 changed.
8. Return the completed code as a diff.

## Self-Review
Before returning: naming matches the cited reference file; nothing outside the file/selection was touched; the completed shape matches the loaded template, not a novel structure; no leftover `NotImplementedException` or placeholder in what was supposed to be finished; every inferred piece of business behavior is traceable to something already in the surrounding code, not invented.

## Rules
- File-type → construct detection is structural (path, base type, interface), not name-guessing — if structure alone can't determine it, that's a Failure Condition, not a best-guess.
- "Safely inferred" means: derivable from project convention (naming, standard exception types, standard validation shape) or unambiguous from the code already written in the same file/selection. Anything else is a TODO.

## Boundaries
- Selection mode never touches a single character outside the selection.
- File mode never redesigns the file's overall shape, only fills gaps within the existing one.
- Never invokes `/implement`, `/align`, `/sync`, `/review`, `/verify`, `/cleanup`, `/commit`, or `/prune`. No hidden pipeline, no autonomous chaining — the developer decides what runs next.
- Never touches code that's already complete, even if it looks improvable — that's `/align`'s job, not this one's.

## Limitations
- Selection mode can hit a selection that's syntactically impossible to complete without touching surrounding code (e.g. a dangling unmatched brace) — this is a Failure Condition, not a license to expand the edit.
- Detection accuracy depends on the file already being structurally recognizable as one of the known constructs; a file that's too sparse to classify (e.g. an empty file with just a namespace) can't be reliably completed without knowing the intended construct — ask rather than assume.

## Expected Result
Diff-only — the completed code, showing only what was added or changed.

## Failure Conditions
- Selection is incomplete in a way that can't be finished without touching surrounding code.
- File type/construct can't be determined from structure.
- No template or pattern exists for the detected construct.
- Finishing correctly requires a business decision that truly can't be inferred and the surrounding code gives no hint — leave the TODO and say so; this is a partial-success outcome, not a hard stop, but must be reported clearly as incomplete.

## Success Criteria
- [ ] Nothing outside the file (or selection) was touched.
- [ ] Completed shape matches the cited template/reference file.
- [ ] Every inferred piece of business logic is traceable to existing surrounding code.
- [ ] Every non-inferable gap is a specific, answerable TODO — never a vague placeholder.
- [ ] No other Skill was invoked.

## Examples

**Correct usage — file mode, safe inference:**
```
File: Order.Persistence/Orders/Write/OrderWriteService.cs — CreateAsync is fully written,
UpdateShippingAddressAsync has only a signature and NotImplementedException.

/complete file

→ Detects: Write persistence service (Persistence Service pattern).
→ Mirrors CreateAsync's shape (repo.UpdateAsync intent-named call) for UpdateShippingAddressAsync.
→ No business decision needed — the method name and existing sibling methods fully determine
  the shape.
```

**Correct usage — selection mode:**
```
Selected: a single incomplete validation rule inside RegisterValidator.cs
  RuleFor(x => x.Email) // TODO

/complete selection

→ Completes only that RuleFor chain (NotEmpty + EmailAddress, matching the pattern used
  elsewhere in the same file) — does not touch any other rule in the file even though one
  nearby rule uses an inconsistent message format.
```

**Incorrect usage — asking it to also clean up nearby code:**
```
User: /complete file, and also fix the naming in the method above it
→ "/complete only finishes what's incomplete — the method above is already complete, even if
   its naming could improve. That's /align's job; run it separately."
```

**Edge case — undecidable business rule:**
```
File: half-written CreateWarehouseHandler.cs — validation stops right before a check that
would reject warehouses in unsupported regions, with no region list defined anywhere else
in the codebase.

→ Completes everything else; leaves:
  // TODO: which regions are supported? No existing feature defines this list — needed before
  // this validation can be completed.
→ Reports the file as completed-except-this-one-TODO, not "done."
```

**Edge case — undetectable construct:**
```
File: a new, nearly-empty .cs file with just `namespace Order.Application.Features.Orders;`
and no class, base type, or interface yet.

/complete file
→ "Can't determine what construct this is meant to be from structure alone — is this a
   Command, Query, Handler, or something else?" (asks, does not guess)
```

## Future Extension Notes
New completable constructs are added the same way `/implement` gains new targets: add the construct to `pattern-library.md` (and `template-library.md` if it has a literal shape), and this Skill picks it up automatically via its Reading Contract — no change needed to this file's own logic.
