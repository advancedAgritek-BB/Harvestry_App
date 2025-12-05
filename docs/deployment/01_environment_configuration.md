# Environment Configuration Guide

**Document Version:** 1.0  
**Date:** December 4, 2025  
**Status:** Production Ready

---

## 1. Overview

This document describes the environment configuration for deploying Harvestry in different environments (development, staging, production).

---

## 2. Environment Variables

### 2.1 Frontend (Next.js)

| Variable | Description | Development | Production |
|----------|-------------|-------------|------------|
| `NEXT_PUBLIC_SUPABASE_URL` | Supabase project URL | Your project URL | Your project URL |
| `NEXT_PUBLIC_SUPABASE_ANON_KEY` | Supabase anon/public key | Your anon key | Your anon key |
| `NEXT_PUBLIC_API_URL` | Backend API base URL | `http://localhost:5000` | `https://api.harvestry.io` |
| `NEXT_PUBLIC_USE_MOCK_AUTH` | Enable mock auth | `true` | `false` (NEVER true) |

**Example `.env.local` (Development):**
```env
NEXT_PUBLIC_SUPABASE_URL=https://your-project.supabase.co
NEXT_PUBLIC_SUPABASE_ANON_KEY=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
NEXT_PUBLIC_API_URL=http://localhost:5000
NEXT_PUBLIC_USE_MOCK_AUTH=true
```

**Example Production Variables (AWS Amplify):**
```env
NEXT_PUBLIC_SUPABASE_URL=https://your-project.supabase.co
NEXT_PUBLIC_SUPABASE_ANON_KEY=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
NEXT_PUBLIC_API_URL=https://api.harvestry.io
NEXT_PUBLIC_USE_MOCK_AUTH=false
```

### 2.2 Backend (.NET Services)

| Variable | Description | Source |
|----------|-------------|--------|
| `Supabase__Url` | Supabase project URL | AWS Secrets Manager |
| `Supabase__JwtSecret` | JWT secret for validation | AWS Secrets Manager |
| `Supabase__WebhookSecret` | Webhook signature secret | AWS Secrets Manager |
| `ConnectionStrings__Identity` | Identity DB connection | AWS Secrets Manager |
| `ConnectionStrings__Genetics` | Genetics DB connection | AWS Secrets Manager |
| `ConnectionStrings__Spatial` | Spatial DB connection | AWS Secrets Manager |

**Example `appsettings.Production.json`:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Security": {
    "CORS": {
      "AllowedOrigins": ["https://app.harvestry.io"],
      "AllowCredentials": true
    }
  },
  "OpenTelemetry": {
    "Endpoint": "https://otel-collector.harvestry.io:4317",
    "EnableConsoleExporter": false
  }
}
```

---

## 3. AWS Secrets Manager Configuration

### 3.1 Secret Structure

Create the following secrets in AWS Secrets Manager:

```
harvestry/production/supabase
├── url
├── jwt_secret
├── webhook_secret
└── anon_key

harvestry/production/database
├── host
├── port
├── username
├── password
└── database

harvestry/production/slack
├── client_id
└── client_secret
```

### 3.2 Secret Access IAM Policy

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "secretsmanager:GetSecretValue"
      ],
      "Resource": [
        "arn:aws:secretsmanager:us-east-1:*:secret:harvestry/production/*"
      ]
    }
  ]
}
```

---

## 4. Database Configuration

### 4.1 AWS RDS PostgreSQL Settings

```yaml
Engine: PostgreSQL 15.x
Instance Class: db.r6g.xlarge (production)
Storage: 500 GB gp3
Multi-AZ: true
Backup Retention: 30 days
Encryption: AES-256
Performance Insights: Enabled
```

### 4.2 Connection String Format

```
Host={rds-endpoint};
Port=5432;
Database=harvestry;
Username={username};
Password={password};
SSL Mode=Require;
Trust Server Certificate=false;
```

### 4.3 TimescaleDB Extension

Ensure TimescaleDB is enabled:
```sql
CREATE EXTENSION IF NOT EXISTS timescaledb;
```

---

## 5. Supabase Configuration

### 5.1 Auth Settings

Configure in Supabase Dashboard > Authentication:

1. **Site URL:** `https://app.harvestry.io`
2. **Redirect URLs:**
   - `https://app.harvestry.io/reset-password`
   - `https://app.harvestry.io/callback`
3. **JWT Expiry:** 3600 seconds (1 hour)
4. **Refresh Token Rotation:** Enabled
5. **Email Confirmations:** Required

### 5.2 Webhook Configuration

Configure auth webhooks in Supabase:

1. **URL:** `https://api.harvestry.io/api/v1/webhooks/supabase/user`
2. **Events:** `user.created`, `user.updated`, `user.deleted`
3. **Secret:** Generate and store in AWS Secrets Manager

---

## 6. Environment Checklist

### 6.1 Development

- [ ] Local PostgreSQL running
- [ ] `.env.local` configured
- [ ] `NEXT_PUBLIC_USE_MOCK_AUTH=true` for local testing
- [ ] Supabase project created (or use mock auth)

### 6.2 Staging

- [ ] AWS RDS instance provisioned
- [ ] Supabase project configured
- [ ] Secrets stored in AWS Secrets Manager
- [ ] CORS configured for staging URLs
- [ ] SSL certificates provisioned

### 6.3 Production

- [ ] Multi-AZ RDS deployment
- [ ] Production Supabase configuration
- [ ] All secrets rotated from staging
- [ ] WAF rules configured
- [ ] CloudWatch alarms set up
- [ ] Backup verification completed
- [ ] Disaster recovery tested

---

## 7. Security Notes

1. **Never commit secrets** - Use environment variables or secrets manager
2. **Rotate secrets regularly** - At least every 90 days
3. **Enable audit logging** - CloudTrail for AWS, audit logs for Supabase
4. **Use least privilege** - IAM roles should have minimal permissions
5. **Encrypt in transit** - TLS 1.3 for all connections
6. **Encrypt at rest** - AES-256 for database storage

---

*End of Environment Configuration Guide*



