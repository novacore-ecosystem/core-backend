# Engineering Scoring Model

**Scope:** the shared scoring model for the Production Inspection Framework. Primarily consumed by `/review` (its headline output); `/verify` and `/inspect` may cite it for an aggregate summary number, but their primary output is the finding list per `inspection-output-standard.md`, not a score.

## Dimensions

Each dimension is scored 0–10 by the inspecting Skill, grounded in what it actually observed (never a felt sense of quality untethered from a specific file/line):

| Dimension | What it measures |
|---|---|
| **Architecture** | Dependency direction and layer placement vs. `docs/02-architecture-rules.md` |
| **Pattern Compliance** | Match against the relevant `pattern-library.md` entry/entries |
| **Rule Compliance** | Match against the relevant `rules-library.md` entries |
| **Maintainability** | How easily this can be safely changed later — coupling, responsibility clarity |
| **Readability** | Clarity at a glance — naming, method length, control-flow complexity |
| **Consistency** | Match against sibling implementations of the same construct elsewhere in the codebase |
| **Naming** | Match against `docs/04-coding-rules.md`'s naming conventions |
| **Layer Separation** | Whether each piece of logic lives in the layer responsible for it |
| **Production Readiness** | Exception handling, transaction correctness, validation completeness |
| **Technical Debt** | Shortcuts, TODOs, duplicated logic, or deferred decisions left behind |

## Method

1. Score each dimension 0–10 independently — a low score in one dimension does not automatically lower another.
2. Every score below 8 must cite the specific observation that justifies it (a file/line, or a named absence) — no unexplained deductions.
3. Overall score = the unweighted mean of the 10 dimensions, expressed as a 0–100 number (mean × 10). Dimensions genuinely not applicable to the inspected target (e.g. "Layer Separation" for a single Value Object with no cross-layer surface) are excluded from the mean, not scored 10 by default.

## Score bands

| Score | Meaning |
|---|---|
| 90–100 | Matches the project's best existing examples; ship as-is |
| 70–89 | Solid, production-usable; the noted weaknesses are worth fixing but not blocking |
| 50–69 | Functional but carries real maintainability/consistency risk; should be improved before this becomes a pattern others copy |
| Below 50 | Not representative of this project's standards; treat as a rewrite candidate, not a polish candidate |

A score is a triage aid, not a verdict — always paired with the specific dimension breakdown and the Weaknesses list that explains *why*, never reported as a bare number.

## Relationship to the Severity model
`inspection-output-standard.md`'s Critical/High/Medium/Low/Suggestion severities classify individual rule *violations*. This scoring model evaluates overall engineering *quality*, which is a broader, more holistic judgment — a construct can score reasonably (e.g. 75) while still having one High-severity finding elsewhere; the two are complementary views (`/verify` finds violations, `/review` scores quality), not the same measurement expressed two ways.
