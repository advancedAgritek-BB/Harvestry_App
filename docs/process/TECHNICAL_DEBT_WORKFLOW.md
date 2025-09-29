# Technical Debt Management Workflow

**Track A Governance** - Process for managing technical debt

## 🎯 Purpose

Define the process for identifying, tracking, prioritizing, and resolving technical debt in a systematic way.

---

## 📋 Definitions

### What IS Technical Debt?

✅ **Technical Debt** includes:
- Code that works but is hard to maintain
- Architecture that doesn't scale well
- Missing tests for critical functionality
- Incomplete implementations that need revisiting
- Suboptimal solutions chosen for speed
- Infrastructure shortcuts that need proper implementation

### What is NOT Technical Debt?

❌ **Not Technical Debt**:
- New feature requests (→ Product Backlog)
- Bugs (→ Bug Tracker)
- Performance optimizations (→ Performance Backlog)
- Security vulnerabilities (→ Immediate Fix)
- User-facing issues (→ Support Tickets)

---

## 🔄 Debt Lifecycle

```
┌─────────────┐
│  Identify   │ ← During development, code review, retrospectives
└──────┬──────┘
       │
       ▼
┌─────────────┐
│   Document  │ ← Add to TECHNICAL_DEBT.md with full context
└──────┬──────┘
       │
       ▼
┌─────────────┐
│ Prioritize  │ ← Assign High/Medium/Low based on impact
└──────┬──────┘
       │
       ▼
┌─────────────┐
│  Schedule   │ ← Add to sprint backlog when capacity allows
└──────┬──────┘
       │
       ▼
┌─────────────┐
│   Resolve   │ ← Implement fix, test, deploy
└──────┬──────┘
       │
       ▼
┌─────────────┐
│   Archive   │ ← Move to completed section in TECHNICAL_DEBT.md
└─────────────┘
```

---

## 1️⃣ Identifying Technical Debt

### When to Identify

Debt can be identified during:
- **Development**: "We're taking a shortcut here"
- **Code Review**: "This could be better, but let's ship it"
- **Retrospectives**: "We should refactor this"
- **Incident Post-Mortems**: "This caused the outage"
- **Onboarding**: New team members spot issues

### Red Flags

Look for these signs:
- [ ] Repeated "TODO" or "FIXME" comments
- [ ] Copy-pasted code (DRY violation)
- [ ] Long functions (>50 lines)
- [ ] God classes (>500 lines)
- [ ] Missing error handling
- [ ] Hard-coded values
- [ ] Test coverage < 70%
- [ ] High cyclomatic complexity (>10)
- [ ] Comments saying "temporary" or "hack"

---

## 2️⃣ Documenting Debt

### Step 1: Add to TECHNICAL_DEBT.md

Use the template in `TECHNICAL_DEBT.md`:

```markdown
### DEBT-XXX: [Short Description]
**Category**: Testing  
**Created**: 2025-09-29  
**Squad**: Core-Platform-Squad  
**Effort**: 3 days  

**Context**: ...
**Why Deferred**: ...
**Impact if Not Fixed**: ...
**Action Required**: ...
**Acceptance Criteria**: ...
```

### Step 2: Add Code Comments

Mark the code with a reference:

```csharp
// DEBT-005: Missing input validation
// See: TECHNICAL_DEBT.md#debt-005
public async Task<Result> ProcessOrder(Order order)
{
    // TODO: Add validation before processing
    await _orderService.ProcessAsync(order);
}
```

### Step 3: Create GitHub Issue (Optional)

For high-priority debt, create an issue using the template:
`.github/ISSUE_TEMPLATE/technical-debt.md`

---

## 3️⃣ Prioritizing Debt

### Priority Matrix

| Priority | When to Address | Criteria |
|----------|-----------------|----------|
| 🔴 **High** | Next sprint | Blocks future work, security risk, or causes incidents |
| 🟡 **Medium** | Within 2-3 sprints | Slows development, increases bug rate |
| 🟢 **Low** | Opportunistic | Minor inconvenience, cosmetic |

### Priority Criteria

**🔴 High Priority** if debt causes:
- Production incidents
- Security vulnerabilities
- Complete inability to add features
- Team velocity < 50% of normal
- Customer-facing impact

**🟡 Medium Priority** if debt causes:
- Frequent bugs in the area
- Slow development (2x effort)
- High onboarding friction
- Code review slowdowns

**🟢 Low Priority** if debt causes:
- Minor code smell
- Slight inconvenience
- Aesthetic issues

---

## 4️⃣ Scheduling Debt Work

### Sprint Planning Integration

#### Option A: Debt Days
- Reserve 20% of each sprint for debt
- Example: 10-day sprint = 2 days debt work

#### Option B: Debt Sprints
- Every 4th sprint is 50% debt
- Example: Sprint 4, 8, 12 focus on debt

#### Option C: Opportunistic
- Fix debt when working nearby
- "Boy Scout Rule": Leave code better than you found it

### Debt Budget

Set a budget for debt work:
- **Minimum**: 10% of sprint capacity
- **Target**: 20% of sprint capacity
- **Maximum**: 30% of sprint capacity (if debt is high)

**Alert**: If debt effort exceeds 2 sprints, escalate to Engineering Manager.

---

## 5️⃣ Resolving Debt

### Before Starting

- [ ] Review debt item in `TECHNICAL_DEBT.md`
- [ ] Ensure all context is understood
- [ ] Estimate effort (re-estimate if needed)
- [ ] Assign to developer

### During Implementation

1. **Create branch**: `debt/DEBT-XXX-short-description`
2. **Write tests first** (if adding test coverage)
3. **Implement fix**
4. **Update documentation**
5. **Remove code comments** referencing debt
6. **Run linters and tests**

### Code Review Checklist

- [ ] All acceptance criteria met
- [ ] Tests added/updated
- [ ] No new debt introduced
- [ ] Documentation updated
- [ ] TECHNICAL_DEBT.md updated (move to archive)

### After Merging

1. Close related GitHub issue (if exists)
2. Move debt item to "Completed Debt" section in `TECHNICAL_DEBT.md`
3. Update debt metrics
4. Celebrate! 🎉

---

## 6️⃣ Reviewing Debt

### Weekly Review (Squad Lead)

- Review new debt items
- Reprioritize based on current work
- Flag high-priority debt for sprint planning

### Sprint Review (Entire Squad)

During retrospective:
- How much debt did we create?
- How much debt did we resolve?
- Is debt increasing or decreasing?
- Do we need a debt sprint?

### Quarterly Review (Engineering Manager)

- Total debt effort vs. team capacity
- Debt trend (increasing/decreasing)
- High-priority debt aging
- Systemic issues causing debt

---

## 📊 Debt Metrics

Track these metrics in `TECHNICAL_DEBT.md`:

### Primary Metrics
- **Total Debt Count**: Number of items
- **Total Debt Effort**: Sum of estimated days
- **High-Priority Debt Age**: Days since creation

### Secondary Metrics
- **Debt Ratio**: Debt effort / Sprint capacity
- **Debt Resolution Rate**: Items resolved per sprint
- **Debt Introduction Rate**: Items added per sprint

### Target Metrics
- Total debt effort < 2 sprint capacity
- High-priority debt age < 30 days
- Resolution rate ≥ Introduction rate

---

## 🚨 Escalation

### When to Escalate

Escalate to Engineering Manager if:
- High-priority debt > 30 days old
- Total debt effort > 2 sprints
- Debt causing production incidents
- Debt blocking critical features

### Escalation Process

1. **Document Impact**: Write brief on business impact
2. **Propose Solution**: Include effort estimate
3. **Request Decision**: Debt sprint? External help?
4. **Follow Up**: Update stakeholders on resolution

---

## ✅ Best Practices

### Do's ✅

- ✅ Document debt immediately when created
- ✅ Include full context (why, impact, action)
- ✅ Link debt to related code with comments
- ✅ Review debt regularly
- ✅ Celebrate debt resolution
- ✅ Use "Boy Scout Rule" opportunistically

### Don'ts ❌

- ❌ Let high-priority debt age > 30 days
- ❌ Hide debt ("we'll remember to fix it")
- ❌ Accumulate debt without tracking
- ❌ Ignore debt in code reviews
- ❌ Skip debt work indefinitely
- ❌ Create debt without documenting why

---

## 📚 Related Documentation

- [Technical Debt Register](../../TECHNICAL_DEBT.md)
- [Definition of Ready](../governance/DEFINITION_OF_READY.md)
- [Definition of Done](../governance/DEFINITION_OF_DONE.md)
- [Code Review Guidelines](../development/CODE_REVIEW.md)

---

**Last Updated**: 2025-09-29  
**Owner**: Engineering Manager  
**Review Frequency**: Quarterly
