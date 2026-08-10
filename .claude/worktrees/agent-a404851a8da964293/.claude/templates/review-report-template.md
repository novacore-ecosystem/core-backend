# Review Report Output Shape

**Scope:** used only by `/review`. Superseded from its original `ReportFindings`-based shape now that `/review` is explicitly a quality-scoring Skill, distinct from `/verify`'s rule-violation checking (`/verify` uses `ReportFindings`; `/review` does not — see `../skills/review/SKILL.md` Boundaries).

```
## Overview
<diff scope: files changed, feature context>

## Strengths
- <specific, file/line-cited, thing this diff does well>
- ...

## Weaknesses
- <specific, file/line-cited, thing holding the score down> — what a stronger version looks like
- ...

## Improvement Opportunities
- <optional, non-blocking "could be even better" — distinct from Weaknesses>

## Scores
| Dimension | Score | Justification (required if < 8) |
|---|---|---|
| Architecture | x/10 | |
| Pattern Compliance | x/10 | |
| Rule Compliance | x/10 | |
| Maintainability | x/10 | |
| Readability | x/10 | |
| Consistency | x/10 | |
| Naming | x/10 | |
| Layer Separation | x/10 | |
| Production Readiness | x/10 | |
| Technical Debt | x/10 | |

**Overall: NN/100 (<band, per ../framework/engineering-scoring.md>)**

## Summary
<one paragraph — the headline judgment>
```

Dimension definitions and score bands come from `../framework/engineering-scoring.md`, unchanged here. Exclude any dimension genuinely not applicable to the diff (per that model's own method) rather than scoring it 10 by default. If the diff is too small to meaningfully score, say so instead of forcing a number (see `../skills/review/SKILL.md` Failure Conditions).
