# NovaCore - Credentials & Access Information

**⚠️ IMPORTANT:** These are development credentials. Change them in `.env` before deploying to production!

---

## 🔐 Database Credentials

### PostgreSQL
```
Host:     localhost
Port:     5432
Username: postgres
Password: NovaCore@Postgres2024
```

**Connection String (Development):**
```
Server=localhost;Port=5432;Database=auth_db;User Id=postgres;Password=NovaCore@Postgres2024;
```

---

### MongoDB
```
Host:     localhost
Port:     27017
Username: admin
Password: NovaCore@MongoDB2024
```

**Connection String (Development):**
```
mongodb://admin:NovaCore@MongoDB2024@localhost:27017/
```

---

## 🔑 Service Credentials

### Seq (Logging)
```
URL:      http://localhost:5341
Auth:     None (disabled for development)
Note:     No authentication required for local development
```

### PgAdmin (PostgreSQL Management)
```
URL:      http://localhost:5050
Email:    admin@novacore.local
Password: NovaCore@PgAdmin2024
```

### Mongo Express (MongoDB Management)
```
URL:      http://localhost:8081
Note:     Auto-authenticated with MongoDB credentials
```

---

## 🌐 Application Access

| Service | URL | Port |
|---------|-----|------|
| **API Gateway** | http://localhost:5000 | 5000 |
| **Swagger UI** | http://localhost:5000/swagger | 5000 |
| **Auth API** | via gateway: http://localhost:5000/api/auth | — (not published directly) |
| **Seq** | http://localhost:5341 | 5341 |
| **Kibana** | http://localhost:5601 | 5601 |
| **PgAdmin** | http://localhost:5050 | 5050 |
| **Mongo Express** | http://localhost:8081 | 8081 |

---

## 📊 Infrastructure Services

| Service | Host | Port | Notes |
|---------|------|------|-------|
| PostgreSQL | postgres | 5432 | Primary database |
| MongoDB | mongo | 27017 | Document store |
| Redis | redis | 6379 | Cache layer |
| Kafka | kafka | 9092 | Message queue (KRaft mode) |
| Elasticsearch | elasticsearch | 9200 | Log aggregation |

---

## 🔄 Environment File Location

`.env` - Central configuration file for all services

**Key variables:**
```env
POSTGRES_PASSWORD=NovaCore@Postgres2024
MONGO_INITDB_ROOT_PASSWORD=NovaCore@MongoDB2024
SEQ_FIRSTRUN_ADMINPASSWORD=NovaCore@Seq2024
PGADMIN_DEFAULT_PASSWORD=NovaCore@PgAdmin2024
```

---

## ⚡ Quick Access Commands

### View Database
```bash
# PostgreSQL via PgAdmin
curl http://localhost:5050

# MongoDB via Mongo Express
curl http://localhost:8081

# Via command line
psql -h localhost -U postgres -d auth_db
mongosh --authenticationDatabase admin -u admin -p NovaCore@MongoDB2024
```

### View Logs
```bash
# Seq
curl http://localhost:5341

# Docker logs
docker-compose logs -f seq
docker-compose logs -f auth-api
```

### Test Connection
```bash
# PostgreSQL
docker exec postgres psql -U postgres -c "SELECT version();"

# MongoDB
docker exec mongo mongosh --authenticationDatabase admin -u admin -p NovaCore@MongoDB2024 --eval "db.adminCommand('ping')"

# Redis
docker exec redis redis-cli ping

# Kafka
docker exec kafka kafka-topics --bootstrap-server localhost:9092 --list
```

---

## 🔒 Security Best Practices

### For Development
- ✅ Current credentials are suitable for local development
- ✅ Services are only accessible locally by default
- ⚠️ Change passwords if exposing to network

### For Production
- ❌ **NEVER** use these credentials in production
- ✅ Generate strong, unique passwords
- ✅ Use secrets management (Azure Key Vault, AWS Secrets Manager, etc.)
- ✅ Enable SSL/TLS for all connections
- ✅ Restrict network access with firewalls
- ✅ Use environment-specific `.env.production`

---

## 📝 Changing Credentials

1. **Update `.env` file:**
   ```bash
   POSTGRES_PASSWORD=your-new-secure-password
   MONGO_INITDB_ROOT_PASSWORD=your-new-secure-password
   SEQ_FIRSTRUN_ADMINPASSWORD=your-new-secure-password
   ```

2. **Restart services:**
   ```bash
   docker-compose down
   docker volume prune -f  # Remove old data
   docker-compose up -d
   ```

3. **Update connection strings in code** (if hardcoded)

---

## 🆘 Troubleshooting

### Can't connect to PostgreSQL?
```bash
# Check service is running
docker-compose ps postgres

# View logs
docker-compose logs postgres

# Verify credentials
docker exec postgres psql -U postgres -c "SELECT 1"
```

### Can't connect to MongoDB?
```bash
# Check service is running
docker-compose ps mongo

# View logs
docker-compose logs mongo

# Test connection
docker exec mongo mongosh --version
```

### Forgot a password?
1. Stop the service: `docker-compose down`
2. Remove the volume: `docker volume rm novacore_postgres_data` (or mongo_data, etc.)
3. Update `.env` with new password
4. Start services: `docker-compose up -d`

---

## 📚 Related Documentation

- See `DOCKER_TROUBLESHOOT.md` for Docker issues
- See `GATEWAY.md` for Gateway configuration
- See `.env` for all environment variables
