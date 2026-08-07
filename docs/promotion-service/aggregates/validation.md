# Validation Entities

**Scope:** Domain-layer facts for the Validation entity group, implemented Phase 2.4. Structural only — no business rule (rule evaluation, simulation execution) is implemented yet.

## No aggregate root this time

Unlike every prior "aggregate" group, the brief gave **no Properties/Navigation section for any of these 5 entities** — no `TenantId`, no `Status`/lifecycle field, nothing marking one of them as an `AggregateRoot`. Rather than inventing a root shape (adding `TenantId`/`IAuditable` root fields not given, or arbitrarily promoting one entity to `AggregateRoot<Guid>`), all 5 stay plain `BaseEntity<Guid>` with `IAuditable` only — no `ITenantEntity`. This is a deliberate, literal reading of the brief's omission, not an oversight on this pass's part; flagging for architect confirmation before Persistence (Phase 3) locks in the schema.

Structurally, the 5 entities form two independent FK chains rather than one aggregate:
- `PromotionValidationPolicy` → `PromotionValidationResult` (via `PolicyId`)
- `PromotionSimulation` → `PromotionSimulationScenario` (via `SimulationId`) → `PromotionSimulationResult` (via `ScenarioId`)

All `Create` factories are public (no owning root to restrict construction to `internal`).

## Entities

| Entity | Shape | Notes |
|---|---|---|
| `PromotionValidationPolicy` | `BaseEntity<Guid>` | Name/RuleType/Configuration (opaque string blob)/Priority |
| `PromotionValidationResult` | `BaseEntity<Guid>` | PolicyId/Status (`ValidationResultStatus` enum, Phase 2.6 — was plain string)/Message |
| `PromotionSimulation` | `BaseEntity<Guid>` | Name/CreatedBy — `CreatedAt` inherited, not redeclared |
| `PromotionSimulationScenario` | `BaseEntity<Guid>` | SimulationId/Name/Input (opaque string blob) |
| `PromotionSimulationResult` | `BaseEntity<Guid>` | ScenarioId/Output/Status (`SimulationResultStatus` enum, Phase 2.6 — was plain string) |

## Enums / Value Objects

`ValidationResultStatus` (Passed/Failed) and `SimulationResultStatus` (Success/Failed) added Phase 2.6 — both result-status fields were finite two-value outcomes left as strings only because the original brief omitted an enum. No Value Object was introduced for this group.

## Phase 2.6 correction

`PromotionValidationResult.Status` and `PromotionSimulationResult.Status` converted from plain strings to dedicated enums. The **`TenantId`/`ITenantEntity` omission was re-confirmed, not fixed** — it remains a genuine data-isolation gap (queries against these 5 entities, plus the 4 Audit entities, bypass the platform's automatic tenant-filter mechanism since there is no tenant-scoped parent to inherit isolation from), but adding `ITenantEntity` here would reverse a decision this doc already documents as deliberate and literal, not accidental. Flagging again, more strongly, for explicit architect confirmation before Phase 3 (Persistence) locks the schema — this is the last call to fix it cheaply, before a backfill migration is the only option.

## Reconciliation notes

No `EntityData`/`UpdateData` wrapper types created (Domain Rule 2). No `TenantId`/`ITenantEntity` added to any of the 5 entities, since none was given — see "No aggregate root this time" above and "Phase 2.6 correction" below.
