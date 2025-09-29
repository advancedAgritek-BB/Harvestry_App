# Branch Protection Rules Setup

**Track A Requirement** - GitHub branch protection configuration

## 🎯 Purpose

Configure branch protection rules to enforce code quality, security, and governance standards before merging to `main`.

---

## 🔒 Required Protection Rules

### Main Branch Protection

Navigate to: **GitHub Repo → Settings → Branches → Add rule**

#### **Branch name pattern**: `main`

#### **Required Settings**:

- [x] **Require a pull request before merging**
  - [x] Require approvals: **2**
  - [x] Dismiss stale pull request approvals when new commits are pushed
  - [x] Require review from Code Owners
  - [x] Restrict who can dismiss pull request reviews (admins only)
  
- [x] **Require status checks to pass before merging**
  - [x] Require branches to be up to date before merging
  - **Required checks**:
    - `.NET CI / lint-dotnet`
    - `.NET CI / test-dotnet`
    - `.NET CI / build-dotnet`
    - `Frontend CI / lint-frontend`
    - `Frontend CI / test-frontend`
    - `Frontend CI / build-frontend`
    - `Security - Secret Scan / secrets-scan`
    - `SBOM Generation / generate-sbom`
    - `PR Checks / security`
  
- [x] **Require conversation resolution before merging**
  
- [x] **Require signed commits** (optional but recommended)
  
- [x] **Require linear history** (prevents merge commits)
  
- [x] **Do not allow bypassing the above settings** (enforces rules for everyone)
  
- [x] **Restrict who can push to matching branches**
  - Allowed: Repository administrators only
  
- [x] **Allow force pushes**: **Disabled**
  
- [x] **Allow deletions**: **Disabled**

---

## 🚀 Feature Branch Protection

### Feature Branch Pattern: `feature/*`

#### **Required Settings**:

- [x] **Require a pull request before merging**
  - [x] Require approvals: **1**
  
- [x] **Require status checks to pass before merging**
  - Same checks as main branch
  
- [x] **Allow force pushes**: **Enabled** (for feature branch cleanup)

---

## 🔥 Hotfix Branch Protection

### Hotfix Branch Pattern: `hotfix/*`

#### **Required Settings**:

- [x] **Require a pull request before merging**
  - [x] Require approvals: **1** (expedited review for emergencies)
  
- [x] **Require status checks to pass before merging**
  - Required checks (reduced for speed):
    - `.NET CI / test-dotnet`
    - `Frontend CI / test-frontend`
    - `Security - Secret Scan / secrets-scan`

---

## 👥 CODEOWNERS Integration

Branch protection automatically enforces CODEOWNERS review requirements. Ensure `/CODEOWNERS` file is configured:

```
# Core Platform
/src/backend/services/core-platform/ @Core-Platform-Squad
/src/backend/services/auth/ @Core-Platform-Squad

# Telemetry & Controls
/src/backend/services/telemetry/ @Telemetry-Controls-Squad
/src/backend/services/controls/ @Telemetry-Controls-Squad

# Workflow & Messaging
/src/backend/services/workflow/ @Workflow-Messaging-Squad
/src/backend/services/messaging/ @Workflow-Messaging-Squad

# Integrations
/src/backend/services/integrations/ @Integrations-Squad

# Data & AI
/src/backend/services/analytics/ @Data-AI-Squad
/src/backend/services/ml/ @Data-AI-Squad

# Frontend
/src/frontend/ @Frontend-Squad

# Database
/src/database/ @Data-Engineering-Squad

# Infrastructure
/src/infrastructure/ @DevOps-Squad
/.github/ @DevOps-Squad
```

---

## 🧪 Testing Branch Protection

### Verify Rules Work

1. **Create test feature branch**:
   ```bash
   git checkout -b feature/test-branch-protection
   echo "test" >> README.md
   git add README.md
   git commit -m "test: verify branch protection"
   git push origin feature/test-branch-protection
   ```

2. **Create PR** (should trigger all checks)

3. **Attempt to merge** without approvals (should be blocked)

4. **Attempt to push directly to main**:
   ```bash
   git checkout main
   git push origin main
   # Expected: Error - protected branch
   ```

---

## 🔐 Signed Commits Setup

### For Developers (Optional but Recommended)

1. **Generate GPG key**:
   ```bash
   gpg --full-generate-key
   # Choose RSA and RSA, 4096 bits
   ```

2. **List GPG keys**:
   ```bash
   gpg --list-secret-keys --keyid-format LONG
   ```

3. **Export public key**:
   ```bash
   gpg --armor --export YOUR_KEY_ID
   ```

4. **Add to GitHub**:
   - Settings → SSH and GPG keys → New GPG key

5. **Configure Git**:
   ```bash
   git config --global user.signingkey YOUR_KEY_ID
   git config --global commit.gpgsign true
   ```

---

## 📊 Branch Protection Metrics

Monitor branch protection effectiveness:

### Key Metrics
- **PR merge time** (target: < 24 hours for feature branches)
- **CI check pass rate** (target: > 95% on first run)
- **Number of approvals per PR** (target: 2+)
- **Merge conflicts** (target: < 5% of PRs)

### Dashboard Query
```sql
SELECT 
    DATE_TRUNC('week', merged_at) as week,
    COUNT(*) as total_prs,
    AVG(EXTRACT(EPOCH FROM (merged_at - created_at)) / 3600) as avg_hours_to_merge,
    SUM(CASE WHEN approvals >= 2 THEN 1 ELSE 0 END) as prs_with_2_approvals
FROM pull_requests
WHERE base_branch = 'main'
  AND merged_at IS NOT NULL
GROUP BY week
ORDER BY week DESC;
```

---

## 🚨 Emergency Bypass Procedure

### When to Bypass (Rare)
- Production outage (P0 incident)
- Security vulnerability hotfix
- Data loss prevention

### Process
1. **Create incident ticket** documenting reason
2. **Notify VP Engineering** via Slack #incidents
3. **Temporarily disable protection** (admin only)
4. **Merge hotfix**
5. **Re-enable protection immediately**
6. **Post-mortem required** within 24 hours

---

## ✅ Verification Checklist

Before marking Track A complete:

- [ ] Main branch protection configured with 2 approvals
- [ ] All CI checks marked as required
- [ ] CODEOWNERS file enforced
- [ ] Force push disabled
- [ ] Branch deletion disabled
- [ ] Signed commits enabled (optional)
- [ ] Feature branch pattern configured
- [ ] Hotfix branch pattern configured
- [ ] Test PR created and verified rules work
- [ ] Team trained on signed commits (if enabled)

---

## 📚 Related Documentation

- [CODEOWNERS](../../CODEOWNERS)
- [Pull Request Template](../../.github/PULL_REQUEST_TEMPLATE.md)
- [Definition of Ready](../governance/DEFINITION_OF_READY.md)
- [Definition of Done](../governance/DEFINITION_OF_DONE.md)

---

**✅ Track A Objective:** Enforce code quality and security through automated branch protection.
