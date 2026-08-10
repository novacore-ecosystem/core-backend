# Shared Execution Rules

**Scope:** binding for every Skill under `.claude/skills/`. A `SKILL.md` links here instead of restating these — if a rule needs to change, it changes once, here.

## 1. Context discipline
- Read only what your Skill's Context Loading section lists as MUST, plus MAY items whose stated condition is actually true for this invocation.
- Never explore the repository "just to be sure" beyond that list. If you find you need a file the list doesn't mention, that is a **doc gap**: say so explicitly in your output, then proceed with the narrowest reasonable read — don't silently expand scope.
- Never read another service's source tree unless the Skill's boundaries explicitly allow it (see `boundaries.md`).

## 2. Pattern-first, template-first
- Before writing any code, locate the existing template or convention doc for that construct (`docs/06-implementation-templates.md`, `docs/conventions/*.md`). Reuse its shape.
- If a template doc names a real reference file (e.g. "mirrors `Auth.API/Endpoints/Register.cs`"), open that file and match its actual current shape — the template is a starting point, the real file is ground truth.
- Never invent a second way to do something the project already has a pattern for. If no pattern exists, stop and say so rather than freehanding a novel structure.

## 3. Stop conditions (common to all skills)
Halt and ask the user instead of proceeding when:
- The target is ambiguous (which service/entity/feature — more than one plausible match).
- No template or convention doc covers the construct being requested.
- Two loaded docs conflict on the same fact.
- The requested action would touch files outside the Skill's stated boundaries.
- The action is destructive or hard to reverse (force-push, history rewrite, deleting tracked files) — no Skill in this framework does these; if one seems to require it, stop.

## 4. Output discipline
- Diff-first: show only what changed, never reprint unchanged code.
- Terse: state the result, not the deliberation that produced it.
- Structured findings (Review, Verify) use the `ReportFindings` tool shape, not freeform prose.

## 5. Git-specific rules (Commit skill)
- Never add an AI co-author trailer.
- Never use `--no-verify`, `--no-gpg-sign`, or force flags.
- Never stage or commit files a skill wasn't explicitly asked to act on.

## 6. Layer boundaries
See `boundaries.md` for the canonical per-layer MUST-NOT-read table. Every code-touching skill (`clean`, `complete`, `scaffold`, `sync`) must check it before loading any source file outside the layer named in the invocation.

## 7. Selection Mode contract
When a skill is invoked against an explicit code selection rather than a whole file: the selection boundary is absolute. Never modify, refactor, or reformat anything outside it — not even adjacent lines that look related. If finishing the selection correctly is impossible without touching surrounding code, stop and say so instead of silently expanding scope. Any skill that supports selection-scoped invocation (currently `complete`) follows this rule.
