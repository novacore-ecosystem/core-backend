-- Development: Single PostgreSQL container (pg) for all services
-- Each service has its own database within the same container
-- Production: Create separate PostgreSQL containers and databases per service

-- Create service-specific databases
CREATE DATABASE auth_db;
CREATE DATABASE user_db;
CREATE DATABASE inventory_db;
CREATE DATABASE order_db;
CREATE DATABASE product_db;
CREATE DATABASE payment_db;
CREATE DATABASE shipping_db;

-- Create Hangfire databases for background job processing
CREATE DATABASE auth_hangfire_db;
CREATE DATABASE user_hangfire_db;
CREATE DATABASE order_hangfire_db;
CREATE DATABASE audit_hangfire_db;
CREATE DATABASE notification_hangfire_db;

-- Notes:
-- - All services connect to same pg container during development
-- - Each service uses its own database (auth_db, user_db, etc.)
-- - To scale: create new PostgreSQL containers (pg-user, pg-order, etc.) and update .env connection strings
-- - No cross-service foreign keys allowed (databases are independent)
