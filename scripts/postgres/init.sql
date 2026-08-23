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
CREATE DATABASE promotion_db;
CREATE DATABASE shipping_db;
CREATE DATABASE content_db;
CREATE DATABASE chat_db;

-- Hangfire databases are service-specific and are no longer created here - each
-- service that owns one creates it on demand via scripts/<Service>/init-hangfire-db.sh
-- (Auth, User, Order, Audit, Notification, Content; Payment/Inventory/Product/Promotion/
-- Shipping/Chat have no Hangfire storage).

-- Notes:
-- - All services connect to same pg container during development
-- - Each service uses its own database (auth_db, user_db, etc.)
-- - To scale: create new PostgreSQL containers (pg-user, pg-order, etc.) and update .env connection strings
-- - No cross-service foreign keys allowed (databases are independent)
