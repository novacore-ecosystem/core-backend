# Setup: Observability (Elasticsearch logs + APM)

**Scope:** shipping structured logs to Elasticsearch alongside Seq, and Elastic APM
(OpenTelemetry-based) tracing via a self-hosted `apm-server`. Currently wired for
**Order.API only** as a vertical slice — see "Extending to the remaining services"
below for rolling it out further.

## What's running

| Container | Role |
|---|---|
| `seq` | existing structured-log sink (unchanged, still primary) |
| `elasticsearch` | now also backs log storage, in addition to Product's search index |
| `es-init` | one-shot container; applies the shared `logs-ilm-policy` ILM policy, then exits |
| `kibana` | view logs (Discover) and traces (APM/Observability) |
| `apm-server` | classic Elastic APM Server; receives OTLP traces from instrumented services on port 8200 |

## Log shipping

Each wired service calls `BuildingBlock.Observability.Logging.SerilogBootstrap.ConfigureAppLogging`
(`src/BuildingBlocks/BuildingBlock.Observability/Logging/SerilogBootstrap.cs`) from its
`Program.cs`, which dual-writes to Seq and Elasticsearch. Index naming:
`{service-name}-logs-{yyyy.MM.dd}` (e.g. `order-api-logs-2026.07.22`), one index per
service per day, written directly (not via a rollover alias/data stream).

Retention: `scripts/elasticsearch/init-ilm-templates.sh` creates one shared 14-day
delete-phase ILM policy (`logs-ilm-policy`). Each service's Serilog Elasticsearch sink
auto-registers its *own* index template (`TemplateName = "{service}-logs-template"`,
scoped to that service's index pattern) with `index.lifecycle.name: logs-ilm-policy`
already set via `TemplateCustomSettings` — this sidesteps Elasticsearch's index template
priority-collision rules, which reject a broad hand-written `*-logs-*` template because
it overlaps with several of Elasticsearch's own built-in system templates (`logs`,
`metrics`, `.fleet-*`, etc. at various default priorities). Both the Elasticsearch sink
and the OTLP trace exporter use their default async/batched delivery — no request-path
latency is added by log/trace shipping.

`correlationId` (from the `X-Correlation-Id` header, see
`BuildingBlock.SharedKernel/Constants/HeaderKeys.cs`) is pushed onto every log line via
`BuildingBlock.Web.Middleware.CorrelationIdMiddleware`, and stamped as a tag on the
current OTel `Activity` — so a request can be pivoted between Kibana Discover (logs) and
the APM/Observability trace view using the same value. It's a linked ID, not unified
with OTel's own `trace.id`.

## Tracing (APM)

Services call `BuildingBlock.Observability.Tracing.ObservabilityExtensions.AddOpenTelemetryObservability`
in `Program.cs`, which registers ASP.NET Core + HttpClient instrumentation and an OTLP
exporter pointed at `apm-server:8200`. Sampling is `AlwaysOnSampler` (100% capture) —
this project runs a single environment with no real production traffic, so there's no
cost/volume budget to protect. DB spans (Npgsql) are added per-service via a small
`AddPersistenceTracing()` extension in each service's `*.Persistence` project (see
`Order.Persistence/DependencyInjection.cs`), keeping the DB driver dependency out of the
shared `BuildingBlock.Observability` package.

## Viewing data

- Kibana Discover (`http://localhost:${KIBANA_PORT}`): create a data view over
  `order-api-logs-*`, filter/search on the `correlationId` field.
- Kibana APM/Observability: traces for Order's endpoints (e.g. `POST /orders`, Cart
  endpoints) appear tagged with `correlationId`; DB spans nest under the request span.

## Known limitations

- `CorrelationIdMiddleware` is registered late in `UseMiddlewares()` (after global
  exception handling, auth, and routing in `UseApplication()`), so exceptions or auth
  failures thrown upstream of it won't yet carry `correlationId` in their log lines.
  Hoisting it earlier in the pipeline is a fast-follow once this vertical slice is
  verified stable.
- `apm-server`'s config (`scripts/apm-server/apm-server.yml`) sets
  `data_streams.wait_for_integration: false`. Without it, this classic self-hosted
  apm-server blocks all ingestion waiting for Kibana's Fleet "APM integration" package to
  provision `traces-apm*`/`metrics-apm*` index templates — which isn't installed here.
  Data still lands in the default `traces-apm-default`/`metrics-apm-default` data
  streams; only the Fleet-managed Kibana quickstart dashboards are unavailable.
- `apm-server`'s Docker image has no `curl`/`wget`, so its healthcheck
  (`docker-compose.override.yml`) uses a bare bash `/dev/tcp` connect instead of an HTTP probe.

## Extending to the remaining services

Auth, User, Product, Inventory, Audit, Notification aren't wired yet. Repeat Order's
wiring: reference `BuildingBlock.Observability`, replace the inline `UseSerilog(...)` in
`Program.cs` with `ConfigureAppLogging`, add `AddOpenTelemetryObservability`, register
`CorrelationIdMiddleware` in `UseMiddlewares()`, and add each service's DB
instrumentation (Npgsql for Postgres-backed services, `MongoDB.Driver.Core.Extensions.DiagnosticSources`
for Audit/Notification's Mongo-backed ones) — plus the matching
`Logging__Elasticsearch__Url` / `Observability__ApmServerUrl` env vars and
`elasticsearch`/`apm-server` `depends_on` entries in `docker-compose.yml`.
