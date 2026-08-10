# Reading Contracts

**Scope:** defines the Reading Contract mechanism itself — the concept every entry in `pattern-library.md` and every future `SKILL.md` (per `command-contract.md`) must declare. This file states the rule once; it is referenced, never restated.

## The mechanism

Every construct that involves loading context — a pattern in the Pattern Library, or a future Skill acting on a specific task — must declare three lists before doing anything else:

- **Required** — documents that are always loaded for this construct, no matter the invocation.
- **Optional** — documents loaded only when a stated condition is actually true for this specific invocation (e.g. "only if the target touches Outbox/Inbox"). An Optional doc whose condition doesn't hold is not loaded — it is not a soft suggestion to read anyway.
- **Forbidden** — documents/areas that must never be loaded for this construct, even if they seem related or "couldn't hurt." This is the actively enforced half of the contract, not just the unlisted default.

## Resolution rules

1. Load Required first, always.
2. Evaluate each Optional condition against the actual invocation; load only the ones that are true.
3. Never load anything on the Forbidden list. If completing the task seems to require crossing a Forbidden boundary, that is not a license to cross it — stop and report the conflict instead (see rule 5).
4. Never explore beyond Required + satisfied-Optional "just to be sure." Per `shared-rules.md` §1, an apparent need for more context is a **doc gap**, not permission to expand scope silently.
5. If two Required/Optional documents state conflicting facts, or the only way to finish requires touching a Forbidden area, stop and ask rather than guessing — this is the same principle stated in `shared-rules.md` §1 and §3, applied specifically to the reading step.

## Where contracts are declared
- **Patterns** — each entry in `pattern-library.md` states its own Required/Optional/Forbidden line.
- **Skills** — `command-contract.md` requires every future `SKILL.md` to include a "Reading Contract" section in this same shape.
- **Layers** — the underlying Required/Forbidden facts for layer boundaries live in `.claude/framework/boundaries.md` and `docs/05-context-loading-map.md`; pattern and Skill contracts cross-reference those rather than re-deriving the same facts independently.
