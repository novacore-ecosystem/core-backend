# Reference: Serialization

**Scope:** the shared JSON serialization configuration. Small, single-purpose doc.

`BuildingBlock.SharedKernel/Serialization/JsonSerializerConfiguration.cs` exposes `JsonSerializerOptions.Default` — the one `JsonSerializerOptions` instance every JSON (de)serialization in the solution should use: `RedisCacheService` (cache values), `KafkaFlowEventPublisher`/integration event consumers (Kafka payloads), API responses (via ASP.NET Core's own configured options, kept consistent with this one).

**Rule:** always reference `JsonSerializerConfiguration.Default` instead of `new JsonSerializerOptions()` or `JsonSerializerOptions.Default` (the BCL default) when manually serializing/deserializing anywhere in this codebase — otherwise casing/naming policy can silently diverge between a producer and consumer (e.g. a Kafka publisher and its consumer using different options would fail to round-trip cleanly).
