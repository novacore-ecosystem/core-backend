#!/bin/sh
# Pulls one or more Vault KV-v2 paths, merges them into a single flat env-var set, and
# execs the real container command. Compose sets VAULT_ADDR/VAULT_TOKEN/VAULT_PATHS and
# overrides `entrypoint` to point at this script instead of editing every Dockerfile.
#
# VAULT_PATHS is comma-separated, applied in order - later paths win on key collisions.
# Each entry is self-contained as "<mount>/<secret-path>" (the leading segment is the
# KV-v2 mount, e.g. "kv") - mirrors the parsing the VaultConfigurationExtensions.cs
# .NET extension does, so both consumers of VAULT_PATHS agree on one format. A raw
# Vault API-style path with the literal "data" segment already in it (e.g.
# "kv/data/nova-core/dev/security") is also accepted - that segment is just stripped.
# Put shared infra paths first and the service's own path last so service-specific
# overrides always take precedence (e.g. "kv/nova-core/dev/infra-kafka,
# kv/nova-core/dev/security,kv/nova-core/dev/services/auth").
#
# Every key in the JSON blueprints already matches the ASP.NET Core double-underscore
# config convention (e.g. "Cache__ConnectionString"), so exporting it verbatim is enough
# for the env-var configuration provider to pick it up - no key translation needed.
set -eu

: "${VAULT_ADDR:?VAULT_ADDR is not set}"
: "${VAULT_TOKEN:?VAULT_TOKEN is not set}"
: "${VAULT_PATHS:?VAULT_PATHS is not set}"

merged="{}"
old_ifs=$IFS
IFS=,
for entry in $VAULT_PATHS; do
    IFS=$old_ifs
    entry=$(echo "$entry" | xargs)
    [ -z "$entry" ] && continue

    mount="${entry%%/*}"
    secret_path="${entry#*/}"
    case "$secret_path" in
        data/*) secret_path="${secret_path#data/}" ;;
    esac

    url="${VAULT_ADDR%/}/v1/${mount}/data/${secret_path}"
    response=$(curl -fsS --header "X-Vault-Token: ${VAULT_TOKEN}" "$url")
    merged=$(echo "$response" | jq -c --argjson acc "$merged" '$acc * .data.data')
done
IFS=$old_ifs

eval "$(echo "$merged" | jq -r '
  to_entries[]
  | "export " + .key + "=" + (.value | tostring | @sh)
')"

exec "$@"
