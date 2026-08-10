# Rules Library

**Scope:** an index of rule categories to their owning document — the official engineering standard, organized by responsibility. This file does not restate rules; it routes to them. Where no owning document exists yet, the category is marked `gap` rather than filled with invented content — see the note at the bottom on why.

## Index

| Category | Owning document |
|---|---|
| Architecture | `docs/02-architecture-rules.md` |
| DDD | `docs/conventions/domain-coding-conventions.md` |
| Entity | `docs/conventions/domain-coding-conventions.md` |
| CQRS | `docs/conventions/application-coding-conventions.md`, `docs/04-coding-rules.md` (CQRS shape section) |
| Persistence | `docs/conventions/persistence-coding-conventions.md`, `docs/04-coding-rules.md` (Repository & Transaction sections) |
| Exception | `docs/reference/exceptions.md` |
| Validation | `docs/04-coding-rules.md` (Validation section) |
| Caching | `docs/reference/caching.md`, `docs/04-coding-rules.md` (Caching / decorator pattern section) |
| Naming | `docs/04-coding-rules.md` (Naming conventions section) |
| Testing | `docs/testing/TestingGuidelines.md` |

## Gaps (no dedicated rules document exists yet)

| Category | Nearest partial coverage | Note |
|---|---|---|
| Performance | `docs/workflows/performance-optimization.md` | That doc is an *investigation workflow* (how to diagnose a slow path), not a set of binding performance rules (e.g. query batching limits, N+1 policy, pagination defaults). Don't treat the workflow doc as a rules source for new code. |
| Logging | `docs/setup/observability.md`, `docs/troubleshooting/seq.md` | Both are operational/setup docs (how logs ship, how to debug Seq), not a rules doc (what must be logged, at what level, with what structured fields). |
| Security | `docs/reference/authorization.md` | Covers authN/authZ policy usage only. No consolidated doc for input handling, secrets, OWASP-class concerns across services. |
| Concurrency | Scattered — e.g. optimistic concurrency tokens mentioned in `docs/services/order-service.md` and `docs/tasks/2026-07-27/Task23_updateorder-always-fails-not-a-race-condition.md`, transaction ownership in `docs/conventions/persistence-coding-conventions.md` | No single doc states the project's concurrency-control policy (when to use a concurrency token vs. a distributed lock vs. neither) — `.claude/framework/boundaries.md`'s idempotency/lock framework memory context is the closest existing decision, but it isn't written into `docs/` yet. |

**Why these are left as gaps, not authored now:** writing a Performance/Logging/Security/Concurrency rules doc from scratch would mean inventing the team's standard rather than documenting one that already exists — the same guessing this framework exists to eliminate (see `reading-contracts.md` resolution rule 5, `shared-rules.md` §1). When a Skill or a human hits one of these gaps, the correct response is to report it, not silently improvise a rule and treat it as established. Filling these in is a deliberate, separate authoring task — see `FRAMEWORK.md` for how to add a rule doc once one is ready to write.
