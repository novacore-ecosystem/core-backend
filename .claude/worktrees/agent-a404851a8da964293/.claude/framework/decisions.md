# Decision Records

**Scope:** pointer into the project's ADR system — does not restate any decision's content.

Architecture Decision Records live at `docs/decisions/` (convention documented in `docs/decisions/README.md`). They capture *why* an architectural choice was made — the problem, the decision, and its known tradeoffs — as opposed to `docs/02-architecture-rules.md` (the binding rule itself) or `docs/conventions/*.md` (the resulting day-to-day shape).

**Enforcement:** before introducing an architectural change — a new dependency direction, a new cross-cutting building block, a deviation from an existing pattern in `pattern-library.md` — check `docs/decisions/` for an ADR that already covers this ground. If one exists and the new work would contradict it, that's a stop condition (per `shared-rules.md` §3): surface the conflict rather than silently overriding a documented decision.

When a genuinely new architectural decision is made, write a new ADR per `docs/decisions/README.md`'s convention — in `docs/decisions/`, not here.
