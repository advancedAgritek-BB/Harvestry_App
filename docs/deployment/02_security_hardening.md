# Security Hardening Guide

**Document Version:** 1.0  
**Date:** December 4, 2025  
**Status:** Production Ready

---

## 1. Authentication Security

### 1.1 JWT Configuration

**Supabase JWT Settings:**
```
Algorithm: HS256
Expiry: 3600 seconds (1 hour)
Refresh Token Rotation: Enabled
Refresh Token Reuse Detection: Enabled
```

**Backend JWT Validation:**
```csharp
options.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ClockSkew = TimeSpan.FromSeconds(30),
    RequireExpirationTime = true,
    RequireSignedTokens = true
};
```

### 1.2 Password Requirements

Configure in Supabase:
- Minimum length: 8 characters
- Require: Uppercase, lowercase, number
- Enable: Password breach detection

### 1.3 Session Management

- Access token: 1 hour expiry
- Refresh token: 7 days expiry
- Automatic token refresh on API calls
- Sign out invalidates all sessions

---

## 2. API Security

### 2.1 Rate Limiting

**Configuration:**
```csharp
services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });
    
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
    });
});
```

### 2.2 CORS Configuration

**Production:**
```csharp
services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://app.harvestry.io")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromHours(1));
    });
});
```

### 2.3 Request Validation

- All input validated with FluentValidation
- SQL injection prevention via parameterized queries
- XSS prevention via output encoding
- CSRF protection via SameSite cookies

---

## 3. Database Security

### 3.1 Row-Level Security (RLS)

All tables must have RLS enabled:
```sql
ALTER TABLE tablename ENABLE ROW LEVEL SECURITY;
ALTER TABLE tablename FORCE ROW LEVEL SECURITY;
```

**Standard RLS Policy Pattern:**
```sql
CREATE POLICY "site_isolation" ON tablename
    FOR ALL
    USING (site_id::TEXT = current_setting('app.current_site_id', true))
    WITH CHECK (site_id::TEXT = current_setting('app.current_site_id', true));
```

### 3.2 Connection Security

```yaml
SSL Mode: Require
Minimum TLS: 1.2
Certificate Validation: Enabled
Connection Encryption: AES-256-GCM
```

### 3.3 Database User Permissions

```sql
-- Application user (limited)
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO app_user;
GRANT USAGE ON ALL SEQUENCES IN SCHEMA public TO app_user;
REVOKE CREATE ON SCHEMA public FROM app_user;

-- Migration user (for schema changes only)
GRANT ALL ON SCHEMA public TO migration_user;
```

---

## 4. Network Security

### 4.1 AWS VPC Configuration

```yaml
VPC CIDR: 10.0.0.0/16

Private Subnets (App/DB):
  - 10.0.1.0/24 (us-east-1a)
  - 10.0.2.0/24 (us-east-1b)
  - 10.0.3.0/24 (us-east-1c)

Public Subnets (ALB):
  - 10.0.101.0/24 (us-east-1a)
  - 10.0.102.0/24 (us-east-1b)
  - 10.0.103.0/24 (us-east-1c)
```

### 4.2 Security Groups

**Application Load Balancer:**
```yaml
Inbound:
  - HTTPS (443) from 0.0.0.0/0
  - HTTP (80) from 0.0.0.0/0 (redirect to HTTPS)
```

**ECS Service:**
```yaml
Inbound:
  - App port (5000) from ALB security group only
Outbound:
  - HTTPS (443) to 0.0.0.0/0 (for Supabase)
  - PostgreSQL (5432) to RDS security group
```

**RDS:**
```yaml
Inbound:
  - PostgreSQL (5432) from ECS security group only
Outbound:
  - None required
```

### 4.3 WAF Rules

Configure AWS WAF with:
- AWS Managed Rules - Common Rule Set
- AWS Managed Rules - SQL Injection
- AWS Managed Rules - Known Bad Inputs
- Rate-based rule: 2000 requests per 5 minutes per IP

---

## 5. Data Protection

### 5.1 Encryption at Rest

| Component | Encryption |
|-----------|------------|
| RDS | AES-256 (AWS KMS) |
| S3 Buckets | SSE-S3 or SSE-KMS |
| Secrets Manager | AWS KMS |
| EBS Volumes | AES-256 |

### 5.2 Encryption in Transit

- TLS 1.3 for all external connections
- TLS 1.2 minimum for internal connections
- Certificate management via AWS Certificate Manager

### 5.3 PII Handling

Personal Identifiable Information (PII):
- Email addresses: Stored encrypted
- Phone numbers: Stored encrypted
- Names: Stored encrypted
- Audit logs: Anonymized after 90 days

---

## 6. Monitoring & Alerts

### 6.1 Security Monitoring

**CloudWatch Alarms:**
- Failed authentication attempts > 10/minute
- 4xx errors > 100/minute
- 5xx errors > 10/minute
- Database connections > 80% capacity

**AWS GuardDuty:**
- Enable for all accounts
- Configure SNS notifications for HIGH severity

### 6.2 Audit Logging

**Enable:**
- CloudTrail for all AWS API calls
- RDS audit logging (pgAudit)
- Application audit trail (database table)
- Supabase auth audit logs

### 6.3 Log Retention

| Log Type | Retention |
|----------|-----------|
| Application logs | 30 days |
| Security logs | 1 year |
| Audit trail | 7 years |
| Access logs | 90 days |

---

## 7. Incident Response

### 7.1 Security Incident Contacts

Configure in AWS Security Hub:
- Primary: security@harvestry.io
- Secondary: oncall@harvestry.io
- Escalation: cto@harvestry.io

### 7.2 Response Procedures

1. **Detection:** CloudWatch/GuardDuty alert
2. **Triage:** Assess severity (P1-P4)
3. **Containment:** Isolate affected resources
4. **Eradication:** Remove threat
5. **Recovery:** Restore services
6. **Lessons Learned:** Post-incident review

---

## 8. Compliance Checklist

### 8.1 Pre-Production

- [ ] All RLS policies verified
- [ ] Secrets rotated from development
- [ ] SSL certificates valid
- [ ] WAF rules active
- [ ] Security groups locked down
- [ ] Logging enabled
- [ ] Backup tested
- [ ] Penetration test completed

### 8.2 Regular Reviews

- [ ] Quarterly access reviews
- [ ] Monthly vulnerability scans
- [ ] Annual penetration test
- [ ] Continuous dependency updates

---

*End of Security Hardening Guide*








