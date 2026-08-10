# Workflow: Project Initialization (understand a service quickly)

**Read first:** [01-architecture-map.md](../01-architecture-map.md) → [02-architecture-rules.md](../02-architecture-rules.md) → the target service doc (`services/auth-service.md` or `services/user-service.md`).

## Steps

1. Read [01-architecture-map.md](../01-architecture-map.md) for the system picture — don't explore `src/` yet.
2. Read [02-architecture-rules.md](../02-architecture-rules.md) for layer boundaries and the composition-root pattern.
3. Read the target service's doc under `services/` for its specific routes, ports, and known divergences/issues.
4. If implementing something, stop here and go to [05-context-loading-map.md](../05-context-loading-map.md) to find the right workflow — do not continue exploring source.
5. Only open source files when a workflow or template doc names a specific file to mirror.

## Checklist before writing any code

- [ ] I know which layer my change belongs in (Domain/Application/Infrastructure/Persistence/API) — see [02-architecture-rules.md](../02-architecture-rules.md#layer-responsibilities)
- [ ] I know whether this service is Auth (reference) or User (check divergences first)
- [ ] I have NOT opened more than 2-3 source files "to get a feel for it" — the docs above should be sufficient
