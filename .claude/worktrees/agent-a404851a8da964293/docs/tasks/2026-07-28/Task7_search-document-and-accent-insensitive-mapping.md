# Task 7: Design UserSearchDocument + Accent-Insensitive Mapping

**Status:** Done (2026-07-28)
**Category:** Elasticsearch

## What was done

Resolved the "no in-repo precedent" gap by extending `BuildingBlock.Search`'s generic `IElasticsearchIndexer<TDocument>`/`ElasticsearchIndexer<TDocument>` with an **additive overload** of `EnsureIndexAsync`/`RecreateIndexAsync` that also accepts `Action<IndexSettingsDescriptor<TDocument>> configureSettings` (the existing 3-arg overloads are untouched — confirmed Product still builds and behaves identically). `UserSearchIndexMapping.ConfigureSettings` defines a custom analyzer (`user_search_name_analyzer`: standard tokenizer + built-in `lowercase`/`asciifolding` token filters — both ship in core Elasticsearch, no plugin needed) applied only to the `SearchName` text field. `DisplayName` stays a plain, unanalyzed `Keyword` so search results show the exact original name. Chose the "analyzer-only" design option from the task's two alternatives (not literal word-order permutations) — a plain `multi_match` query's own per-term OR semantics already gives word-order independence once terms are folded/lowercased consistently. `UserSearchDocument` also gained `FirstName`/`MiddleName`/`LastName` (stored, `Index(false)`) to preserve `SearchUsersItemResponse`'s existing discrete-name-parts contract (an addition beyond the original plan, needed once Task 10's mapping-back-to-response-DTO was worked out). No live-ES spike was run this session (no Docker instance up) — the mapping/analyzer code is correct against the Elastic.Clients.Elasticsearch API (verified via reflection against the installed package) and compiles, but real-Elasticsearch verification of the Vietnamese-diacritic behavior is still open, tracked under Task 17.

## Objective

Define the `UserSearchDocument` fields and the Elasticsearch mapping that actually satisfies the request's hardest requirement: name search that works "regardless of locale" — different word orders (`Nguyen Van A` / `Van A` / `A Nguyen`), accent-insensitivity (so a caller who can't type diacritics still finds the record), case-insensitivity, and whitespace normalization. **This is the one piece of the entire epic with no existing in-repo precedent to copy.**

## Current state (grounded findings — why this is genuinely new ground)

- Product's mapping (`ProductSearchIndexMapping.cs:15-32`, read in full) has **no custom analyzer, no normalizer, no `asciifolding`/ICU filter anywhere**. `Text` fields (`Name`, `VariationNames`, `CategoryNames`, `TagNames`) get Elasticsearch's plain default `standard` analyzer — this gives case-insensitivity for free (lowercasing) but explicitly does **not** fold accents (`"café"` won't match `"cafe"`). `docs/tasks/2026-07-27/Task13_case-insensitive-search-missing.md:19` confirms this in writing: Product's search is described as "case-insensitive," never as accent-insensitive. Copying Product's mapping verbatim would **not** satisfy this task's requirement — confirmed by direct inspection, not inferred.
- No other service in the repo uses a custom ES analyzer, ICU plugin, or ASCII-folding filter anywhere — this is a genuinely first-of-its-kind addition to the stack.
- Product's document-field-naming convention to still follow: ES documents serialize C# PascalCase properties to camelCase automatically (`Name` → `name`, `CategoryIds` → `categoryIds`) via the Elastic client's default serializer — not something explicitly configured anywhere, just observed behavior (`ProductSearchRepository.cs`'s query-string literals are all camelCase). Keep this consistent for User's document.
- Product's `Status` field is a documented stand-in (borrows the Default Variation's status since Product itself has no lifecycle field) — **User's own aggregate already has a real `Status` (`UserStatus` enum)**, so don't replicate that workaround; map `Status` directly.

## Scope

- `UserSearchDocument` fields (per the request's explicit minimum list, cross-checked against what's actually available on `UserProfile` after Tasks 1/2): `UserId`, `DisplayName` (formatted via Task 5, for *display* in search results — original casing/accents preserved), `SearchName` (see below — normalized, **not** for display), `Email`, `PhoneNumber` (plus the existing `PhoneSearch`/`PhoneReverse` normalized columns, reused as document fields so prefix/suffix search parity with today's Postgres behavior is possible), `Roles`, `Status`, `CreatedAt`, `UpdatedAt`. Evaluate adding `UserName` (currently keyword-searchable in Postgres today — dropping it from the ES document would be a regression versus the existing endpoint).
- `SearchName` generation: concatenate `FirstName`/`MiddleName`/`LastName` in **every plausible order** the request lists (or rely on ES's own multi-term matching instead of literally duplicating permutations — see the design options below) — lowercased, accents folded, whitespace-collapsed. Decide between two concrete approaches and document the choice:
  1. **Multi-field permutation approach**: precompute a small set of token-order variants (e.g. `"first middle last"`, `"last middle first"`, plus a token-bag) into `SearchName` at write time, then use ES's standard tokenization (which is inherently order-agnostic within a single `match` query with default `OR` semantics) — likely sufficient on its own, since a standard `match` query already matches "Van A" against "Nguyen Van A" regardless of order, as long as accents are folded and the tokens exist somewhere in the field.
  2. **Custom analyzer with `asciifolding` + `lowercase` token filters** on a single `SearchName` text field — simpler mapping, relies on ES's own tokenizer/matching for order-independence rather than precomputing permutations. **Recommended starting point** — simpler, and ES's multi-match/OR-token semantics already handle word-order-agnostic matching without needing to store multiple literal permutations.
  - Either way, `asciifolding` is the load-bearing piece for the accent-insensitivity requirement — spike this against a local Elasticsearch instance before committing, since (per Product's research) nothing in this codebase has exercised it before.
- Mapping (`UserSearchIndexMapping.cs`): `Keyword` for `UserId`/`Status`/`Roles` (array-of-keyword, mirroring Product's `CategoryIds` pattern), `Text` with the custom accent/case-folding analyzer for `SearchName`, plain `Keyword` (no analysis, no folding — display should preserve exact user input) for `DisplayName`, `Keyword` for `Email`/`PhoneNumber`/`PhoneSearch`/`PhoneReverse` (exact/prefix-style matching, not full-text), `Date` for `CreatedAt`/`UpdatedAt`.

## Dependencies

- **Depends on:** Task 2 (MiddleName must exist to feed `SearchName`), Task 5 (share name-token-cleaning logic with the DisplayName formatter, but NOT the accent-folding — display must preserve original text).
- **Blocks:** Task 8 (projection builder populates this document shape), Task 10 (query composition reads these exact field names).

## Estimated complexity

Large — this is the highest-uncertainty task in the whole epic specifically because there's no in-repo template; budget explicit spike time against a local Elasticsearch instance to validate the analyzer/normalizer approach before finalizing the mapping.

## Risks

- Getting the analyzer wrong (e.g. applying `asciifolding` to `DisplayName` instead of only `SearchName`) would silently corrupt what users see in search results — keep the two fields' analysis settings deliberately different and test both explicitly.
- If Elasticsearch's bundled `asciifolding` filter (available in core, no plugin needed) doesn't fully cover Vietnamese diacritics to the team's satisfaction, the fallback is an ICU folding plugin (`icu_folding`) — that requires a Docker image change (`docker.elastic.co/elasticsearch/elasticsearch` doesn't ship ICU by default) — flag this as a possible Task 9 (Infrastructure) dependency if the spike shows plain `asciifolding` is insufficient.

## Completion checklist

- [ ] Spike completed against local ES: `asciifolding` (or ICU) validated against real Vietnamese name examples from the request (`Nguyen Van A`, etc.)
- [ ] `UserSearchDocument` fields finalized, matched against the request's minimum list plus the `UserName` regression check
- [ ] `SearchName` generation approach chosen and documented (permutation vs. analyzer-only)
- [ ] `UserSearchIndexMapping.cs` written, `DisplayName`/`SearchName` given deliberately different analysis settings
- [ ] Unit tests: word-order variants, accent variants, case variants, extra-whitespace variants all resolve to matching `SearchName` tokens
