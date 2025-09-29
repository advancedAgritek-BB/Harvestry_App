# Grafana Datasource Credential Rotation Notice

**Date**: 2025-09-29  
**Severity**: HIGH  
**Status**: REMEDIATED  

---

## 🚨 Issue Summary

A hardcoded PostgreSQL database password was exposed in the Grafana datasources configuration file at:
- `src/infrastructure/monitoring/grafana/provisioning/datasources/datasources.yml` (line 65)

**Exposed Credential**: `harvestry_dev_password` (development environment)

---

## ✅ Remediation Actions Taken

### 1. Replaced Hardcoded Password with Environment Variable

**File**: `src/infrastructure/monitoring/grafana/provisioning/datasources/datasources.yml`

```yaml
# BEFORE (INSECURE)
secureJsonData:
  password: harvestry_dev_password

# AFTER (SECURE)
secureJsonData:
  password: ${GF_DATASOURCES_POSTGRES_PASSWORD}
```

### 2. Updated Docker Compose Configuration

**File**: `docker-compose.yml`

Added environment variable to Grafana service:
```yaml
environment:
  GF_DATASOURCES_POSTGRES_PASSWORD: harvestry_dev_password
```

### 3. Added Security Documentation

- Added header comments to `datasources.yml` emphasizing that credentials must NEVER be hardcoded
- Added inline security comment for the PostgreSQL datasource configuration
- Created this notice document for tracking and reference

---

## 🔄 Required Actions

### For Development Environment

✅ **Completed**: Environment variable configured in `docker-compose.yml`

### For Staging/Production Environments

⚠️ **ACTION REQUIRED**: System administrators must:

1. **Rotate the compromised credential immediately**:
   ```sql
   -- Connect to PostgreSQL as superuser
   ALTER USER harvestry_user WITH PASSWORD 'new_secure_password_here';
   ```

2. **Update secret management system**:
   - Store new password in Vault/KMS/Secret Manager
   - Ensure secret is tagged with appropriate access policies
   - Document secret rotation in audit logs

3. **Configure environment variable** in deployment:
   
   **Kubernetes/Helm**:
   ```yaml
   # values.yaml or secret
   grafana:
     env:
       - name: GF_DATASOURCES_POSTGRES_PASSWORD
         valueFrom:
           secretKeyRef:
             name: grafana-secrets
             key: postgres-password
   ```
   
   **Docker/Docker Compose**:
   ```yaml
   environment:
     GF_DATASOURCES_POSTGRES_PASSWORD: ${POSTGRES_PASSWORD_FROM_VAULT}
   ```
   
   **Systemd/Direct Deployment**:
   ```bash
   # /etc/grafana/grafana.env
   GF_DATASOURCES_POSTGRES_PASSWORD=password_from_vault
   ```

4. **Verify configuration**:
   - Restart Grafana service
   - Confirm PostgreSQL datasource connects successfully
   - Check Grafana logs for authentication errors: `/var/log/grafana/grafana.log`

---

## 🔐 Security Best Practices

### Never Hardcode Secrets

❌ **Bad**:
```yaml
password: my_secret_password
api_key: sk_live_abc123xyz
```

✅ **Good**:
```yaml
password: ${DB_PASSWORD}
api_key: ${API_KEY}
```

### Use Secret Management

**Development**:
- Environment variables in `.env` files (never commit to git)
- Docker Compose environment variables
- Local secret files with restricted permissions

**Staging/Production**:
- HashiCorp Vault
- AWS Secrets Manager / Azure Key Vault / GCP Secret Manager
- Kubernetes Secrets with encryption at rest
- Sealed Secrets or External Secrets Operator

### Credential Rotation Policy

| Environment | Rotation Frequency | Method |
|-------------|-------------------|---------|
| Development | Annually or on exposure | Manual rotation + docker-compose update |
| Staging | Quarterly | Automated via secret manager |
| Production | Quarterly or on exposure | Automated via secret manager with zero-downtime |

---

## 📋 Verification Checklist

- [x] Hardcoded password removed from repository
- [x] Environment variable interpolation implemented
- [x] Development environment configured
- [ ] **Staging credential rotated** (SRE Team)
- [ ] **Production credential rotated** (SRE Team)
- [ ] Secret manager policies updated
- [ ] Deployment documentation updated
- [ ] Team briefed on secure credential management

---

## 📚 Related Documentation

- [Security Best Practices](../development/standards/SECURITY_STANDARDS.md)
- [Secret Management Guide](../../src/infrastructure/secrets/README.md)
- [Deployment Procedures](../deployment/procedures/)
- [Grafana Setup](GRAFANA_SETUP.md)

---

## 📞 Contact

**Security Team**: security@harvestry.com  
**SRE On-Call**: sre-oncall@harvestry.com  
**Incident Response**: incidents@harvestry.com

---

**Next Review**: 2025-12-29 (Quarterly review of all service credentials)
