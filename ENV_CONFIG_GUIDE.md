# NG.MicroERP - Environment Configuration Guide

## Overview
This guide explains how to configure the application using environment variables for security and flexibility.

## Critical Security Credentials (Use Environment Variables ONLY)

### JWT Configuration
```bash
# Generate a secure key: openssl rand -base64 32
JWT_KEY=YOUR_SECURE_KEY_HERE
JWT_ISSUER=aamirrashid.com
JWT_AUDIENCE=aamirrashid.com
```

### Database Connection (Store Securely)
```bash
# For SQL Server
CONNECTION_STRING=Password=YourPassword;Persist Security Info=False;User ID=sa;Initial Catalog=HWPI;Data Source=server.com;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
CONTROLCENTER_CONNECTION_STRING=Password=YourPassword;Persist Security Info=False;User ID=sa;Initial Catalog=ControlCenter;Data Source=server.com;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

### CORS Configuration
```bash
# Allowed origins for CORS (comma-separated)
ALLOWED_ORIGINS=https://yourdomain.com,https://www.yourdomain.com,https://api.yourdomain.com
```

## Setting Environment Variables

### Windows (PowerShell)
```powershell
$env:JWT_KEY = "your_key_here"
$env:JWT_ISSUER = "aamirrashid.com"
$env:JWT_AUDIENCE = "aamirrashid.com"
$env:CONNECTION_STRING = "..."
```

### Windows (CMD)
```cmd
setx JWT_KEY "your_key_here"
setx JWT_ISSUER "aamirrashid.com"
setx JWT_AUDIENCE "aamirrashid.com"
setx CONNECTION_STRING "..."
```

### Linux/macOS
```bash
export JWT_KEY="your_key_here"
export JWT_ISSUER="aamirrashid.com"
export JWT_AUDIENCE="aamirrashid.com"
export CONNECTION_STRING="..."
```

### Docker (.env file)
```dockerfile
JWT_KEY=your_key_here
JWT_ISSUER=aamirrashid.com
JWT_AUDIENCE=aamirrashid.com
CONNECTION_STRING=...
ALLOWED_ORIGINS=https://yourdomain.com
```

## ASP.NET Core User Secrets (Development)
```bash
# Initialize user secrets
dotnet user-secrets init

# Set secrets
dotnet user-secrets set "Jwt:Key" "your_key_here"
dotnet user-secrets set "Jwt:Issuer" "aamirrashid.com"
dotnet user-secrets set "Jwt:Audience" "aamirrashid.com"
dotnet user-secrets set "ConnectionStrings:Default" "..."

# List secrets
dotnet user-secrets list
```

## Azure Key Vault Configuration

### For Production:
```csharp
// In Program.cs
if (!app.Environment.IsDevelopment())
{
    var keyVaultUrl = new Uri(builder.Configuration["KeyVault:Url"]);
    var credential = new DefaultAzureCredential();
    builder.Configuration.AddAzureKeyVault(keyVaultUrl, credential);
}
```

## Configuration Priority (Highest to Lowest)
1. Environment Variables
2. appsettings.{ENVIRONMENT}.json
3. appsettings.json
4. User Secrets (Development only)
5. Default values in code

## Security Checklist
- [ ] Never commit sensitive credentials to version control
- [ ] Use .gitignore to exclude appsettings.json files
- [ ] Rotate JWT keys regularly
- [ ] Use strong, complex passwords
- [ ] Enable HTTPS in production
- [ ] Validate and sanitize all user inputs
- [ ] Use parameterized queries for database access
- [ ] Monitor logs for suspicious activity

## Health Check Endpoints
- `/health` - General health status
- `/health/ready` - Readiness probe for Kubernetes
