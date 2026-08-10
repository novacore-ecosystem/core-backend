# Inspection Output Standard

**Scope:** the shared report structure and severity model for the Production Inspection Framework (`/verify`, `/inspect`, and — for its per-weakness detail only — `/review`). Defines the shape once so no inspection Skill re-derives its own; a Skill's SKILL.md states only its delta from this standard, never a redefinition of it.

## Report shape

Every finding-based inspection result (`/verify`, `/inspect`) is structured:

```
## Overview
<what was inspected, scope, target>

## Summary
<one paragraph: overall state, headline risk if any>

## Detected Issues
### [<Severity>] <one-line title>
- **Rule Reference:** <the exact doc + section this violates — pattern-library.md, rules-library.md,
  02-architecture-rules.md, conventions/*, or an ADR in docs/decisions/>
- **Explanation:** what is wrong, and why it's wrong in this project's specific context — never a
  generic best-practice statement disconnected from a cited rule
- **Recommendation:** the specific correction, referencing the established pattern/template to use
- **Expected Benefit:** what improves if this is fixed (concrete: "prevents X", not "better code")
- **Priority:** how urgently this should be addressed relative to other findings in this report

(repeat per finding, most severe first)

## Conclusion
<is this production-ready as-is, and the shortest path to yes if not>
```

If there are zero findings, the Detected Issues section is omitted entirely — never a placeholder "no issues found" filler; state it in the Summary/Conclusion instead.

## Severity model

| Severity | Meaning | Example |
|---|---|---|
| **Critical** | Would cause a production incident, data loss, security exposure, or a hard architecture violation that breaks the system's guarantees (e.g. bypassing the Outbox for an integration event, breaking transaction atomicity) | Direct `IEventPublisher.PublishAsync` call from feature code, bypassing Outbox |
| **High** | A real architecture/pattern violation with meaningful risk, but not immediately catastrophic — will cause a production issue under a specific, plausible condition | Handler calling `DbContext` directly instead of a Read/Write service |
| **Medium** | A pattern/rule inconsistency that increases maintenance cost or technical debt but doesn't threaten correctness today | Hand-mapping missing a field that happens to always be null today but won't stay that way |
| **Low** | Readability, naming, or minor consistency issue with no functional or architectural risk | Inconsistent method ordering within a file |
| **Suggestion** | An optional improvement, not a violation of anything documented — the code is compliant as-is | A more idiomatic LINQ expression for the same result |

Severity is assigned by matching the finding against an actual documented rule/pattern's stated importance — never by gut feel. A finding with no citable Rule Reference cannot be Critical or High; at most it's a Suggestion (see `../../framework/reading-contracts.md` resolution rule 5 — don't guess at severity any more than at a rule itself).

## Which Skills use this
- **`/verify`** — every finding follows this shape exactly, across all severities.
- **`/inspect`** — uses this shape for its "Detected Issues" section, scoped to the one aspect requested (not every category).
- **`/review`** — does **not** use this shape. Review evaluates engineering quality, not rule compliance, and uses its own Strengths/Weaknesses/Improvement Opportunities/Score/Summary structure (see the `review` Skill and `engineering-scoring.md`). A Weakness in a Review report may reference a severity-like priority for triage purposes, but never claims a Rule Reference unless one genuinely exists — most quality judgments are informed opinion, not rule violations.
