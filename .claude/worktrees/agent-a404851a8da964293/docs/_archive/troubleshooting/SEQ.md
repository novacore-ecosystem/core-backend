# Seq Troubleshooting Guide

## Common Issues & Solutions

### 1. ❌ Seq Unhealthy / Failed to Start

**Symptoms:**
```
✘ Container seq           Error dependency seq failed to start
dependency failed to start: container seq is unhealthy
```

**Causes & Solutions:**

#### A. Password Format Issues
**Problem:** Special characters in password (like `@`) can cause parsing errors
```env
# ❌ DON'T USE:
SEQ_FIRSTRUN_ADMINPASSWORD=NovaCore@Seq2024

# ✅ USE INSTEAD:
SEQ_FIRSTRUN_NOAUTHENTICATION=true  # For development
```

**Fix:**
```bash
# Set in .env
SEQ_FIRSTRUN_NOAUTHENTICATION=true

# Or use plain password without special chars:
SEQ_FIRSTRUN_ADMINPASSWORD=NovaCoreSeq2024
```

---

#### B. Startup Timeout
**Problem:** Seq takes longer to initialize, health check fails
```yaml
healthcheck:
  start_period: 10s  # ❌ Too short!
```

**Fix:**
```yaml
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:80/health"]
  interval: 10s
  timeout: 5s
  retries: 5
  start_period: 30s  # ✅ Give it 30 seconds to start
```

---

#### C. Port Already in Use
**Problem:**
```
Error: bind: address already in use
```

**Solution:**
```bash
# Find process using port 5341
lsof -i :5341  # macOS/Linux
netstat -ano | findstr :5341  # Windows

# Kill the process or use different port in .env:
SEQ_PORT=5342
```

---

### 2. 🚨 High Memory/CPU Usage

**Problem:** Seq consuming too many resources

**Solution:**
```bash
# Limit container resources
docker-compose.override.yml:
  seq:
    deploy:
      resources:
        limits:
          memory: 512M
          cpus: '0.5'
```

---

### 3. 📊 No Logs Appearing in Seq

**Problem:** Services running but logs not showing

**Verify Seq is working:**
```bash
# Check if Seq is healthy
docker ps | grep seq

# View Seq logs
docker logs seq

# Test API
curl http://localhost:5341/api/events
```

**Check service logging config:**
- Verify `SEQ_URL` is set correctly in service `.env`
- Default: `SEQ_URL=http://seq:5341`
- Ensure service is configured to use Seq
  ```csharp
  Log.Logger = new LoggerConfiguration()
      .WriteTo.Seq(Environment.GetEnvironmentVariable("SEQ_URL") ?? "http://seq:5341")
      .CreateLogger();
  ```

---

### 4. 🔄 Seq Won't Restart

**Symptoms:**
```
Container seq exited with code 1
```

**Solution:**
```bash
# Full reset
docker-compose down
docker volume rm novacore_seq_data
docker-compose up -d

# Or remove just Seq
docker rm seq
docker volume rm novacore_seq_data
docker-compose up -d seq
```

---

## ✅ What Should Work

### Accessing Seq
```
URL: http://localhost:5341
Auth: None (for development)
Status: Should be accessible immediately
```

### Health Check
```bash
# Should return HTTP 200
curl http://localhost:5341/health

# Should return events data
curl http://localhost:5341/api/events
```

### Logs Should Flow
```bash
# Watch Seq logs
docker logs -f seq

# Should show something like:
# [Information] Seq server initialized
# [Information] Started listening on 0.0.0.0:80
```

---

## 🔧 Configuration Reference

### .env Settings
```env
SEQ_CONTAINER_NAME=seq              # Container name
SEQ_PORT=5341                       # External port
SEQ_URL=http://seq:5341             # Internal URL
SEQ_ACCEPT_EULA=Y                   # Must be Y to start
SEQ_FIRSTRUN_NOAUTHENTICATION=true  # No auth for dev
```

### docker-compose.override.yml
```yaml
seq:
  container_name: ${SEQ_CONTAINER_NAME}
  ports:
    - "${SEQ_PORT}:80"
  environment:
    ACCEPT_EULA: ${SEQ_ACCEPT_EULA}
    SEQ_FIRSTRUN_NOAUTHENTICATION: "true"  # ✅ Correct format
  healthcheck:
    test: ["CMD", "curl", "-f", "http://localhost:80/health"]
    interval: 10s
    timeout: 5s
    retries: 5
    start_period: 30s  # ✅ Long enough
```

---

## 📋 Quick Checklist

- [ ] Seq image pulled successfully
- [ ] `SEQ_ACCEPT_EULA=Y` in .env
- [ ] `SEQ_FIRSTRUN_NOAUTHENTICATION=true` (for development)
- [ ] Port 5341 not in use by another process
- [ ] Health check timeout >= 30s start period
- [ ] Can reach `http://localhost:5341`
- [ ] `curl http://localhost:5341/health` returns 200

---

## 🆘 Still Having Issues?

### Debug Steps
```bash
# 1. Check if container is running
docker ps | grep seq

# 2. View detailed logs
docker logs seq --tail=100

# 3. Inspect container
docker inspect seq

# 4. Test connectivity
docker exec seq curl -v http://localhost:80/health

# 5. Full reset
docker-compose down -v
docker-compose up -d seq
```

### Nuclear Option (Last Resort)
```bash
# Remove everything Seq-related and restart
docker-compose down
docker volume rm novacore_seq_data 2>/dev/null || true
docker rmi datalust/seq:latest 2>/dev/null || true
docker-compose up -d seq

# Wait 30 seconds and check
sleep 30
docker ps | grep seq
```

---

## 📚 Related Files
- `.env` - Environment configuration
- `docker-compose.override.yml` - Override settings
- `CREDENTIALS.md` - Access information
- `DOCKER_TROUBLESHOOT.md` - General Docker issues
