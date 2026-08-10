# Verify Report Output Shape

**Scope:** used only by `/verify`. This is a thin delta on top of `../framework/inspection-output-standard.md`, which owns the actual report structure and severity model — this file states only what `/verify` adds on top of that shared shape.

## Delta from the shared standard
- **Build result comes first**, before the Overview/Detected Issues sections — a failed build is an automatic Critical finding and is reported before anything else, per `../skills/verify/SKILL.md`'s Inspection Workflow step 3.
- **Findings** use `inspection-output-standard.md`'s exact shape (Severity / Rule Reference / Explanation / Recommendation / Expected Benefit / Priority), reported via `ReportFindings`, most-severe first.
- **`solution`-scope reports** are segmented by service heading — each service gets its own Detected Issues block, not one merged list.
- **Conclusion** always ends with an explicit deploy verdict: "ready", "ready with caveats", or "not ready — N blocking issue(s)". Never a vague "looks mostly fine."

Everything else — the Overview/Summary/Detected Issues/Conclusion skeleton and the Critical/High/Medium/Low/Suggestion severity definitions — comes from `../framework/inspection-output-standard.md` unchanged.
