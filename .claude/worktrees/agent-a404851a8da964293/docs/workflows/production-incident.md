# Workflow: Production Incident

**Read first:** [troubleshooting/seq.md](../troubleshooting/seq.md), the affected service's doc under `services/`. Do not read architecture docs first — triage, then investigate.

## Suggested investigation order

1. **Scope the blast radius.** One service down, or the Gateway itself? Check `docker compose ps` / container health status first — see [setup/docker.md](../setup/docker.md#troubleshooting).
2. **Check Seq for the error.** Structured logs carry `[StatusCode] [Client Message] ... [System Message] ... [Stack Trace]` for every handled exception (`ExceptionHandlerHelper`'s log format, see [reference/exceptions.md](../reference/exceptions.md)) — search by service/time window before grepping source.
3. **If it's a 5xx surfacing as "unexpected exception":** the thrown exception type isn't recognized by `ExceptionHandlerHelper` — this is a known failure mode (see [services/user-service.md](../services/user-service.md#known-issues) for a live example) and usually means a raw BCL exception was thrown somewhere it shouldn't have been. Check the stack trace's originating handler against [02-architecture-rules.md](../02-architecture-rules.md#exception-rule).
4. **If it's connectivity** (DB/Redis/Kafka unreachable): check `docker-compose.yml`'s `depends_on`/health checks and the relevant `setup/` doc for the component. Only the Gateway is host-published — if a downstream service is unreachable *from outside*, that's expected (see [01-architecture-map.md](../01-architecture-map.md#networking)); check whether the Gateway itself can reach it instead.
5. **If it's auth-related** (401s that shouldn't happen): distinguish Gateway-level rejection (JWT integrity failure or refresh-token-not-in-Redis, see [services/gateway.md](../services/gateway.md)) from service-level rejection (role/policy failure inside the target service, see [reference/authorization.md](../reference/authorization.md)) — the Gateway and the service log separately, check both.
6. **If it's event/message related** (data not propagating between services): check Kafka consumer lag and topic name match (`{serviceName}.{eventType}` lowercased) — see [reference/events.md](../reference/events.md) and [workflows/add-integration-event.md](add-integration-event.md) for the exact naming rule.

## After mitigating

- If the root cause is a code bug, follow [workflows/fix-bug.md](fix-bug.md) for the actual fix — don't leave a manual mitigation (e.g. a restarted container, a manually cleared cache key) as the permanent resolution.
- If the root cause reveals a documentation gap (a failure mode nobody had written down), add it to the relevant doc's "Known issues" section as part of closing the incident, not as a follow-up that may never happen.
