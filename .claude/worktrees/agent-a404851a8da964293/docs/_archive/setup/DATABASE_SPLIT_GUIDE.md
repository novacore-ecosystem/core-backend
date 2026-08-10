# Database Split Guide

**Quick reference for splitting services to separate PostgreSQL containers**

---

## 📊 Current Setup (Development)

```
docker-compose.yml:
  pg (single PostgreSQL container)
    ├── auth_db
    ├── user_db
    ├── inventory_db
    ├── order_db
    ├── product_db
    └── auth_hangfire_db
```

All services connect to single `pg` container during development.

---

## 🔄 How to Split a Service

### Example: Split User Service to Separate Database

**Step 1: Add new PostgreSQL container in docker-compose.yml**

Copy the `pg` service definition and rename:

```yaml
  pg-user:
    image: ${POSTGRES_IMAGE}
    container_name: pg-user
    environment:
      - POSTGRES_USER=${POSTGRES_USER}
      - POSTGRES_PASSWORD=${POSTGRES_PASSWORD}
      - POSTGRES_DB=${PG_DATABASE}
    volumes:
      - postgres_user_data:/var/lib/postgresql/data
      - ./scripts/postgres/init-user.sql:/docker-entrypoint-initdb.d/init.sql
    ports:
      - "5433:5432"
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER}"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - default
```

**Step 2: Update .env connection string**

```env
# Before (using shared pg)
USER_DB_CONNECTION=Server=pg;Port=5432;Database=user_db;...

# After (using separate pg-user)
USER_DB_CONNECTION=Server=pg-user;Port=5432;Database=user_db;...
```

**Step 3: Create service-specific init script (optional)**

`scripts/postgres/init-user.sql`:
```sql
CREATE DATABASE user_db;
```

**Step 4: Update user-api depends_on**

```yaml
  user-api:
    depends_on:
      pg-user:  # Changed from pg
        condition: service_healthy
```

**Step 5: Start the service**

```bash
docker-compose up -d pg-user user-api
```

---

## ✅ Quick Split Pattern

### 1 Service → Dedicated DB (5 steps)

1. **docker-compose.yml**: Add `pg-[service]` container (copy/paste pattern)
2. **.env**: Change `[SERVICE]_DB_CONNECTION` from `pg` to `pg-[service]`
3. **init-[service].sql**: Create init script with `CREATE DATABASE [service]_db;`
4. **docker-compose.yml**: Update service `depends_on: pg-[service]`
5. **Start**: `docker-compose up -d pg-[service] [service]-api`

---

## 📋 Split Checklist

### To Split User Service:

- [ ] Add `pg-user` service in docker-compose.yml
- [ ] Update `USER_DB_CONNECTION` in .env to use `pg-user`
- [ ] Create `scripts/postgres/init-user.sql`
- [ ] Update `user-api` depends_on to use `pg-user`
- [ ] Verify: `docker-compose up -d pg-user user-api`

### To Split All Services:

- [ ] `pg-auth` + `AUTH_DB_CONNECTION`
- [ ] `pg-user` + `USER_DB_CONNECTION`
- [ ] `pg-inventory` + `INVENTORY_DB_CONNECTION`
- [ ] `pg-order` + `ORDER_DB_CONNECTION`
- [ ] `pg-product` + `PRODUCT_DB_CONNECTION`

---

## 🔗 Reusable Pattern

### PostgreSQL Container Template

```yaml
  pg-[SERVICE]:
    image: ${POSTGRES_IMAGE}
    container_name: pg-[SERVICE]
    environment:
      - POSTGRES_USER=${POSTGRES_USER}
      - POSTGRES_PASSWORD=${POSTGRES_PASSWORD}
      - POSTGRES_DB=${PG_DATABASE}
    volumes:
      - postgres_[SERVICE]_data:/var/lib/postgresql/data
      - ./scripts/postgres/init-[SERVICE].sql:/docker-entrypoint-initdb.d/init.sql
    ports:
      - "543X:5432"  # Use 5433, 5434, 5435, etc.
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER}"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - default
```

### Connection String Template

```env
[SERVICE]_DB_CONNECTION=Server=pg-[SERVICE];Port=5432;Database=[service]_db;User Id=postgres;Password=NovaCore@Postgres2026;
```

### Volume Template (at end of docker-compose.yml)

```yaml
volumes:
  postgres_[SERVICE]_data:
```

---

## 📊 Volume Names

| Service | Volume Name |
|---------|------------|
| auth | postgres_auth_data |
| user | postgres_user_data |
| inventory | postgres_inventory_data |
| order | postgres_order_data |
| product | postgres_product_data |

---

## 🔌 Port Mapping

| Service | Port |
|---------|------|
| pg (shared) | 5432 |
| pg-auth | 5433 |
| pg-user | 5434 |
| pg-inventory | 5435 |
| pg-order | 5436 |
| pg-product | 5437 |

---

## 🧪 Verify Split Success

```bash
# Check container is running
docker ps | grep pg-user

# Connect to new database
psql -h localhost -p 5434 -U postgres -d user_db

# Verify service connects
docker logs user-api | grep "Connected"
```

---

## 📝 Notes

- **Development**: Keep all services on single `pg` container (saves memory)
- **Staging**: Split critical services (auth, user) to separate containers
- **Production**: Each service gets dedicated PostgreSQL instance
- **Zero code changes**: Only .env and docker-compose.yml updates needed

---

**Version:** 1.0  
**Pattern**: Copy/paste container definition and update 3 places
