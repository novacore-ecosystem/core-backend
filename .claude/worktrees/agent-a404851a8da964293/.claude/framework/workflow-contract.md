# Workflow Contract

**Scope:** the generic execution lifecycle every future Skill follows. This is the *runtime model* — what stages happen and in what order. `command-contract.md` is the *file format* — what fields a `SKILL.md` must declare to support this lifecycle. The two are complementary: a Skill's Reading Contract field (command-contract.md) is what stage 2 below executes against.

Skills are not implemented in this task (see `FRAMEWORK.md`) — this contract is the spec they must follow when they are.

## Stages

1. **Trigger Resolution**
   Parse the invocation and its arguments. If the target is ambiguous — more than one plausible match for an entity/service/feature name — stop and ask rather than guessing which one.

2. **Reading Contract Resolution**
   Load every Required document. Evaluate each Optional document's stated condition against this specific invocation; load only the ones that are true. Never load anything on the Forbidden list. Follow `reading-contracts.md`'s resolution rules exactly — this stage is where that mechanism actually executes.

3. **Pattern & Template Resolution**
   For any Skill that writes or changes code: consult `pattern-library.md` for the relevant construct's philosophy before consulting `template-library.md` for its literal shape. If a real reference file is cited, open it — it is ground truth over the template's prose. Skip this stage for read-only/analysis Skills that don't produce code (nothing to template).

4. **Rule Validation**
   Cross-check the intended change against the relevant entries in `rules-library.md`. If a relevant category is a documented gap, that's not a green light to invent a rule — proceed conservatively and note the gap in the output.

5. **Execution**
   Perform the Skill's actual work, per its own Execution Rules (command-contract.md field). This is the only stage where side effects happen.

6. **Self-Validation**
   Before returning anything, check the work against the Skill's own Success Criteria (command-contract.md field). A Skill must not report success it hasn't actually verified.

7. **Output**
   Emit the result per the Skill's Output field. Diff-first, terse — no restating unchanged code or narrating the stages above.

## Extension points

- A Skill MAY insert custom sub-stages between Execution (5) and Self-Validation (6) — e.g. a compile step, a test run. These are Skill-specific and don't need to be declared in this contract.
- Read-only/analysis Skills MAY skip stage 3 (Pattern & Template Resolution) — there's no code shape to template when nothing is being written.
- No Skill may skip stage 2 (Reading Contract Resolution) or stage 7 (Output) — every Skill declares what it reads and produces a result, even if the result is "nothing to do."
- Adding a new stage to this contract is a change to this file, applying to every Skill at once — do not add ad hoc stages inside an individual `SKILL.md` that aren't either a declared extension point or a proposed amendment here.
