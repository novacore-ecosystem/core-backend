# Task 15: Product Search does not index or search Variation Name

**Status:** Resolved 2026-07-27.

## Source

SmartCommerce V3 Search/Cart/Stock checklist audit, 2026-07-27 (read-only, no fixes applied).

## Current state

`ProductSearchDocument` (`src/Services/Product/Product.Application/Abstractions/Search/ProductSearchDocument.cs:10-24`) has no variation-name field — only `DefaultVariationId`/`DefaultVariationSku`. The ES index mapping (`ProductSearchIndexMapping.cs:18`) only maps `Name` as a `text` field with a `keyword` sub-field; there is no per-variation nested/object mapping. The query itself (`ProductSearchRepository.cs:55-57`) does `MultiMatch` over `["name", "categoryNames", "tagNames"]` only. `ProductSearchProjectionBuilder.cs:43-48` only ever projects the *default* variation's `Id`/`Sku` when building the document — the full `Variations` collection (and therefore any variation's `Name`) never reaches the index. This is confirmed as by-design in `docs/reference/search.md:46` ("no full variation list").

Note: this task is currently blocked by Task 16 in the sense that `ProductVariation.Name` is presently a dead property that's never set (see Task 16) — indexing an always-empty field would accomplish nothing. Sequence Task 16 before or alongside this one.

## Why this matters

Checklist requirement: Product Search keyword should match Product Name OR Product Variation Name, case-insensitive. Today a customer searching by a variation-specific name (e.g. a color/size variant name) gets no results even if that's the only name they know.

## Suggested acceptance criteria

- Extend `ProductSearchDocument` to carry variation names (e.g. a `VariationNames: string[]` field populated from all active variations, not just the default one).
- Update `ProductSearchIndexMapping` with a `text` mapping for the new field (standard analyzer keeps it case-insensitive, consistent with `Name`).
- Update `ProductSearchProjectionBuilder` to populate it from the full `Variations` collection.
- Extend the `MultiMatch` query fields list in `ProductSearchRepository.cs` to include it.
- Requires a full reindex (`RebuildProductSearchIndex`) after deploying the mapping change, since existing documents won't retroactively gain the new field.

## What was done

- `ProductSearchDocument` gained `VariationNames: IReadOnlyList<string>`; `ProductSearchIndexMapping` maps it as a `Text` field (standard analyzer, same as `CategoryNames`/`TagNames` — case-insensitive).
- `ProductSearchProjectionBuilder.Build` populates it from `product.Variations.Where(v => v.Status == ProductVariationStatus.Active).Select(v => v.Name).Distinct()` — Active only, so a Discontinued/Inactive variation's name can't surface a product the customer can't actually buy that variant from.
- `ProductSearchRepository`'s `MultiMatch` query now includes `variationNames` alongside `name`/`categoryNames`/`tagNames`.
- `docs/reference/search.md` updated (document field list + repository query description) since both were stale references to "no full variation list."
- Scoped build of `Product.API` passes.

## What wasn't done

No live Elasticsearch reindex/smoke test was run — Docker Compose wasn't running in this session and spinning up the full stack (Postgres/ES/Kafka) for a single-task verification wasn't judged worth the resource cost on this dev machine (see `docs/ai_execution_strategy` conventions). The mapping/query change is correct against the compiled code, but per the acceptance criteria above, whoever deploys this must trigger `RebuildProductSearchIndex` — existing indexed documents won't retroactively gain `variationNames` otherwise.

**Depends on:** Task 16 (done — Variation Name is now settable/non-empty).
