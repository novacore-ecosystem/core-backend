# Task 19 — Elasticsearch Search Relevance Audit (User + Product)

Status: **Audit approved, decisions made 2026-07-28 — implementation (Tasks 20-27) not yet started.**

## Decisions (2026-07-28)

1. **Email:** whole-token matching is sufficient (already works today) — no change to Email mapping/query.
2. **Product SKU:** index **all** variation SKUs, not just the default variation's — Task 24 is confirmed in-scope (schema addition to `ProductSearchDocument`/`ProductSearchProjectionBuilder`), not blocked on a further product-owner decision.
3. **Brand field:** deferred entirely — out of scope for this effort (`ProductEntity` has no Brand today; a domain change is a separate initiative).
4. **Sequencing:** alias-based blue/green reindex infra (Task 20) goes **first**, before any mapping-changing task (21/23/24), per the recommendation — avoids a search-unavailable window on every reindex from here on.

## Why this exists

Both User Search and Product Search currently require near-complete input to return
results ("administrator" must be typed almost in full; long product/user names need
near-exact matches). This document audits root cause across both services and proposes
a redesign, per the request in chat. See [[user_service_search_locale_cache_epic]] for
the epic that originally built this infrastructure (Tasks 6–18, completed earlier today).

---

## Phase 1–3: Audit findings (mapping, analyzers, query strategy)

### User Service (`user-search` index)

| Aspect | Current state | File |
|---|---|---|
| Analyzer | One custom analyzer `user_search_name_analyzer` = `standard` tokenizer + `lowercase` + `asciifolding`. Applied **only** to `SearchName`. | `UserSearchIndexMapping.cs` |
| `SearchName` | `Text`, custom analyzer. Built from `FirstName + MiddleName + LastName` joined by space. | `UserSearchProjectionBuilder.cs` |
| `DisplayName` | `Keyword` (unanalyzed, exact casing preserved for UI display only) | |
| `FirstName`/`MiddleName`/`LastName` | `Keyword`, `Index(false)` — **not searchable at all**, stored only | |
| `UserName`/`Email` | `Text`, default `standard` analyzer + `.keyword` subfield for sorting | |
| `PhoneNumber`/`PhoneSearch`/`PhoneReverse` | `Keyword`, no analysis — prefix/suffix only via `Prefix` query on normalized digit strings | |
| `Roles`/`Status` | `Keyword`, case-sensitive exact match (no normalizer) | |
| Query | Single `bool`: `must` = one `multi_match` (default `best_fields`, no `fuzziness`, no `operator`) over `["searchName","userName","email"]`; `filter` = `term` on `roles`/`status`, `prefix` on `phoneSearch`/`phoneReverse`; `must_not` for role exclusion. | `UserSearchRepository.cs` |
| Roles search | Role is `Keyword` + `term` query = **exact string match only**. Searching `adm` for `Administrator` returns nothing — this is the reported "administrator" bug. | |

### Product Service (`product-search` index)

| Aspect | Current state | File |
|---|---|---|
| Analyzer | **None.** No `ConfigureSettings`/analysis block exists at all. Every text field uses ES's default `standard` analyzer (case-insensitive only, no accent-folding, no partial-token support). | `ProductSearchIndexMapping.cs` |
| `Name`, `VariationNames`, `CategoryNames`, `TagNames` | `Text`, default analyzer, `Name` has a `.keyword` subfield for sorting | |
| `Code` | `Keyword` — exact match only | |
| SKU coverage | Only `DefaultVariationSku` is indexed (`Keyword`), and it is **not** included in the `multi_match` fields — SKU is not searchable via free text at all | |
| Brand / Description / Metadata | **Not indexed. Fields don't exist** in `ProductSearchDocument` or in `ProductEntity` usage by the projection builder | |
| Query | Single `bool`: `must` = one `multi_match` (default `best_fields`, no fuzziness) over `["name","variationNames","categoryNames","tagNames"]`; `filter` = `term` on `categoryIds`/`tagIds`/`status`. | `ProductSearchRepository.cs` |

### Root cause of "must type almost the whole word"

Both services use `multi_match` with the **default `best_fields` type** against fields
tokenized by either `standard` (Product, User's `userName`/`email`) or `standard +
lowercase/asciifolding` (User's `searchName`). None of these tokenizers do partial-word
matching — a `standard` analyzer only produces whole tokens ("administrator" is a single
token). A `multi_match`/`match` query on a whole-token field can only match when the
**query is itself a complete token** (or matches via analyzer-side tokenization of
multi-word queries). There is:

- **No edge-ngram field** for prefix-as-you-type matching.
- **No `match_bool_prefix`/`search_as_you_type`** field type anywhere.
- **No `fuzzy`/`AUTO` fuzziness** parameter on any query — so "Jhon" never matches "John".
- **No `wildcard`/`prefix` query on analyzed text fields** — the only `prefix` queries
  in the codebase are on the `Keyword` phone fields, unrelated to name/product search.
- Role matching is `term` on an unanalyzed `Keyword` — inherently exact-match, cannot
  partial-match by design, not an analyzer gap.

This fully explains all three reported symptoms: role search requiring the full string,
long product names requiring near-complete input, and long user names requiring
near-complete input. It is a **query/mapping design gap**, not a performance or
infrastructure gap — the ES cluster, client, indexing pipeline, and sync/rebuild flows
(both services self-consume Kafka events into `IUserSearchIndexer`/`IProductSearchIndexer`,
plus admin-triggered `RecreateIndexAsync` rebuild endpoints) are all sound and reusable
as-is.

### Other correctness gaps found during audit (secondary, worth fixing alongside)

- User: `Enum.Parse<UserStatus>(d.Status)` in `SearchUsersHandler` has no fallback —
  throws on unexpected/empty status.
- User: `SearchUsersHandler.BuildCriteria` only honors `request.Sorts.FirstOrDefault()`
  — multi-sort requests silently drop all but the first clause.
- User: role filter only supports a single role value (`Eq`/`Ne`), no multi-role `in`.
- Product: no `Brand` field on the aggregate/document at all — "Searching brand names"
  from the objectives is not currently possible without a schema addition upstream of
  search.
- Both: index bootstrap is fail-open (ES down at startup → search silently degraded,
  no crash) — acceptable, not a defect, just worth reconfirming as intentional.
- Both: no alias/blue-green reindexing — `RecreateIndexAsync` does a blocking
  drop+create, causing an availability gap during rebuild.
- Both: deep pagination via `from`/`size` only, no `search_after` — will degrade at
  large `from` values, not a near-term concern given current data volume.

---

## Phase 4–5: Redesigned search behavior & ranking strategy

### Recommended approach: `search_as_you_type` + `edge_ngram` hybrid, tiered `bool` query

Reuse the existing custom-analyzer infrastructure (`ConfigureSettings` hook already
exists and is wired for User via the 4-arg `EnsureIndexAsync`/`RecreateIndexAsync`
overloads in `IElasticsearchIndexer<TDocument>` — Product just needs to start using the
overload it already has access to but doesn't call).

**For name/product-name/tag/category fields ("should support partial + typo"):**

Use ES's built-in `search_as_you_type` field type (Elasticsearch 7.2+, no plugin
required) instead of hand-rolled edge-ngram fields. It automatically generates
`field`, `field._2gram`, `field._3gram`, and `field._index_prefix` subfields, and is
designed to be queried with `multi_match` + `type: bool_prefix` — this is the standard
production pattern (Elastic's own docs and Shopify/GitHub-style admin search UIs use
this exact mechanism) and needs no additional index size tuning beyond what edge-ngram
would cost, but with far less mapping complexity.

Query shape becomes a **tiered `bool` `should`** to get the exact → prefix → contains →
fuzzy ranking Phase 5 asks for, instead of one flat `multi_match`:

```
bool.should:
  1. match_phrase (boost: 10)         — rewards exact/near-exact phrase matches highest
  2. multi_match type=bool_prefix     — rewards prefix-of-any-token matches (the "burg" → "Burger" case)
  3. multi_match type=best_fields,
     fuzziness=AUTO                   — typo tolerance, lowest boost, only for name-like fields
  minimum_should_match: 1
```

This directly satisfies the Phase 5 ordering requirement (exact → prefix → contains →
fuzzy) via boost values rather than separate queries, keeping one round-trip to ES.

**For role/status-like short controlled vocabularies ("adm" → "Administrator"):**

Do **not** apply `search_as_you_type` here — roles are a small, known, finite set.
Instead add an `edge_ngram` **prefix-only** subfield (min_gram 2, max_gram 15) on
`roles`, keeping the existing `Keyword` field for exact filtering untouched. Query with
a plain `match` against the ngram subfield. This is cheaper than `search_as_you_type`
for a low-cardinality field and avoids fuzzy false-positives on short role names (fuzzy
matching "man" against "Manager" via edit-distance is risky — prefix is exactly what's
wanted and asked for in the objectives, not typo tolerance).

**For email:** keep `standard` analyzer (already tokenizes on `@`/`.`, so "john" and
"gmail" already match today) — add the same `search_as_you_type` treatment only if
partial mid-token search (e.g. "gma" → gmail) is desired; the objectives example only
requires whole-token search ("gmail" not "gma"), which already works. No change needed
unless the user wants partial-token email search too — flag as an open question below.

**For phone:** current prefix/suffix design (normalized digits + reversed-digits trick)
is already the textbook approach for cheap prefix+suffix search without wildcards —
**no change recommended**, it's a correct pattern already.

**Status:** already filter-only (`term` in `filter`, not `must`) for both services —
already matches the Phase 4 requirement ("status should behave as a filter"). No change
needed.

**Word-order independence:** already works today for User (`multi_match`'s OR-of-terms
semantics against `SearchName`) and will continue to work with `bool_prefix`/fuzzy
tiers layered on top. No change needed for word order specifically.

### Fields to add to mapping

| Service | Field | Type change |
|---|---|---|
| User | `SearchName` | `search_as_you_type` (replaces plain `Text` + custom analyzer subfields become moot — `search_as_you_type` needs its own analyzer config; keep `lowercase`+`asciifolding` as its analyzer) |
| User | `UserName` | `search_as_you_type` |
| User | `Roles` | add `roles.prefix` edge_ngram subfield alongside existing `Keyword` |
| Product | `Name` | `search_as_you_type` |
| Product | `VariationNames`, `CategoryNames`, `TagNames` | `search_as_you_type` (or plain fuzzy `multi_match` if index-size budget is tight — see Phase 6) |
| Product | new `Brand` field (requires upstream domain change — out of scope for search-only work, flag to product owner) | `search_as_you_type` once it exists |
| Product | `Code` / SKU | consider a dedicated `sku` edge_ngram or `wildcard`-free prefix field if "searching SKU" partial-match is required; currently `Code` is `Keyword` exact-only and no full SKU list is indexed at all (only default variation's SKU) |

---

## Phase 6: Performance review

- **`search_as_you_type` cost:** generates 3–4 subfields per field at index time
  (roughly 3–4x the token count vs. a plain `Text` field for that field only). For the
  actual field set involved (Name/SearchName/UserName/VariationNames/CategoryNames/TagNames
  — all short strings, not large description blobs), this is a bounded, predictable
  cost and is the standard production trade-off; it is far cheaper than wildcard/regexp
  queries (which are O(index size) at query time, not bounded at index time) or a
  naive custom edge-ngram-on-every-field approach (which tends to over-generate grams
  if min_gram is set too low).
- **Avoid:** wildcard/regexp queries — none exist today, keep it that way; they were
  correctly avoided in the phone-search design already (prefix+reverse trick instead
  of wildcard `*1234`).
- **Avoid indexing `Description` for ranking** (Phase 4 explicitly asks to review
  this): Product's `Description` isn't indexed today at all. Recommend **do not add
  it to the primary `multi_match`/`bool_prefix` should-clauses** — long free-text
  fields dilute relevance scoring and inflate index size for comparatively low search
  value. If description search is wanted, add it as a **separate, lower-boost
  `should` clause** with plain `match` (no ngram/prefix expansion) rather than folding
  it into the same tiered query as Name.
- **Completion Suggester:** not recommended — it requires a dedicated `completion`
  field with its own FST-based structure, mainly useful for a dropdown-typeahead UX
  distinct from the current filtered-list search endpoints (`SearchUsers`/`SearchProducts`
  return full filtered/sorted paginated resultsets, not a suggestion dropdown). Skip
  unless a typeahead UI is explicitly planned — would be over-engineering per the
  Phase 7 instruction.
- **Filter cache:** all `filter` clauses used today (`term` on `roles`/`status`/
  `categoryIds`/`tagIds`, `prefix` on phone fields) are already correctly placed in
  `bool.filter` (not `must`), which is what makes them eligible for ES's filter
  cache and non-scoring — already following best practice, no change needed.
- **Rebuild/reindex cost:** switching field types requires a full mapping change,
  which ES does not support in-place — requires `RecreateIndexAsync` (already exists)
  or, better, a one-time move to alias-based blue/green reindexing (currently
  missing on both services) to avoid a search-unavailable window during the
  migration. Recommend doing the alias migration as prerequisite infra work bundled
  into this same effort, not deferred again.

---

## Phase 7: Best-practice comparison

- **GitHub/Shopify/Jira-style admin search:** exactly the `search_as_you_type` +
  tiered-boost `bool` pattern recommended above — this is the standard, not a novel
  design. Adopting it aligns with production norms rather than reinventing.
- **Amazon/e-commerce-style typo tolerance + prefix-as-you-type:** covered by the
  fuzzy tier + `bool_prefix` tier.
- **Not worth adopting for this project's scale:** Completion Suggester (see Phase 6),
  synonym dictionaries (no evidence of a synonym requirement in the objectives —
  role/product names are project-specific vocabulary, not general English synonyms),
  n-gram-on-everything (would blow up index size for description-length fields with
  little relevance payoff), custom scoring scripts (`function_score` with script —
  boost-based `should` tiers achieve the same ranking without script-query overhead
  or the maintenance cost of Painless scripts).

---

## Phase 8: Implementation plan (dependency order)

**Do not implement without approval — this is the proposed breakdown only.**

1. **Task 20 — Alias-based index versioning (both services).** Prerequisite: lets
   later mapping-breaking changes redeploy via blue/green swap instead of a blocking
   drop+create. Touches `UserSearchIndexNames`/`ProductSearchIndexNames`,
   `ElasticsearchIndexer<TDocument>`, both `*SearchIndexer` classes. No mapping/query
   change yet — pure infra, independently shippable, de-risks every task after it.

2. **Task 21 — User: `SearchName`/`UserName` → `search_as_you_type`, tiered query
   rewrite.** Mapping change (`UserSearchIndexMapping`) + query rewrite
   (`UserSearchRepository.BuildBoolQuery`) to the 3-tier `should` (`match_phrase` →
   `bool_prefix` → `fuzzy best_fields`). Requires reindex via Task 20's alias swap.

3. **Task 22 — User: `Roles` edge_ngram prefix subfield + query update.** Additive
   mapping field, existing `Keyword` untouched (filter behavior unaffected), only the
   free-text role search path changes.

4. **Task 23 — Product: add analyzer settings + `Name`/`VariationNames`/
   `CategoryNames`/`TagNames` → `search_as_you_type`, same tiered query rewrite as
   Task 21, applied to `ProductSearchRepository`.** This is the first time Product
   gets a `ConfigureSettings` block at all (currently has none) — start calling the
   4-arg `EnsureIndexAsync`/`RecreateIndexAsync` overload already present in
   `IElasticsearchIndexer<TDocument>` but unused by `ProductSearchIndexer`.

5. **Task 24 — Product: index all variation SKUs.** Decided 2026-07-28 — not just the
   default variation's. Schema addition: `ProductSearchDocument` gains a `VariationSkus`
   list (mirrors the existing `VariationNames`/`VariationIds` shape), populated by
   `ProductSearchProjectionBuilder`, added to the `multi_match`/tiered query's field set.

6. **Task 25 — Brand field: deferred, not implemented.** `Brand` does not exist on
   `ProductEntity` today — this is a domain/persistence change outside search's scope.
   Explicitly out of scope for this epic (decided 2026-07-28); revisit only if/when
   Brand is added to the domain independently.

7. **Task 26 — Fix the two correctness bugs found during audit** (`Enum.Parse`
   fallback in `SearchUsersHandler`, multi-sort clause support) — unrelated to
   relevance redesign, safe to do independently/in parallel with any of the above.

8. **Task 27 — Testing.** No dedicated automated tests exist today for either search
   path (`find -iname "*ProductSearch*Test*"` → zero hits; User search test status
   should be reconfirmed — Task 17 in the original epic covered some testing but the
   new tiered-query behavior needs new relevance-focused test cases: prefix match,
   fuzzy match, exact-match-ranks-first assertions). Requires a live ES instance per
   [[testing_conventions]] — phased Domain-before-Application approach; here it's
   Persistence-repository-level integration tests against a real (test-container) ES.

### Breaking changes / migration required

- Mapping changes for `SearchName`, `UserName`, `Name`, `VariationNames`,
  `CategoryNames`, `TagNames` are **not compatible in-place** — ES will reject a
  `PUT mapping` that changes an existing field's type. Every task above touching a
  mapping requires a full reindex (`RecreateIndexAsync`, or the new alias swap from
  Task 20).
- No API contract changes anticipated — `SearchUsersQuery`/`SearchProductsQuery`
  request/response shapes stay the same; only server-side query construction and
  index mapping change.
- Existing docs (`docs/reference/search.md`) will need updates once implemented —
  per [[docs_first_rule]].

### Open questions — resolved

See "Decisions (2026-07-28)" at the top of this document. All four questions are
answered; implementation may proceed starting with Task 20.
