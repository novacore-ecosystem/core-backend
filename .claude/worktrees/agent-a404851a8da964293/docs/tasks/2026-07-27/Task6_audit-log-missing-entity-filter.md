# Task 6: Audit log query API has no server-side filter by entity or actor

**Status:** Open.

## Source

Full-system business-requirements audit, 2026-07-27. Requirement: Audit Log for Customer/Product/Order — verify "audit querying."

## Current state

`GET /audit-logs` (`Audit.API/Endpoints/ListAuditLogs.cs:25-51`, backed by `AuditLogReadService.cs:13-43`) only filters by `service`, `from`, `to`, plus page/pageSize. There is no `rootEntityType`/`rootEntityId`/`actor` filter, even though `AuditLogEntry` documents carry this data (see the payload richness noted in the audit — `RootEntityType`/`RootEntityId`/`Metadata.Actor` are all present in the stored document, just not queryable by them).

The frontend works around this today by paging up to 5×50=250 records per service and filtering by `entityId` client-side (`AuditTrailDialog.tsx:34-57`) — a limitation the frontend's own component comment and docs already acknowledge.

## Why this matters

Beyond 250 events for a given service, the client-side workaround silently truncates — an entity's audit trail can appear empty or incomplete even though the data exists in MongoDB. This is a real risk if the audit trail is ever relied on for a dispute or compliance question.

## Suggested acceptance criteria

- `GET /audit-logs` accepts `rootEntityType` and `rootEntityId` (and ideally `actor`) as filters, executed server-side against MongoDB.
- A single entity's complete history is retrievable in one paginated call regardless of total event volume for its service.

**Cross-ref:** NovaCoreUI `docs/tasks/2026-07-27/Task8_audit-trail-dialog-client-side-pagination-risk.md` (blocked on this task).
