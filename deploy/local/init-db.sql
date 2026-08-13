-- Idempotent bootstrap for the LOCAL environment's target databases inside the developer's
-- existing GLOBAL PostgreSQL instance. Run once by the one-shot `db-init` service before any
-- API container starts (see docker-compose.yml). Safe to re-run - CREATE DATABASE can't run
-- inside a transaction/DO block, so each statement uses the \gexec idiom instead: it only
-- executes the generated "CREATE DATABASE ..." when the database doesn't already exist.
--
-- Names match the root project's scripts/postgres/init.sql exactly - these are the same
-- databases the full stack uses, not a parallel naming scheme.

SELECT 'CREATE DATABASE auth_db'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'auth_db')\gexec

SELECT 'CREATE DATABASE auth_hangfire_db'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'auth_hangfire_db')\gexec

SELECT 'CREATE DATABASE user_db'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'user_db')\gexec

SELECT 'CREATE DATABASE user_hangfire_db'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'user_hangfire_db')\gexec

-- Audit/Notification domain data lives in Mongo - these two are Hangfire storage only.
SELECT 'CREATE DATABASE audit_hangfire_db'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'audit_hangfire_db')\gexec

SELECT 'CREATE DATABASE notification_hangfire_db'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'notification_hangfire_db')\gexec
