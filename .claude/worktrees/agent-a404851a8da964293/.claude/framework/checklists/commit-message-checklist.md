# Commit Message Checklist

**Scope:** used only by `/commit`. Distilled from this repo's actual `git log` history (Conventional Commits, consistently applied) — not an invented convention. No other doc in `docs/` owns commit-message style, so this is the one net-new checklist in the framework.

## Format
```
<type>(<scope>): <description>
```
- Lowercase `type`, lowercase `scope`, lowercase first word of `description` (unless a proper noun/identifier).
- Imperative, present tense: "add", "fix", "resolve" — not "added", "adds", "fixes".
- No trailing period.
- `scope` = the service or component the change is centered on (`inventory`, `order`, `building block`, `gateway`, ...) — match the scope names already used in `git log`, don't invent new ones.

## Types observed in this repo
`feat`, `fix`, `refactor`, `chore`, `docs`. (`test`, `perf`, `build`, `ci` are valid Conventional Commit types if a change genuinely needs them, but have no precedent here yet — use only if none of the five above fit.)

## Validation checklist (run against the staged diff, not just the message text)
- [ ] **Type** matches what the diff actually does (a diff that only changes tests is not `feat`; a diff that changes behavior is not `chore`).
- [ ] **Scope** names the actual service/component touched — if the diff spans multiple services, either pick the dominant one or flag it as a candidate for `/cleanup` splitting instead.
- [ ] **Grammar**: imperative mood, no trailing period, lowercase per the format rules above.
- [ ] **Clarity**: a reader with no other context can tell what changed from the subject line alone.
- [ ] **Action**: the subject describes what the commit *does*, not what the code *is* (e.g. "fix broken DI in inventory" not "DI is broken in inventory").
- [ ] **Consistency**: matches the style of recent real commits (`git log --oneline -30`) — don't introduce a new format even if it seems clearer in isolation.

## Decision rule
If a user-provided message already passes every box above, use it unchanged — do not rewrite for taste. If it fails one or more boxes, propose exactly one improved version and use that. Never offer multiple alternatives, never re-litigate a message that already passes.
