# Environment Configuration

**Managing .env and .env.template files**

---

## 📋 Overview

- **`.env`** - Actual configuration with sensitive values (IGNORED in git)
- **`.env.template`** - Template with empty values (COMMITTED to git)

This pattern ensures:
- ✅ Sensitive data (passwords, API keys) not in git
- ✅ Team can see required variables
- ✅ Easy onboarding for new developers
- ✅ Easy CI/CD setup

---

## 🔄 The Pattern

### `.env` (Development/Production)
```env
COMPOSE_PROJECT_NAME=novacore
POSTGRES_PASSWORD=MySecurePassword123!
JWT_SECRET=super-secret-key-12345678
API_KEY=sk-12345678...
```

### `.env.template` (Template)
```env
COMPOSE_PROJECT_NAME=
POSTGRES_PASSWORD=
JWT_SECRET=
API_KEY=
```

---

## ✅ Setup for New Developer

1. **Clone repository**
   ```bash
   git clone ...
   cd NovaCore
   ```

2. **Copy template to .env**
   ```bash
   cp .env.template .env
   ```

3. **Fill in values**
   ```bash
   # Edit .env with your local configuration
   nano .env
   ```

4. **Start services**
   ```bash
   docker-compose up
   ```

---

## 🔄 When Adding New Variables

**Step 1:** Add to `.env.template` (with empty value)
```env
# NEW_SERVICE section
NEW_SERVICE_DB_CONNECTION=
NEW_SERVICE_API_KEY=
```

**Step 2:** Commit `.env.template` to git
```bash
git add .env.template
git commit -m "add new service variables template"
```

**Step 3:** Fill in `.env` locally (not committed)
```env
NEW_SERVICE_DB_CONNECTION=Server=pg;Port=5432;Database=new_service_db;...
NEW_SERVICE_API_KEY=some-key-value
```

**Step 4:** Start/test locally
```bash
docker-compose up
```

---

## 📝 Workflow: Adding User Service

### 1. Update `.env.template`

Add new section:
```env
# ============================================================================
# USER SERVICE
# ============================================================================
USER_CONTAINER_NAME=
USER_SERVICE_URL=
USER_PORT=
USER_DB_CONNECTION=
USER_REDIS_URL=
USER_KAFKA_BROKERS=
USER_SEQ_URL=
```

### 2. Commit Template
```bash
git add .env.template
git commit -m "add user service variables to template"
```

### 3. Update Local `.env`

```env
# ============================================================================
# USER SERVICE
# ============================================================================
USER_CONTAINER_NAME=user-api
USER_SERVICE_URL=http://user-api:5100
USER_PORT=5101
USER_DB_CONNECTION=Server=pg;Port=5432;Database=user_db;User Id=postgres;Password=NovaCore@Postgres2026;
USER_REDIS_URL=redis:6379
USER_KAFKA_BROKERS=kafka:9092
USER_SEQ_URL=http://seq:5341
```

### 4. Test
```bash
docker-compose up user-api
```

---

## 🔐 Security Best Practices

### ✅ DO
- [ ] Keep `.env` in `.gitignore`
- [ ] Commit `.env.template` to git
- [ ] Use placeholder/empty values in template
- [ ] Document required variables in comments
- [ ] Rotate secrets regularly
- [ ] Use `.env.prod` for production (also ignored)

### ❌ DON'T
- [ ] Commit `.env` with real passwords
- [ ] Hardcode secrets in code
- [ ] Share `.env` file via email
- [ ] Use same secrets across environments
- [ ] Leave default passwords in production

---

## 📊 File Structure

```
NovaCore/
├── .gitignore                    # ← Ignores .env
├── .env                          # ← Local config (not committed)
├── .env.template                 # ← Template (committed)
├── docker-compose.yml
├── docker-compose.override.yml
└── docs/
    └── setup/
        └── ENV_CONFIGURATION.md  # ← This file
```

---

## 🔍 Checking .gitignore

Verify `.env` is ignored:

```bash
# Check if .env is in .gitignore
grep "\.env" .gitignore

# Verify git doesn't track .env
git ls-files | grep "\.env"  # Should NOT show .env
```

---

## 🚀 CI/CD Integration

### GitHub Actions Example
```yaml
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      # Create .env from template + secrets
      - name: Create .env
        run: |
          cp .env.template .env
          echo "POSTGRES_PASSWORD=${{ secrets.POSTGRES_PASSWORD }}" >> .env
          echo "JWT_SECRET=${{ secrets.JWT_SECRET }}" >> .env
      
      # Build/test with .env
      - name: Build
        run: docker-compose build
```

### GitLab CI Example
```yaml
before_script:
  - cp .env.template .env
  - echo "POSTGRES_PASSWORD=$POSTGRES_PASSWORD" >> .env
  - echo "JWT_SECRET=$JWT_SECRET" >> .env
```

---

## 📋 Sync Checklist

When updating environment variables:

- [ ] Update `.env.template` first (empty values)
- [ ] Commit `.env.template` to git
- [ ] Update local `.env` with actual values
- [ ] Test locally
- [ ] `.env` is in `.gitignore` (don't commit)
- [ ] Notify team of new variables needed

---

## 🆘 Troubleshooting

### Service can't connect to database
```bash
# Check .env has correct value
grep DATABASE_CONNECTION .env

# Check database is running
docker ps | grep pg

# Verify inside container
docker exec auth-api printenv | grep DATABASE_CONNECTION
```

### Missing variables error
```bash
# Compare .env with .env.template
diff .env .env.template

# Fill in missing values
# Then test again
docker-compose up
```

### Accidentally committed .env
```bash
# Remove from git history
git rm --cached .env
git commit --amend -m "remove .env from history"

# Add to .gitignore
echo ".env" >> .gitignore
git add .gitignore
git commit -m "add .env to gitignore"

# Rotate secrets in production!
```

---

## 📚 Related Files

- `.env` — local configuration (git-ignored), project root
- `.env.template` — template for new developers, project root
- `docker-compose.yml` / `docker-compose.override.yml` — consume `.env` variables
- [../architecture/DOCKER_CONFIGURATION.md](../architecture/DOCKER_CONFIGURATION.md) — the 3-layer config pattern this fits into
- [NEW_SERVICE_WORKFLOW.md](NEW_SERVICE_WORKFLOW.md) — adding a new service's variables
