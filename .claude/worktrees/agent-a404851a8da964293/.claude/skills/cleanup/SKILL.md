---
name: cleanup
description: Analyze NovaCore's current workspace and organize pending changes into logical commit groups — analysis only, never commits or modifies files
---

## Purpose
Give a clear, groupable picture of everything currently changed (staged + unstaged) before starting new work or committing — so a mixed working tree doesn't turn into a mixed commit.

## Responsibilities
- Read the full current change set (`git status`, `git diff`, `git diff --staged`).
- Classify every changed file with `../../framework/change-classification.md`.
- Group files into recommended commits, recommend an order, and warn about mixed-concern files.

Explicitly **not** this skill's responsibility: staging, committing, or editing anything (`/commit` does the committing, once files are already grouped and staged by the human), judging code quality (`/review`), finding dead code (`/prune`).

## Input
`/cleanup` — no arguments. Always operates on the entire current working tree state, not a subset.

## Reading Contract
- **Required:** `git status`, `git diff` (unstaged), `git diff --staged` (staged), `../../framework/change-classification.md`
- **Optional:** none
- **Forbidden:** `docs/**` — this is pure git-state analysis, not an architecture task; no source file should be opened beyond what the diff itself already shows

## Execution Rules
1. Run `git status --short` to enumerate every changed, staged, and untracked file.
2. For each file, read its diff (staged or unstaged, whichever applies — a file can appear in both, treat its staged and unstaged hunks separately if they differ in nature) and classify it against every category in `change-classification.md`, including the new **Generated** category.
3. Group files by category into candidate commit sets.
4. For any file whose diff spans more than one category, do not force it into either bucket — list it under Warnings instead, with both categories named.
5. For **Generated** files, apply classification rule 5: pair each one with the real change that produced it rather than giving it an orphan group.
6. Propose a commit order: Migration and Infrastructure groups generally precede the Feature/Bug Fix groups that depend on them; Documentation and Formatting groups are order-independent and can go last.
7. Present the result per `../../templates/cleanup-grouping-template.md`'s shape.

## Rules
- One file, one primary group — never split a single file's diff across two proposed commits (that requires `git add -p`, which this skill recommends but does not perform).
- Untracked files are classified the same as modified ones — "new file, uncategorized" is not a valid output; use the same category table.
- A file with zero identifiable category signal (e.g. a binary asset with no diff content) is reported as **UNKNOWN**, not silently dropped from the report.

## Boundaries
- Never runs `git add`, `git commit`, `git stash`, or any command that changes repository state.
- Never edits file contents.
- Never invokes `/commit` itself, even after producing a clean grouping — it hands the recommendation back to the human, who runs `/commit` per group manually. No hidden pipeline, no autonomous chaining.
- Never expands scope to reviewing code quality inside the diffs it reads — that's `/review`'s job; `/cleanup` only classifies and groups.

## Limitations
- Classification is signal-based (file path, diff shape) and can misjudge a file whose real intent isn't visible from the diff alone (e.g. a one-line change that looks like Formatting but is actually a subtle Bug Fix) — when uncertain, the skill should say so rather than picking confidently.
- Grouping is per-file, not per-hunk — a file that's 90% one category and 10% another is still reported whole under Warnings; this skill doesn't attempt automatic hunk splitting.
- Commit-order recommendations are heuristic (Migration/Infra before Feature) — a codebase-specific dependency the skill can't see (e.g. this migration is actually independent) means the human should still sanity-check the proposed order.

## Expected Result
A grouping report per `../../templates/cleanup-grouping-template.md`: numbered candidate commit groups (each a suggested type/scope/subject + file list), a Warnings section for mixed-category files, and a suggested commit order with reasoning. No git commands executed.

## Failure Conditions
- Working tree is completely clean (`git status --short` empty) — report that and stop, nothing to analyze.
- A file's diff is unreadable (binary, encoding issue) — report it as UNKNOWN rather than guessing its category.

## Success Criteria
- [ ] Every file from `git status --short` appears in exactly one group or in Warnings — none silently omitted.
- [ ] No Migration or Generated file was folded into a Feature group.
- [ ] Every Warnings entry names both categories it spans.
- [ ] Suggested order states a reason per group, not just a bare sequence.
- [ ] No git state was mutated during analysis.

## Examples

**Correct usage — clean split:**
```
git status --short:
 M Order.Domain/Entities/Order.cs
 M Order.Application/Features/Orders/Commands/CancelOrder/CancelOrderHandler.cs
?? Order.Persistence/Migrations/20260803_AddRestockedFlag.cs
?? Order.Persistence/Migrations/20260803_AddRestockedFlag.Designer.cs

## Suggested commit groups
### 1. fix(order): restock inventory on cancel
- Order.Domain/Entities/Order.cs
- Order.Application/Features/Orders/Commands/CancelOrder/CancelOrderHandler.cs

### 2. chore(order): add restocked-flag migration
- Order.Persistence/Migrations/20260803_AddRestockedFlag.cs

### 3. Generated (pairs with group 2)
- Order.Persistence/Migrations/20260803_AddRestockedFlag.Designer.cs

## Suggested order
1. Group 2 + 3 (migration lands before the fix that depends on the new column)
2. Group 1
```

**Incorrect usage — asking it to also commit:**
```
User: /cleanup and then commit the feature group
→ "/cleanup only analyzes and groups — it never commits. Here's the grouping; run /commit
   yourself once you've staged a group."
```

**Edge case — nothing changed:**
```
git status --short: (empty)
→ "Working tree is clean — nothing to group."
```

**Edge case — mixed file:**
```
Product.API/Endpoints/GetProduct.cs diff shows both a new query parameter (Feature) and an
unrelated reformatted block 40 lines away (Formatting).

## Mixed files (span >1 category — recommend splitting)
- Product.API/Endpoints/GetProduct.cs — contains both Feature and Formatting; consider
  `git add -p` to split before committing
```

## Testing Strategy
- **Positive:** a working tree with 3 clearly-separated single-category changes → verify 3 clean groups, correct order, no Warnings section (per the template's "omit if none" rule).
- **Positive:** a migration + its `.Designer.cs` alongside an unrelated feature → verify the Generated file is paired with the migration group, not orphaned or merged into the feature.
- **Negative:** clean working tree → verify it reports "nothing to group" and issues no further analysis.
- **Boundary:** a single file whose diff is exactly 50/50 Feature/Formatting → verify it lands in Warnings, not forced into either group.
- **Boundary:** an untracked binary asset with no readable diff → verify it's reported as UNKNOWN, not dropped or misclassified.
- **Failure recovery:** if `git diff` output is unexpectedly truncated/large → the skill should say so rather than silently analyzing a partial view as if it were complete.

## Future Extension Notes
New categories go into `../../framework/change-classification.md`, not into this file — `/cleanup` and `/review` both consume that shared taxonomy, so adding a category there automatically extends both without editing either skill. If per-hunk (rather than per-file) grouping is ever needed, that's a materially bigger change to Execution Rules step 4 and should be scoped as its own follow-up, not bolted on here.
