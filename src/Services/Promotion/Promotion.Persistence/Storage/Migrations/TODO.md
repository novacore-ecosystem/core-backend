# Migration — TODO

**Phase:** 7 (Migration Preparation). Phase 3 (Persistence, all of 3.1-3.5) is now complete — `PromotionDbContext` carries the full 103-entity schema plus Outbox/Inbox — but no EF Core migration has been generated yet.

Migration generation is deliberately deferred, not automatic on Phase 3 closing: run `dotnet ef migrations add InitialCreate` from `Promotion.API` (see `PromotionDbContextFactory.cs` for the design-time context) only when an explicit future prompt calls for it - Phase 7 re-verifies the full migration set against a fresh database as its own readiness check regardless, see [../../../../docs/promotion-service/phases/phase-7-migration-preparation.md](../../../../docs/promotion-service/phases/phase-7-migration-preparation.md). Generating migrations does not itself require Docker/PostgreSQL running (design-time model building only), but this task still isn't triggered until that later prompt.
