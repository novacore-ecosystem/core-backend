# Command Contract

**Scope:** the required-fields spec every future `SKILL.md` must satisfy. This is the *file format*; `workflow-contract.md` is the *runtime model* it supports — each field below is what a specific lifecycle stage in that contract executes against.

Skills are not implemented in this task (see `FRAMEWORK.md`) — this is the spec they must be written against when they are.

## Required fields

| Field | Content | Feeds workflow-contract stage |
|---|---|---|
| **Purpose** | One or two sentences: what this command does and why it exists as its own Skill rather than folded into another. | — |
| **Input** | The exact trigger syntax and arguments, including what's required vs. optional, and how ambiguity in the input should be handled. | 1. Trigger Resolution |
| **Reading Contract** | Required / Optional (with conditions) / Forbidden documents, in the shape defined by `reading-contracts.md`. | 2. Reading Contract Resolution |
| **Execution Rules** | The deterministic, numbered algorithm the Skill follows — what it does with what it loaded. References `pattern-library.md`/`template-library.md`/`rules-library.md` entries by name rather than restating their content. | 3–5. Pattern/Template Resolution, Rule Validation, Execution |
| **Boundaries** | What this Skill explicitly never does — its non-goals. Prevents scope creep into territory another Skill owns. | — |
| **Expected Result** | The concrete shape of what a successful run produces (a diff, a report, a commit) — not a vague description. | 7. Output |
| **Failure Conditions** | The specific situations that halt execution and require asking the user instead of proceeding — ambiguous target, missing template, conflicting docs, boundary violation. | 1–2. Trigger/Reading Contract Resolution (where most stop conditions surface) |
| **Success Criteria** | The checklist the Skill runs against its own output before returning it — what "done correctly" means, checkable, not aspirational. | 6. Self-Validation |

## Format skeleton

```markdown
---
name: <skill-name>
description: <one line, used for Skill-tool matching>
---

## Purpose
## Input
## Reading Contract
## Execution Rules
## Boundaries
## Expected Result
## Failure Conditions
## Success Criteria
```

## Reconciliation note

The 8 Skills built in the prior session (`.claude/skills/{commit,cleanup,clean,complete,scaffold,review,verify,sync}/SKILL.md`) predate this contract and use an earlier field set (`Trigger`, `Context Loading`, `Execution Workflow`, `Templates & Docs Used`, `Validation Checklist`, `Output Contract`, `Stop Conditions`) that maps closely but not exactly onto the fields above (`Trigger`→Input, `Context Loading`→Reading Contract, `Execution Workflow`→Execution Rules, `Output Contract`→Expected Result, `Stop Conditions`→Failure Conditions, `Validation Checklist`→Success Criteria). This is informational only — no action is taken on those files by this task. Reconciling them to this contract is deferred to whenever Skills are next revisited.
