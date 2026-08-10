# Workflow: Performance Optimization

**Read first:** only load [reference/caching.md](../reference/caching.md) if caching is a plausible fix — don't pre-load architecture docs for an investigation task.

## How to investigate

1. **Get a number before you touch anything.** Reproduce the slow path and measure it (Seq structured logs, or a manual timer around the suspect code) — see [troubleshooting/seq.md](../troubleshooting/seq.md) for querying request timings.
2. **Identify the layer.** In this codebase, the usual suspects in order of likelihood:
   - N+1 queries in a repository/handler (EF Core `Include`/projection missing)
   - Missing cache on a read-heavy path that already has `ICacheService` available (see [reference/caching.md](../reference/caching.md))
   - Synchronous blocking (`.Result`/`.Wait()`) outside the accepted startup-only exceptions (see [04-coding-rules.md](../04-coding-rules.md#async))
   - Kafka consumer backpressure (`ConsumerWorkersCount`/`ConsumerBufferSize` in `KafkaOptions`, see [03-building-blocks-reference.md](../03-building-blocks-reference.md#messaging--messagingkafka))
   - gRPC call latency (check `GrpcClientOptions.Timeout`/retry config, see [reference/grpc.md](../reference/grpc.md))
3. **Isolate**: is it this request, or systemic (container CPU/memory, DB connection pool exhaustion, Redis latency)? Check `docker stats` and each service's health check before assuming application-level cause.

## What metrics matter

- Request duration (p50/p95, not just average) — Seq structured logs carry this per request.
- DB query count per request (a sudden N+1 shows as many near-identical queries in a short window).
- Cache hit rate, if the path is supposed to be cached (`RoleCacheService`/similar — no built-in metrics today, so this means eyeballing Redis `MONITOR` or adding a temporary log line, not reading a dashboard).
- Kafka consumer lag, if the slow path is event-driven.

## When to optimize

Only after you've measured and identified the actual bottleneck. Do not: add caching speculatively to a path that isn't measured as slow, change `IsInit`/cron frequency on a background job without evidence it's the bottleneck, or introduce a new abstraction (decorator, saga, etc.) purely for perceived performance — every one of those adds a layer that itself needs to be understood by the next reader for zero proven benefit.
