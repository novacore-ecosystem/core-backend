---
name: review
description: Engineering-quality code review of NovaCore's current diff — scored, holistic, strengths/weaknesses — distinct from /verify's rule-compliance checking
---

## Purpose
Judge engineering quality, not rule compliance: is this diff readable, maintainable, well-organized, consistent with sibling code, and appropriately simple — the way a senior engineer would read it, not the way a linter would. `/verify` answers "does this violate anything documented"; `/review` answers "is this good work," which is a broader, more holistic judgment that can be true even when every rule technically passes, and can find real problems no rule captures.

Note: this project-scoped skill intentionally shadows the global `review` skill within this repo — the global one summarizes a diff to save tokens; this one performs the full engineering-quality evaluation below.

## Supported Commands
`/review` — reviews `git diff --staged`, falling back to `git diff` (unstaged) if nothing is staged. No target argument — review is always diff-scoped; a specific-construct compliance check is `/verify`'s job, not this one's.

## Reading Contract
- **Required:** `docs/02-architecture-rules.md`, `docs/04-coding-rules.md`, `../../framework/engineering-scoring.md`, `../../framework/shared-rules.md`
- **Optional (conditional on what the diff touches):**
  - Domain files → `docs/conventions/domain-coding-conventions.md`
  - Application/handler files → `docs/conventions/application-coding-conventions.md`
  - Persistence files → `docs/conventions/persistence-coding-conventions.md`
  - Any construct with a `../../framework/pattern-library.md` entry → that entry, for the "what would this look like done well" baseline
- **Forbidden:** files outside the diff; `docs/_archive/**`; other services not touched by the diff

## Responsibilities
- Evaluate the diff against the ten dimensions in `engineering-scoring.md`.
- Identify concrete strengths (not just weaknesses — a review that never says what's good isn't calibrated).
- Produce an overall score with per-dimension breakdown.

Explicitly **not** this skill's responsibility: itemized rule-violation findings with severity/Rule-Reference citations (`/verify`), fixing anything (`/align`), a single-aspect deep dive (`/inspect`).

## Inspection Workflow
1. Get the diff (staged, or unstaged if nothing staged). If neither exists, stop.
2. Load Required docs plus the Optional docs whose condition is true for this diff.
3. For each changed construct, load its `pattern-library.md` entry if one exists — this is the "what senior-engineer-quality looks like here" reference point, not a compliance gate.
4. Score each of `engineering-scoring.md`'s ten dimensions 0–10, citing the specific observation behind any score below 8 — exclude dimensions genuinely not applicable to this diff (see the scoring model's method).
5. Compile:
   - **Strengths** — specific things this diff does well, each citing a file/line, not generic praise.
   - **Weaknesses** — specific things that hold the score down, each citing a file/line and what a stronger version would look like.
   - **Improvement Opportunities** — optional, non-blocking suggestions distinct from Weaknesses (a Weakness holds the score down; an Improvement Opportunity is a "could be even better," closer to a Suggestion in spirit but framed as an opportunity, not a defect).
6. Compute the overall score (mean × 10) per `engineering-scoring.md`.
7. Write the Summary — one paragraph, the headline judgment a senior reviewer would open with.
8. Return the full report per `../../templates/review-report-template.md`.

## Rules
- Every Strength and Weakness cites a specific file/line — a review with generic, untethered praise or criticism has failed its own purpose.
- Cross-check the diff's actual content against `../../framework/change-classification.md` — if it claims to be one kind of change but reads as another, that observation belongs in Weaknesses.
- A diff can score well overall while still having a Weakness worth fixing — the score is not a pass/fail gate the way `/verify`'s findings are.

## Boundaries
- Never edits code — findings only.
- Never reviews the whole repository, only the current diff.
- Never produces itemized Severity/Rule-Reference findings in `/verify`'s shape — that's a different Skill with a different purpose; conflating the two formats would blur the compliance/quality distinction this framework depends on.
- Never invokes `/verify`, `/inspect`, `/align`, `/sync`, `/implement`, `/complete`, `/cleanup`, `/commit`, or `/prune`. No hidden pipeline, no autonomous chaining.

## Limitations
- Quality judgment is inherently more subjective than `/verify`'s rule-citation model — two runs of `/review` against the same diff should land close but won't be bit-identical the way a rule-violation check would be; the ten-dimension structure exists specifically to bound that subjectivity, not eliminate it.
- A diff touching a pattern-library gap (no real example to compare against) makes the Consistency and Pattern Compliance dimensions harder to score confidently — say so rather than scoring with false precision.
- Review quality depends on the diff being complete enough to judge in context — a tiny, isolated diff (e.g. one line) may not have enough surface for a meaningful ten-dimension score; say so rather than padding a score.

## Scoring Model
See `../../framework/engineering-scoring.md` — this skill uses it as its primary output mechanism.

## Expected Result
A report per `../../templates/review-report-template.md`: Overview, Strengths (cited), Weaknesses (cited), Improvement Opportunities, per-dimension score table, overall score + band, Summary.

## Failure Conditions
- No diff at all (nothing staged, nothing unstaged).
- Diff is too small/isolated to meaningfully score across ten dimensions — report which dimensions were skippable and why, don't force a number.

## Success Criteria
- [ ] Every Strength and Weakness cites a specific file/line.
- [ ] Every scored dimension below 8 has a stated justification.
- [ ] Overall score correctly computed as the mean of applicable dimensions × 10.
- [ ] No itemized Severity/Rule-Reference findings appear — that shape is reserved for `/verify`.
- [ ] No code was modified, no other Skill was invoked.

## Examples

**Correct usage:**
```
Staged: CreateWarehouseHandler.cs + CreateWarehouseValidator.cs (new feature)

→ Strengths: "Validator correctly scopes required-field checks to only the fields with no
  sensible default (CreateWarehouseValidator.cs:8-14) — matches the project's 'only validate
  what's worth validating' convention."
→ Weaknesses: "CreateWarehouseHandler.cs:22 — the address-normalization logic is inlined in
  the handler; every sibling handler in this feature extracts this kind of thing to a private
  method (see UpdateWarehouseHandler.cs:31 for the established shape)."
→ Scores: Architecture 9, Pattern Compliance 8, Readability 6 (justified above), ... 
→ Overall: 82/100 — "Solid, production-usable; the one extraction issue is worth fixing before
  this shape gets copied elsewhere."
```

**Incorrect usage — asking it to also check compliance:**
```
User: /review, and also tell me if this violates any architecture rules
→ "Rule-violation checking with severity is /verify's job, not /review's — run /verify handler
   for that. This review covers engineering quality only."
```

**Edge case — diff too small:**
```
Staged: a single one-line typo fix in a log message.
→ "Diff too small to meaningfully score across ten dimensions — this is a trivial, clearly
   correct change. No review report generated beyond: looks fine."
```

## Future Extension Notes
If a project-specific eleventh dimension becomes worth tracking, add it to `engineering-scoring.md`, not here — every Skill that scores (currently just `/review`) picks it up automatically via the shared model.
