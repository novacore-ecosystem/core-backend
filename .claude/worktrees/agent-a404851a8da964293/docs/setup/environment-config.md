# Setup: Environment Configuration

**Scope:** the `.env`/`.env.template` workflow. Condensed from the former `guides/ENV_CONFIGURATION.md` (archived). See [setup/docker.md](docker.md) for how these feed the 3-layer Docker config.

## The pattern

- `.env` — actual values, git-ignored, never committed.
- `.env.template` — every key present, empty values, committed as the reference for what's required.
- `appsettings.json`/`appsettings.Development.json` in each service carry **zero secrets or environment-specific values** — only structural config (e.g. the Gateway's `Gateway:Services:{Key}` shape). Everything else comes from env vars via `docker-compose.override.yml`.

## New developer setup

```bash
cp .env.template .env
# fill in values
docker-compose up -d
```

## Adding a new variable (e.g. for a new service)

1. Add the key(s) to `.env.template` with empty values, grouped under a `# SERVICE NAME` comment block matching the existing section style.
2. Add the same key(s) to `.env` with real local values.
3. Wire them into `docker-compose.override.yml` as `- Some__ConfigPath=${THE_ENV_VAR}` (double-underscore = config-section nesting, matching .NET's configuration binder).
4. Confirm the service's DI code reads that config path (`configuration.GetSection("Some:ConfigPath")` or a bound options class).

Full worked example for adding a service's variables: [workflows/new-service-scaffold.md](../workflows/new-service-scaffold.md#4-docker).

## Security

- `.env` is git-ignored — verify with `git ls-files | grep '\.env'` (should return nothing but `.env.template`).
- Never commit real credentials, even temporarily. If it happens: `git rm --cached .env`, rotate every credential that was exposed, then fix `.gitignore`.
- Production credentials come from a secrets manager, not `.env` — this file is for local/dev only.

## Troubleshooting

| Symptom | Check |
|---|---|
| Service can't connect to a dependency | `docker exec {service} printenv \| grep {KEY}` — confirm the value actually reached the container |
| "Missing configuration" error at startup | `diff .env .env.template` — a key present in the template but missing from `.env` |
| `${VAR}` literally appears instead of being substituted | Confirm `docker-compose` is run from the repo root, where `.env` lives |
