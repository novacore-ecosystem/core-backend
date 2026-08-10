# Task 11: `RebuildProductSearchIndex` has no documented auth requirement

**Status:** Open.

## Source

Full-system business-requirements audit, 2026-07-27 (Product feature review).

## Current state

The endpoint exists and works, but its access-control expectation isn't documented — flagged independently by the frontend's own doc (`NovaCoreUI/docs/backend/product/README.md:57`).

## Suggested acceptance criteria

- Document (and verify enforced) the intended auth requirement — presumably admin-only, consistent with other index/maintenance-style endpoints.

**Cross-ref:** NovaCoreUI `docs/tasks/2026-07-27/Task11_rebuild-search-index-dead-code.md` (related — the endpoint also has no frontend caller at all).
