# Task 10: Category/Tag delete has no usage-count precheck

**Status:** Open.

## Source

Full-system business-requirements audit, 2026-07-27 (Product feature review).

## Current state

Deleting a category/tag still referenced by products only fails at the point of a 409 conflict — there is no endpoint reporting "N products reference this category/tag" before attempting the delete.

## Why this matters

Cosmetic/UX gap, not a functional break — the delete is correctly blocked, it's just discovered late (after attempting) rather than upfront.

## Suggested acceptance criteria

- An endpoint or existing list response reports usage count so the UI can warn before a delete attempt.
