# Inspect Report Output Shape

**Scope:** used only by `/inspect`. A thin delta on `../framework/inspection-output-standard.md`, scoped to a single aspect.

```
## Aspect: <the one requested aspect>
## Target: <file/feature inspected>

## Current State
<factual description of what the target actually does today, with respect to this aspect —
no judgment yet>

## Detected Issues
### [<Severity, or "Diagnostic" if this is a rules-library.md gap aspect>] <title>
- **Rule Reference:** <cite the real doc, or state "none — gap aspect, see rules-library.md">
- **Explanation:** ...
- **Recommendation:** ...
- **Expected Benefit:** ...
- **Priority:** ...

(omit entirely if none found)

## Potential Risks
<concrete scenario(s) if the issue(s) above go unaddressed — not generic "could be a problem">

## Recommendations
<specific fix path, referencing the established pattern where one exists>
```

For gap aspects (`performance`, `security` beyond authZ — per `../skills/inspect/SKILL.md`'s Reading Contract table), every Detected Issue is explicitly labeled "Diagnostic" rather than a severity, and capped at Suggestion per `inspection-output-standard.md`'s own rule (no Rule Reference → no Critical/High). Never blur this distinction to make a finding look more authoritative than the underlying documentation supports.
