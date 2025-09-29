# Pull Request

## Changes
<!-- Describe what changed and why -->

### Summary
<!-- Brief description of the changes -->

### Type of Change

- [ ] 🐛 Bug fix (non-breaking change that fixes an issue)
- [ ] ✨ New feature (non-breaking change that adds functionality)
- [ ] 💥 Breaking change (fix or feature that would cause existing functionality to change)
- [ ] 📚 Documentation update
- [ ] 🔧 Configuration change
- [ ] ♻️ Refactoring (no functional changes)
- [ ] 🎨 UI/UX improvements
- [ ] ⚡ Performance improvements
- [ ] 🔐 Security improvements

## Testing
<!-- Describe the testing you performed -->

### Test Coverage

- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] E2E tests added/updated (if applicable)
- [ ] Manual testing performed

### Test Results
<!-- Paste relevant test output or screenshots -->

## Related Issues
<!-- Link to related issues using #issue-number -->

Closes #
Related to #

## Database Changes
<!-- Check all that apply -->

- [ ] No database changes
- [ ] Migration files added
- [ ] RLS policies updated
- [ ] Indexes added/modified
- [ ] Seeds updated

### Migration Notes
<!-- If migrations are included, describe them and any rollback considerations -->

## Feature Flags
<!-- Check all that apply -->

- [ ] No feature flags
- [ ] New feature flag added
- [ ] Existing feature flag modified
- [ ] Feature flag enablement (requires PDD + Runbook if prod)

### Feature Flag Details
<!-- If modifying feature flags, provide details -->

**Flag Name:**  
**Scope:** (site-scoped / global)  
**Risk Level:** (low / medium / high)  

**For high-risk flags (closed-loop, ai auto-apply, etc.):**

- [ ] PDD linked: <!-- URL -->
- [ ] Runbook linked: <!-- URL -->
- [ ] Promotion checklist followed
- [ ] Shadow mode tested (if applicable)

## Observability
<!-- Check all that apply -->

- [ ] Metrics/traces/logs added
- [ ] Dashboards updated
- [ ] Alerts configured
- [ ] Runbook updated
- [ ] N/A - No observability changes needed

## Security Checklist
<!-- All items must be checked before merge -->

- [ ] No secrets or credentials committed
- [ ] RLS policies reviewed (if database changes)
- [ ] ABAC gates added for high-risk operations (if applicable)
- [ ] Input validation added
- [ ] Audit trail captures changes (if applicable)
- [ ] Dependencies scanned for vulnerabilities

## Code Quality Checklist
<!-- All items must be checked before merge -->

- [ ] No files exceed 500 lines
- [ ] Functions are under 40 lines
- [ ] Single responsibility principle followed
- [ ] Code is DRY (Don't Repeat Yourself)
- [ ] Descriptive naming conventions used
- [ ] Comments added for complex logic
- [ ] No linter warnings

## Documentation
<!-- Check all that apply -->

- [ ] README updated (if applicable)
- [ ] API documentation updated (if applicable)
- [ ] User guide updated (if applicable)
- [ ] Runbook created/updated (if operational changes)
- [ ] ADR created (if architectural change)
- [ ] No documentation changes needed

## Breaking Changes
<!-- If this is a breaking change, describe the impact and migration path -->

### Impact
<!-- Who/what is affected? -->

### Migration Path
<!-- How should users adapt to this change? -->

## Deployment Notes
<!-- Any special deployment considerations -->

- [ ] No special deployment steps
- [ ] Requires configuration changes (specify below)
- [ ] Requires data migration
- [ ] Requires service restart
- [ ] Requires zero-downtime deployment strategy

### Deployment Steps
<!-- If special steps are needed, list them here -->

## Rollback Plan
<!-- Describe how to rollback if issues arise -->

## Screenshots
<!-- If UI changes, include before/after screenshots -->

## Reviewer Notes
<!-- Anything specific you want reviewers to focus on? -->

## Squad Sign-off
<!-- Tag the relevant squad leads for review -->

**Primary Squad:** @harvestry/[squad-name]  
**Reviewers:**

- [ ] Reviewed by squad lead
- [ ] Reviewed by peer engineer
- [ ] Security review (if applicable)
- [ ] SRE review (if infrastructure changes)

---

**By submitting this PR, I confirm:**

- [ ] I have tested these changes locally
- [ ] I have followed the coding standards
- [ ] I have updated relevant documentation
- [ ] I have added appropriate tests
- [ ] This PR is ready for review
