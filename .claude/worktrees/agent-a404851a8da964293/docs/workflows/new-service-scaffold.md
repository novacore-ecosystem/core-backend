# Workflow: Scaffold a Brand-New Service

**Read first:** nothing else — this workflow sequences everything you need. It supersedes the old `SERVICE_TEMPLATE.md`/`NEW_SERVICE_WORKFLOW.md` (archived, see [08-migration-plan.md](../08-migration-plan.md)), which described a pre-`BuildingBlock.Web` architecture and the wrong port convention. **Auth Service is the model to copy from — mirror its files, don't write from scratch.**

## 1. Projects

Create five projects under `src/Services/{Service}/`, matching Auth's exact set: `{Service}.Domain`, `{Service}.Application`, `{Service}.Infrastructure`, `{Service}.Persistence`, `{Service}.API`. Add them to `NovaCore.sln`. Reference chain per [02-architecture-rules.md](../02-architecture-rules.md#dependency-direction-must-never-be-violated):

- `Domain` → `BuildingBlock.Domain`
- `Application` → `Domain` + `BuildingBlock.Application`
- `Infrastructure` → `Application` + `BuildingBlock.Infrastructure` (+ `BuildingBlock.Grpc`/`BuildingBlock.Messaging.Kafka`/`BuildingBlock.Contract` if this service does gRPC/Kafka)
- `Persistence` → `Application` (repository interfaces) — EF Core + Npgsql packages
- `API` → `Application` + `Infrastructure` + `Persistence` + `BuildingBlock.Web`

## 2. Layer scaffolding

Mirror Auth's folder layout exactly — see [04-coding-rules.md](../04-coding-rules.md#folder-structure-per-feature). Each layer gets one `DependencyInjection.cs` with a single public `Add{Layer}` method (`AddApplication`/`AddInfrastructure`/`AddPersistence`/`AddPresentation`).

`{Service}.API/Program.cs` — copy Auth's/User's `Program.cs` structure: Serilog→Seq, Kestrel dual-listen on **`8080` (REST) / `5002` (gRPC)** — this is the fixed internal port convention, do not invent new ports (see [01-architecture-map.md](../01-architecture-map.md#networking)), then:

```csharp
builder.Services.AddPersistence(config).AddApplication().AddInfrastructure(config).AddPresentation(config);
var app = builder.Build();
// migrate DB
app.UseApplication();
app.Run();
```

`{Service}.API/DependencyInjection.cs` — `AddPresentation` calls `services.AddBuildingBlockWeb(configuration, webOptions).AddCommonAuthorizationPolicies().AddAuthorization(...)`, where `webOptions` is a `BuildingBlockWebOptions` with this service's title/description/route-prefix/contact-url — **do not hand-write Swagger/CORS/Carter/health-check wiring**, `AddBuildingBlockWeb` already does it. See [services/auth-service.md](../services/auth-service.md#di-composition-authapidependencyinjectioncs-authapiapplicationpipelinecs) for the exact call shape.

`{Service}.API/ApplicationPipeline.cs` — `UseApplication` calls `app.UseBuildingBlockWeb(webOptions)` plus this service's own seeding/mapping — again, no hand-written exception-handler/Swagger-UI/CORS middleware calls.

## 3. First entity + first feature

Follow [workflows/add-new-domain-entity.md](add-new-domain-entity.md), then [workflows/add-new-repository.md](add-new-repository.md), then [workflows/add-new-api.md](add-new-api.md) for the first real endpoint.

## 4. Docker

- `src/Services/{Service}/{Service}.API/Dockerfile` — copy Auth's or User's, updating only the project name and the `COPY` list of `BuildingBlocks/*.csproj` files this service actually references (must include `BuildingBlock.Web.csproj` since `API` references it).
- Uncomment/add the service block in `docker-compose.yml` (there are already commented-out placeholders for Inventory/Order/Product to copy the shape from) — `depends_on` whichever of `pg`/`redis`/`kafka`/`seq` this service actually uses.
- Add the service's env vars to `docker-compose.override.yml` and `.env`/`.env.template` — see [setup/environment-config.md](../setup/environment-config.md) for the required/optional variable pattern. **`appsettings.json` must not contain secrets or environment-specific values** — only structural config (e.g. `Gateway:Services:{Key}` shape), same as every existing service.

## 5. Gateway registration

Add a `Gateway:Services:{Key}` entry to `src/ApiGateways/YarpApiGateway/appsettings.json` (`Url`, `Name`, `Path`, `SwaggerUrl`, `RequireAuth`) — see [services/gateway.md](../services/gateway.md#config). The Gateway itself does not need code changes for a new service (routing is config-driven).

## Checklist

- [ ] Ports are `8080`/`5002` internal, nothing published directly (only the Gateway is)
- [ ] `AddPresentation`/`UseApplication` use `AddBuildingBlockWeb`/`UseBuildingBlockWeb` — no hand-rolled Swagger/CORS/Carter/exception-handler code
- [ ] `appsettings.json` has zero secrets/environment values
- [ ] Registered in `NovaCore.sln`, `docker-compose.yml`, `docker-compose.override.yml`, `.env`/`.env.template`, and the Gateway's `appsettings.json`
- [ ] `docs/services/{service}.md` created, following the shape of `services/auth-service.md`, and linked from `docs/README.md`
