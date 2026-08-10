# Troubleshooting: Seq

**Scope:** Seq (structured logging) startup and connectivity issues. Condensed from the former `troubleshooting/SEQ.md` (archived) — same content, corrected dead cross-reference (`DOCKER_TROUBLESHOOT.md` never existed; use [setup/docker.md](../setup/docker.md) instead).

## Seq unhealthy / fails to start

**Password format**: special characters (e.g. `@`) in `SEQ_FIRSTRUN_ADMINPASSWORD` can cause parsing errors. Current `.env` sets `SEQ_FIRSTRUN_ADMINPASSWORD` with an `@` character — if Seq fails to start, this is the first thing to check. Fix: either set `SEQ_FIRSTRUN_NOAUTHENTICATION=true` (development-only, disables auth entirely) or change the password to avoid special characters.

**Startup timeout**: Seq's healthcheck needs a generous `start_period` (30s is the current setting in `docker-compose.override.yml`) — if you see `dependency failed to start: container seq is unhealthy` and Seq just needs more time, this is the setting to check/increase, not necessarily a real failure.

**Port conflict**: `lsof -i :5341` (or `netstat -ano | findstr :5341` on Windows) to find the conflicting process, or change `SEQ_PORT` in `.env`.

## No logs appearing in Seq

```bash
docker ps | grep seq                    # is it running?
docker logs seq                         # any startup errors?
curl http://localhost:5341/api/events   # is the API responding?
```

Then check the emitting service: confirm its `SEQ_URL` env var resolves to `http://seq:5341` (internal DNS name, not `localhost`, when running inside Docker) and that its `Program.cs` actually wires `Log.Logger` to `.WriteTo.Seq(seqUrl)` (every service in this solution does this by convention — see any `Auth.API/Program.cs`/`User.API/Program.cs`).

## Seq won't restart / stuck

```bash
docker-compose down
docker volume rm novacore_seq_data   # only if you're OK losing existing logs
docker-compose up -d seq
```

## Quick checklist

- [ ] `SEQ_ACCEPT_EULA=Y` in `.env`
- [ ] Password has no problematic special characters, or `SEQ_FIRSTRUN_NOAUTHENTICATION=true` is set
- [ ] Port `5341` free
- [ ] Healthcheck `start_period` ≥ 30s
- [ ] `curl http://localhost:5341/health` returns 200

## Related

- [setup/docker.md](../setup/docker.md) — general Docker troubleshooting
- [setup/credentials.md](../setup/credentials.md) — access info
- [reference/exceptions.md](../reference/exceptions.md#central-mapping) — the log format every service's exceptions are written in, useful when searching Seq
