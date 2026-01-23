# 📋 Deployment Checklist - Production Ready

## Pre-Deployment Verification

### Code & Configuration
- [ ] All hardcoded credentials removed from code
- [ ] appsettings.json files do NOT contain sensitive data
- [ ] Environment variables properly documented
- [ ] No TODO comments in production code
- [ ] All new services registered in Program.cs
- [ ] Serilog configured for production

### Security
- [ ] JWT_KEY generated (use: `openssl rand -base64 32`)
- [ ] ALLOWED_ORIGINS configured for your domain
- [ ] HTTPS enabled
- [ ] Security headers added (done: X-Content-Type-Options, X-Frame-Options, etc.)
- [ ] CORS policy restricted (not AllowAll)
- [ ] Input validation on all forms
- [ ] Database credentials not in code

### Testing
- [ ] Form state persistence works (refresh test)
- [ ] Error page displays correctly
- [ ] Validation errors show clearly
- [ ] Reconnection modal works (stop API test)
- [ ] Rate limiting works (100 req/min test)
- [ ] Health check endpoint responds (/health)
- [ ] Logs are created in /logs/ directory
- [ ] No console.log left in production code

### Performance
- [ ] Response compression enabled
- [ ] Static files cached properly
- [ ] Database connection pooling configured
- [ ] API response times acceptable
- [ ] Large files compressed

### Monitoring & Logging
- [ ] Log directory exists and writable (/logs/)
- [ ] Log rotation configured (daily, 30-day retention)
- [ ] Health check endpoint accessible
- [ ] Error notification setup (if applicable)
- [ ] Performance metrics baseline established

---

## Environment Setup

### Step 1: Set Environment Variables

#### Windows (PowerShell)
```powershell
# Copy and run this
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:JWT_KEY = "$(openssl rand -base64 32)"
$env:JWT_ISSUER = "yourdomain.com"
$env:JWT_AUDIENCE = "yourdomain.com"
$env:CONNECTION_STRING = "Password=yourpassword;Persist Security Info=False;User ID=sa;Initial Catalog=HWPI;Data Source=your.server.com;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
$env:CONTROLCENTER_CONNECTION_STRING = "Password=yourpassword;Persist Security Info=False;User ID=sa;Initial Catalog=ControlCenter;Data Source=your.server.com;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
$env:ALLOWED_ORIGINS = "https://yourdomain.com,https://www.yourdomain.com"

# Verify they're set
Get-ChildItem env: | Where-Object {$_.Name -like "JWT*" -or $_.Name -like "*CONNECTION*" -or $_.Name -like "ALLOWED*"}
```

#### Linux/macOS
```bash
# Copy and run this
export ASPNETCORE_ENVIRONMENT=Production
export JWT_KEY=$(openssl rand -base64 32)
export JWT_ISSUER=yourdomain.com
export JWT_AUDIENCE=yourdomain.com
export CONNECTION_STRING="Password=yourpassword;Persist Security Info=False;User ID=sa;Initial Catalog=HWPI;Data Source=your.server.com;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
export CONTROLCENTER_CONNECTION_STRING="Password=yourpassword;Persist Security Info=False;User ID=sa;Initial Catalog=ControlCenter;Data Source=your.server.com;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
export ALLOWED_ORIGINS="https://yourdomain.com,https://www.yourdomain.com"

# Verify they're set
env | grep -E "JWT|CONNECTION|ALLOWED"
```

#### Docker (.env file)
```dockerfile
ASPNETCORE_ENVIRONMENT=Production
JWT_KEY=your_generated_key_here
JWT_ISSUER=yourdomain.com
JWT_AUDIENCE=yourdomain.com
CONNECTION_STRING=your_connection_string_here
CONTROLCENTER_CONNECTION_STRING=your_connection_string_here
ALLOWED_ORIGINS=https://yourdomain.com,https://www.yourdomain.com
```

### Step 2: Create Log Directory
```bash
# Linux/macOS/WSL
mkdir -p ./logs
chmod 755 ./logs

# Windows (PowerShell)
New-Item -ItemType Directory -Path ".\logs" -Force
```

### Step 3: Build & Publish
```bash
# Build for production
dotnet build -c Release

# Publish
dotnet publish -c Release -o ./publish
```

### Step 4: Start Application
```bash
# Windows
cd publish
NG.ControlCenter.WebSite.exe

# Linux/macOS
cd publish
./NG.ControlCenter.WebSite
```

---

## Post-Deployment Verification

### Health Checks
```bash
# Check API health
curl https://yourdomain.com/health

# Expected response:
# {
#   "status": "Healthy",
#   "checks": {
#     "api_health": {
#       "status": "Healthy",
#       "description": "API is running normally"
#     }
#   }
# }
```

### Log Monitoring
```bash
# Watch logs in real-time (Linux/Mac)
tail -f ./logs/app-*.txt

# View latest errors
grep ERROR ./logs/app-*.txt | tail -20
```

### Performance Check
```bash
# Load test with Apache Bench
ab -n 1000 -c 10 https://yourdomain.com/

# Expected: No errors, response time < 200ms
```

### Security Check
```bash
# Test CORS
curl -H "Origin: https://evil.com" \
     -H "Access-Control-Request-Method: POST" \
     -H "Access-Control-Request-Headers: X-Requested-With" \
     -X OPTIONS https://yourdomain.com/api/test

# Expected: 403 Forbidden (not 200)
```

---

## Rollback Plan

### If Issues Occur

1. **Check logs first**
   ```bash
   tail -100 ./logs/app-*.txt
   grep ERROR ./logs/app-*.txt
   ```

2. **Verify configuration**
   ```bash
   # Check environment variables
   env | grep -E "JWT|CONNECTION|ASPNETCORE"
   ```

3. **Check connectivity**
   ```bash
   # Test database
   sqlcmd -S your.server.com -U sa -P yourpassword -d HWPI -Q "SELECT @@VERSION"
   ```

4. **Revert to previous version**
   ```bash
   # Backup current version
   cp -r ./publish ./publish.backup.failed
   
   # Restore previous version
   cp -r ./previous_version/* ./publish/
   
   # Restart application
   systemctl restart microerp
   ```

---

## Ongoing Maintenance

### Daily
- [ ] Check `/health` endpoint responding
- [ ] Review logs for errors
- [ ] Monitor response times

### Weekly
- [ ] Review error logs for patterns
- [ ] Check disk space (log files)
- [ ] Verify backups running

### Monthly
- [ ] Rotate JWT_KEY
- [ ] Review security logs
- [ ] Update NuGet packages
- [ ] Performance analysis

### Quarterly
- [ ] Full security audit
- [ ] Capacity planning
- [ ] Disaster recovery drill

---

## Critical Contacts

| Issue | Contact | Time |
|-------|---------|------|
| Application Down | DevOps Team | ASAP |
| Database Down | DBA | ASAP |
| Security Incident | Security Team | ASAP |
| Performance Issue | Performance Team | 1 hour |
| Bug Report | Development Team | 4 hours |

---

## Important URLs

- **Application**: https://yourdomain.com
- **API**: https://yourdomain.com/swagger
- **Health**: https://yourdomain.com/health
- **Logs Location**: ./logs/app-YYYY-MM-DD.txt
- **Status Dashboard**: (Configure based on your monitoring tool)

---

## Secrets Backup (SECURE THIS!)

Save this information in a secure location (NOT in code):
```
JWT_KEY: [SECURE_LOCATION]
DB_PASSWORD: [SECURE_LOCATION]
API_KEY: [SECURE_LOCATION]
```

**Never share these keys via email or chat!**

---

## Final Verification Checklist

Before going live, verify:

```
SECURITY
☐ No hardcoded credentials
☐ HTTPS enabled
☐ Security headers present
☐ Rate limiting active
☐ CORS restricted
☐ JWT validation working

RELIABILITY
☐ Error handling working
☐ Retry logic active
☐ Circuit breaker responsive
☐ Health checks passing
☐ Database connected
☐ Logs writing to file

PERFORMANCE
☐ Response time < 200ms
☐ Compression enabled
☐ Static caching working
☐ No memory leaks
☐ Load test successful

OPERATIONS
☐ Monitoring active
☐ Alerting configured
☐ Backups scheduled
☐ Logs rotated
☐ Team trained on new features
```

---

## 🚀 Deployment Success Criteria

Application is ready for production when:

✅ All checklist items completed  
✅ Health checks passing  
✅ Logs clean (no errors)  
✅ Performance acceptable  
✅ Security verified  
✅ Monitoring active  
✅ Team trained  
✅ Rollback plan ready  

**Status: READY FOR PRODUCTION** ✨

---

## Support & Documentation

- **IMPLEMENTATION_GUIDE.md** - How to use new features
- **ENV_CONFIG_GUIDE.md** - Environment setup
- **QUICK_REFERENCE.md** - Quick syntax reference
- **CHANGES_SUMMARY.md** - Complete change list
- **BEFORE_AFTER_COMPARISON.md** - Improvement summary
