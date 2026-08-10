---
name: commit
description: Safely create production-quality git commits for NovaCore from staged changes only, with validated Conventional Commit messages
---

## Purpose
Create a git commit whose message accurately, consistently, and minimally describes the staged changes — nothing more. This is a daily-use, single-responsibility tool: it commits, it does not stage, clean up, review, or verify.

## Responsibilities
- Read the staged diff and only the staged diff.
- Validate (or generate) a Conventional Commit message against `../../framework/checklists/commit-message-checklist.md`.
- Create the commit.

Explicitly **not** this skill's responsibility: deciding what should be staged (`/cleanup`), judging code quality (`/review`), confirming the build (`/verify`), removing dead code (`/prune`). If any of those would help, this skill *suggests* running them — it never runs them itself (see Boundaries).

## Input
`/commit` or `/commit <message>`.
- No message: derive one from the staged diff.
- Message given: validate it; use unchanged if it passes, otherwise propose exactly one corrected version.
There is no other argument shape — this skill takes no flags, no target selection. If nothing is staged, that's a Failure Condition, not an input to interpret differently.

## Reading Contract
- **Required:** `git diff --staged` (the *only* diff this skill ever reads), `git log --oneline -30` (style calibration), `../../framework/checklists/commit-message-checklist.md`, `../../framework/change-classification.md` (category → Conventional Commit type mapping, see Rules), `../../framework/shared-rules.md`
- **Optional:** none — this skill's scope is narrow enough that no conditional reading applies
- **Forbidden:** `git diff` (unstaged), any file not part of the staged diff, `docs/**` (commit-message shape is entirely owned by this skill's own checklist, not the architecture docs)

## Execution Rules
1. Run `git diff --staged --stat`. If empty, stop — Failure Condition, nothing to commit.
2. Read the full staged diff (content, not just stat).
3. Read `git log --oneline -30` to confirm current type/scope usage is still consistent with the checklist.
4. Classify the staged diff's dominant nature using `../../framework/change-classification.md`'s categories, then map it to a Conventional Commit **type** via the table in Rules below. If the staged diff is genuinely mixed across categories (e.g. a feature file plus an unrelated formatting-only file), don't force one type — flag it as a Failure Condition and point at `/cleanup` instead of guessing which category should win.
5. Derive the **scope** from the service/component(s) actually touched (see Rules — Scopes).
6. If the user supplied a message: run it through every box in the checklist. If it passes all of them, use it **unchanged** — do not rewrite a message that already passes, even if another phrasing reads better.
7. If it fails one or more boxes, or no message was supplied: construct exactly **one** message (`type(scope): description`) and state which checklist box(es) it satisfies that the alternative didn't.
8. Commit: `git commit -m "<message>"`. Never `--no-verify`, never a co-author trailer, never any AI-attribution text of any kind.
9. Run `git status` to confirm the commit succeeded and report it.

## Rules

### Conventional Commit types supported
`feat`, `fix`, `refactor`, `revert`, `docs`, `build`, `ci`, `chore`, plus `perf`/`test`/`style` if a staged diff is genuinely and only that (rare in practice here, per the precedent noted in `commit-message-checklist.md`).

### Classification → type mapping
| `change-classification.md` category | Conventional Commit type |
|---|---|
| Feature | `feat` |
| Bug Fix | `fix` |
| Refactor | `refactor` |
| Rename | `refactor` (unless the rename is purely mechanical with zero logic touched anywhere in the diff, still `refactor` — Conventional Commits has no separate "rename" type) |
| Formatting | `style` |
| Infrastructure | `build` (Docker/hosting/startup wiring) or `ci` (pipeline config) — pick based on what actually changed |
| Configuration | `chore` |
| Migration | `feat` if it ships alongside the feature that needs it and this is the only commit for that feature, otherwise `chore` if the migration stands alone |
| Documentation | `docs` |
| Generated | never its own commit type — generated file diffs ride along with the type of the real change that produced them (see `change-classification.md` rule 5); if a diff is *purely* regenerated output with no accompanying real change, that's itself a Failure Condition, not a normal commit |
| A user's diff that reverts a previous commit | `revert` — recognizable as a diff that is the exact inverse of a recent commit in `git log` |

### Scopes
Scope = the service or component the change is centered on, matching names already in active use: `auth`, `user`, `product`, `inventory`, `order`, `audit`, `notification`, `gateway`, or `building block` (cross-cutting `BuildingBlock.*` changes, per existing precedent in `git log`). If the diff is genuinely repo-wide (e.g. a `.editorconfig` change), omit the scope rather than inventing one — `type: description` is valid Conventional Commits.

## Boundaries
- Never stages, unstages, or discards anything — only commits what's already staged.
- Never inspects unstaged changes, even to sanity-check whether the staged set looks incomplete.
- Never amends, force-pushes, or rewrites history.
- Never invokes `/cleanup`, `/review`, `/verify`, or `/prune` itself — if the staged diff looks mixed, under-reviewed, unbuilt, or full of dead code, it says so in its output and names the relevant command; it does not run it. No hidden pipeline, no autonomous chaining.
- Never adds a co-author trailer or any AI-attribution text.
- Never offers more than one candidate message.

## Limitations
- Type/scope inference is diff-content-based, not perfect — a diff that's structurally a refactor but happens to also fix a latent bug in passing may be misclassified; the checklist's "type matches what the diff actually does" box exists specifically to catch this, but ultimately relies on the diff being legible.
- The scope list above is a living set drawn from current precedent, not an enforced enum — a genuinely new service/component scope is valid the first time it's used; the skill should recognize it from the touched file paths rather than rejecting it for not being pre-listed.
- Revert detection is heuristic (diff inversion against recent history), not guaranteed — if uncertain, ask rather than mislabel as `revert`.

## Expected Result
Either: a completed commit + `git status` confirmation. Or, if the message needed correcting: the one proposed message, which checklist box(es) it fixes, then the commit + confirmation.

## Failure Conditions
- Nothing staged.
- Staged diff is mixed across more than one classification category with no clear dominant one — report this and point at `/cleanup`, don't force a single type/message onto it.
- Staged content looks like a secret (`.env`, credentials, private keys) — warn and stop, don't commit until the user explicitly confirms.
- A file that looks purely Generated (per `change-classification.md`) is staged with no accompanying real change in the same staged set.

## Success Criteria
- [ ] Only staged content was read or acted on.
- [ ] The final message passes every box in `commit-message-checklist.md`.
- [ ] Type was chosen via the classification → type mapping table, not guessed ad hoc.
- [ ] Scope matches actual touched service(s) or is correctly omitted.
- [ ] No co-author trailer, no AI attribution, no `--no-verify`.
- [ ] Exactly one message was proposed, never a menu of alternatives.
- [ ] `git status` after the commit confirms a clean, expected result.

## Examples

**Correct usage — message already good:**
```
Staged: Order.Domain/Entities/Order.cs, Order.Application/Features/Orders/Commands/CancelOrder/*
User: /commit fix(order): restock inventory on cancel

Staged diff shows RestockAsync is now called from CancelOrderHandler where it was previously
missing → classification: Bug Fix → type: fix. Scope "order" matches the touched service.
Message passes every checklist box unchanged.

→ committed as: fix(order): restock inventory on cancel
```

**Incorrect usage — message needs correction:**
```
Staged: Inventory.API/Endpoints/CreateWarehouse.cs (new file), Inventory.Application/Features/... (new)
User: /commit fixed warehouse stuff

Fails: not Conventional Commit format, not imperative, vague clarity, wrong type (diff adds a
new endpoint + command — classification: Feature → type: feat, not "fixed").

→ proposed: feat(inventory): add create-warehouse endpoint
   (fixes: format, type, clarity, action — grammar box only technically also failed on "fixed")
→ committed as: feat(inventory): add create-warehouse endpoint
```

**Edge case — nothing staged:**
```
User: /commit
git diff --staged --stat is empty.
→ "Nothing staged — nothing to commit. Stage changes first, or run /cleanup if you're not sure
   how to group what's currently modified."
```

**Edge case — mixed staged set:**
```
Staged: Order.Application/.../CreateOrder/CreateOrderHandler.cs (new validation logic)
        + Product.API/Endpoints/GetProduct.cs (unrelated whitespace reformat)

→ "Staged changes span two unrelated categories (Bug Fix in order, Formatting in product) —
   not committing as one. Suggest splitting via `git reset` + two commits, or run /cleanup for
   a grouping recommendation." (Does not invoke /cleanup itself.)
```

## Testing Strategy
- **Positive:** stage a single-category change with an already-correct message → verify it commits unchanged with no rewrite proposed.
- **Positive:** stage a single-category change with a poor message (wrong type, vague, non-imperative) → verify exactly one corrected message is proposed and used, and that it independently satisfies every checklist box.
- **Negative:** run with nothing staged → verify it stops cleanly with no commit attempted and no fabricated diff analysis.
- **Negative:** stage a mixed-category diff → verify it refuses to force a single type and correctly names `/cleanup` without invoking it.
- **Boundary:** stage a diff that is a byte-for-byte revert of the previous commit → verify `revert` type is correctly proposed, not `fix` or `chore`.
- **Boundary:** stage a `.env` file alongside legitimate code changes → verify it halts and warns rather than committing silently.
- **Failure recovery:** if `git commit` itself fails (e.g. a pre-commit hook rejects it) → report the hook's actual output verbatim; never retry with `--no-verify`.

## Future Extension Notes
If new scopes become common (a new service ships), no change to this file is needed — the Scopes rule already derives scope from touched paths rather than a hardcoded enum; only the illustrative list above may want a refresh for readability. If Conventional Commit type precedent expands (e.g. `perf` becomes common), add it to "Conventional Commit types supported" and extend the mapping table — this file, not `commit-message-checklist.md`, owns the classification→type mapping, so the checklist itself doesn't need touching for that kind of change.
