# FRP-03 Completion Plan - Genetics, Strains & Batches

**Document Type:** Planning Template (Archived)  
**Created:** October 2, 2025  
**Status:** ✅ Delivered — template retained for reference  
**Actual Completion:** October 2, 2025  
**Owner:** Core Platform/Genetics Squad

> **Update:** FRP-03 is complete and production-ready. The unchecked checklist items below represent the original template and no longer track active work. Final evidence lives in `FRP03_FINAL_STATUS_UPDATE.md` and `FRP03_CURRENT_STATUS.md`.

---

## 📊 COMPLETION OVERVIEW

### Scope Summary
FRP-03 establishes the genetics and batch management foundation for the Harvestry ERP system. This includes strain definitions, batch lifecycle tracking, mother plant health monitoring, and lineage traceability for compliance reporting.

### Key Deliverables
- **Genetics Management System** - Strain definitions, phenotypes, genetic profiles
- **Batch Lifecycle Engine** - State machine with event tracking and lineage
- **Configurable Stage Templates** - Site-defined stages & transitions with optional commissioning support
- **Mother Plant Registry** - Health logs, propagation tracking, genetic source
- **Compliance Foundation** - Lineage tracking for seed-to-sale reporting

> Optional setup/commissioning services can assist customers with stage template configuration during onboarding, ensuring jurisdictional alignment without bespoke engineering.

### Success Criteria
- ✅ All checklist items completed
- ✅ 8 quality gates passed
- ✅ Acceptance criteria validated
- ✅ Performance targets met (p95 < 200ms)
- ✅ Integration tests passing
- ✅ Ready for FRP-07 handoff

---

## 📋 COMPLETION CHECKLIST

### Phase 1: Database Schema ✅

#### Migration 1: Genetics & Strains
- [ ] `genetics` table with genetic profiles, cannabinoid ranges, terpene profiles
- [ ] `phenotypes` table with expression notes, visual characteristics, aroma profiles
- [ ] `strains` table with breeder info, cultivation notes, compliance requirements
- [ ] RLS policies for all genetics tables
- [ ] Indexes for performance optimization

#### Migration 2: Batches & Mother Plants
- [ ] `batch_stage_definitions` table for site-configurable stages
- [ ] `batch_stage_transitions` table for allowed stage flows
- [ ] `batch_stage_history` table for audit trail
- [ ] `batches` table with lifecycle tracking, state machine, lineage
- [ ] `batch_events` table with event logging and audit trail
- [ ] `batch_relationships` table for splits, merges, transformations
- [ ] `batch_code_rules` table for jurisdiction-compliant code generation
- [ ] `mother_plants` table with propagation tracking, location management
- [ ] `mother_health_logs` table with health status, treatments, observations
- [ ] `mother_health_reminder_settings` table (reminder cadence stored, delivery hooks deferred)
- [ ] `propagation_settings` table for site-wide propagation limits + approver policy (JSON)
- [ ] `propagation_override_requests` table for approval routing
- [ ] RLS policies for all batch tables
- [ ] Indexes for performance optimization

### Phase 2: Domain Layer ✅

#### Domain Entities
- [ ] `Genetics.cs` - Genetic profiles, cannabinoid ranges, terpene profiles
- [ ] `Phenotype.cs` - Expression characteristics, visual traits, aroma profiles
- [ ] `Strain.cs` - Named combinations, breeder info, cultivation notes
- [ ] `Batch.cs` - Lifecycle state machine, regulatory stage tracking, lineage
- [ ] `BatchEvent.cs` - Event logging, state changes, audit trail
- [ ] `BatchRelationship.cs` - Parent-child relationships, splits, merges
- [ ] `BatchStageDefinition.cs` - Site-level stage metadata
- [ ] `BatchStageTransition.cs` - Allowed stage flows
- [ ] `BatchStageHistory.cs` - Stage change audit trail
- [ ] `BatchCodeRule.cs` - Rule-driven batch code definitions
- [ ] `MotherPlant.cs` - Propagation tracking, health monitoring, location management
- [ ] `MotherHealthLog.cs` - Health status, treatments, observations
- [ ] `MotherHealthReminderSettings.cs` - Reminder cadence configuration
- [ ] `PropagationSettings.cs` - Site-wide propagation limits and approval policy
- [ ] `PropagationOverrideRequest.cs` - Approval workflow aggregate

#### Value Objects
- [ ] `BatchCode.cs` - Unique batch identifier generation
- [ ] `PlantId.cs` - Mother plant identifier
- [ ] `GeneticProfile.cs` - Growth characteristics, environmental preferences
- [ ] `TerpeneProfile.cs` - Aroma and flavor profiles
- [ ] `HealthAssessment.cs` - Structured health observations
- [ ] `StageKey.cs` - Stable identifier for site-defined stages
- [ ] `TargetEnvironment.cs` - Environmental preferences blueprint
- [ ] `ComplianceRequirements.cs` - Jurisdictional reporting metadata

#### Enums
- [ ] `GeneticType.cs` - Indica, Sativa, Hybrid, Autoflower, Hemp
- [ ] `YieldPotential.cs` - Low, Medium, High, VeryHigh
- [ ] `BatchType.cs` - Seed, Clone, TissueCulture, MotherPlant
- [ ] `BatchSourceType.cs` - Purchase, Propagation, Breeding, TissueCulture
- [ ] `BatchStatus.cs` - Active, Quarantine, Hold, Destroyed, Completed, Transferred
- [ ] `MotherPlantStatus.cs` - Active, Quarantine, Retired, Destroyed
- [ ] `HealthStatus.cs` - Excellent, Good, Fair, Poor, Critical
- [ ] `EventType.cs` - Created, StageChange, LocationChange, PlantCountChange, Harvest, Split, Merge, Quarantine, Hold, Destroy, NoteAdded, PhotoAdded, MeasurementRecorded
- [ ] `RelationshipType.cs` - Split, Merge, Propagation, Transformation
- [ ] `PressureLevel.cs` - None, Low, Medium, High
- [ ] `PropagationOverrideStatus.cs` - Pending, Approved, Rejected, Expired
- [ ] `StandardBatchStage.cs` - Default templates shipped with the platform

### Phase 3: Application Layer ✅

#### Application Services
- [ ] `GeneticsManagementService.cs` - Genetics, phenotype, and strain CRUD with compliance data
- [ ] `BatchLifecycleService.cs` - Batch lifecycle, regulatory stage enforcement, batch code rule execution
- [ ] `BatchStageConfigurationService.cs` - Stage definition, ordering, and transition management
- [ ] `MotherHealthService.cs` - Mother plant health, propagation limit enforcement, override routing, reminder management

#### DTOs
- [ ] `CreateGeneticsRequest.cs` & `UpdateGeneticsRequest.cs` - Genetics CRUD payloads
- [ ] `CreatePhenotypeRequest.cs` & `UpdatePhenotypeRequest.cs` - Phenotype payloads
- [ ] `CreateStrainRequest.cs` & `UpdateStrainRequest.cs` - Strain payloads with compliance metadata
- [ ] `CreateBatchRequest.cs` & `UpdateBatchRequest.cs` - Batch lifecycle payloads
- [ ] `BatchStageRequest.cs` / `BatchStageResponse.cs` / `BatchStageOrderUpdateRequest.cs` - Stage configuration payloads
- [ ] `BatchStageTransitionRequest.cs` / `BatchStageTransitionResponse.cs` - Transition management payloads
- [ ] `BatchStageChangeRequest.cs` - Regulatory stage transition validation
- [ ] `BatchSplitRequest.cs` / `BatchMergeRequest.cs` - Relationship operations
- [ ] `BatchCodeGenerationContext.cs` - Context for rule evaluation
- [ ] `BatchCodeRuleRequest.cs` / `BatchCodeRuleResponse.cs` - Rule management
- [ ] `BatchCodeResponse.cs` - Generated batch code
- [ ] `UpdatePropagationSettingsRequest.cs` / `PropagationSettingsResponse.cs` - Site-wide propagation controls
- [ ] `RegisterPropagationRequest.cs` - Propagation count submission
- [ ] `CreatePropagationOverrideRequest.cs` / `PropagationOverrideDecisionRequest.cs` / `PropagationOverrideResponse.cs` - Override workflow DTOs
- [ ] `MotherPlantHealthLogRequest.cs` / `HealthAssessmentDto.cs` - Health logging payloads
- [ ] `UpdateMotherPlantRequest.cs` / `MotherPlantResponse.cs` - Mother plant management
- [ ] `GeneticsResponse.cs`, `PhenotypeResponse.cs`, `StrainResponse.cs`, `BatchResponse.cs`, `BatchLineageResponse.cs`, `BatchEventResponse.cs` - Read models
- [ ] `MotherPlantHealthSummaryResponse.cs` - Aggregated health view
- [ ] `MotherHealthReminderSettingsResponse.cs` & `UpdateHealthReminderSettingsRequest.cs` - Reminder cadence configuration

#### Interfaces
- [ ] `IGeneticsManagementService.cs` - Genetics management contract
- [ ] `IBatchLifecycleService.cs` - Batch lifecycle contract
- [ ] `IBatchStageConfigurationService.cs` - Stage configuration contract
- [ ] `IMotherHealthService.cs` - Mother plant health contract

### Phase 4: Infrastructure Layer ✅

#### Repositories
- [ ] `GeneticsDbContext.cs` - Database context with RLS integration
- [ ] `GeneticsRepository.cs` - Genetics CRUD with RLS
- [ ] `PhenotypeRepository.cs` - Phenotype CRUD with RLS
- [ ] `StrainRepository.cs` - Strain CRUD with RLS
- [ ] `BatchRepository.cs` - Batch CRUD with RLS and regulatory stage queries
- [ ] `BatchEventRepository.cs` - Event logging with RLS
- [ ] `BatchRelationshipRepository.cs` - Relationship tracking with RLS
- [ ] `BatchStageDefinitionRepository.cs` - Stage definition persistence with RLS
- [ ] `BatchStageTransitionRepository.cs` - Transition rules with RLS
- [ ] `BatchStageHistoryRepository.cs` - Stage history audit with RLS
- [ ] `BatchCodeRuleRepository.cs` - Rule management with RLS
- [ ] `MotherPlantRepository.cs` - Mother plant CRUD with RLS
- [ ] `MotherHealthLogRepository.cs` - Health log CRUD with RLS
- [ ] `MotherHealthReminderSettingsRepository.cs` - Reminder cadence with RLS
- [ ] `PropagationSettingsRepository.cs` - Site-wide propagation settings with RLS
- [ ] `PropagationOverrideRequestRepository.cs` - Override workflow persistence with RLS

### Phase 5: API Layer ✅

#### Controllers
- [ ] `GeneticsController.cs` - Genetics CRUD endpoints
- [ ] `StrainsController.cs` - Strain CRUD endpoints
- [ ] `BatchesController.cs` - Batch lifecycle endpoints (stages, splits, quarantine, destroy)
- [ ] `BatchStagesController.cs` - Stage definition, ordering, and transition endpoints
- [ ] `BatchCodeRulesController.cs` - Batch code rule management + preview
- [ ] `MotherPlantsController.cs` - Mother plant health & propagation endpoints
- [ ] `PropagationController.cs` - Site-wide propagation settings, override approvals

#### Validators
- [ ] `CreateGeneticsRequestValidator.cs` / `UpdateGeneticsRequestValidator.cs` - Genetics payload validation
- [ ] `CreateStrainRequestValidator.cs` / `UpdateStrainRequestValidator.cs` - Strain payload validation
- [ ] `CreateBatchRequestValidator.cs` / `UpdateBatchRequestValidator.cs` - Batch payload validation
- [ ] `BatchStageRequestValidator.cs` / `BatchStageOrderUpdateRequestValidator.cs` - Stage definition validation
- [ ] `BatchStageTransitionRequestValidator.cs` - Transition rule validation
- [ ] `BatchStageChangeRequestValidator.cs` - Regulatory stage validation
- [ ] `BatchSplitRequestValidator.cs` / `BatchMergeRequestValidator.cs` - Relationship validation
- [ ] `BatchCodeRuleRequestValidator.cs` / `BatchCodeGenerationContextValidator.cs` - Rule + preview validation
- [ ] `UpdatePropagationSettingsRequestValidator.cs` - Propagation limit validation
- [ ] `RegisterPropagationRequestValidator.cs` - Propagation count validation
- [ ] `CreatePropagationOverrideRequestValidator.cs` / `PropagationOverrideDecisionRequestValidator.cs` - Override workflow validation
- [ ] `UpdateMotherPlantRequestValidator.cs` - Mother plant updates
- [ ] `MotherPlantHealthLogRequestValidator.cs` - Health log validation
- [ ] `UpdateHealthReminderSettingsRequestValidator.cs` - Reminder cadence validation

### Phase 6: Testing ✅

#### Unit Tests
- [ ] `GeneticsTests.cs` - Genetics domain logic tests
- [ ] `PhenotypeTests.cs` - Phenotype domain logic tests
- [ ] `StrainTests.cs` - Strain domain logic tests
- [ ] `BatchTests.cs` - Batch state machine + regulatory stages
- [ ] `BatchStageDefinitionTests.cs` - Stage definition ordering and metadata
- [ ] `BatchStageTransitionTests.cs` - Transition rules and approvals
- [ ] `BatchStageHistoryTests.cs` - Stage audit trail
- [ ] `BatchCodeRuleTests.cs` - Rule parsing and resets
- [ ] `PropagationSettingsTests.cs` - Limit calculations
- [ ] `PropagationOverrideRequestTests.cs` - Approval lifecycle
- [ ] `GeneticsManagementServiceTests.cs` - Service logic tests
- [ ] `BatchLifecycleServiceTests.cs` - Lifecycle + stage enforcement
- [ ] `BatchStageConfigurationServiceTests.cs` - Stage configuration orchestration
- [ ] `BatchCodeRuleServiceTests.cs` - Rule management service tests
- [ ] `MotherHealthServiceTests.cs` - Health, propagation, overrides

#### Integration Tests
- [ ] `GeneticsManagementTests.cs` - Genetics E2E tests
- [ ] `BatchLifecycleTests.cs` - Batch lifecycle + regulatory stage tests
- [ ] `BatchStageConfigurationTests.cs` - Stage definition + transition workflows
- [ ] `BatchCodeRuleTests.cs` - Rule evaluation + preview E2E
- [ ] `MotherPlantTests.cs` - Mother plant health + propagation tests
- [ ] `PropagationControlTests.cs` - Site-wide limits & overrides
- [ ] `RlsGeneticsTests.cs` - RLS security tests (genetics, batches, rules, propagation)

### Phase 7: Production Polish ✅

#### Configuration
- [ ] `Program.cs` - DI registration for all services
- [ ] `appsettings.json` - Configuration for genetics service
- [ ] Environment variables documented
- [ ] Connection string management

#### Quality Assurance
- [ ] Code coverage ≥90% for services
- [ ] All tests passing
- [ ] No linter warnings
- [ ] Swagger documentation complete
- [ ] Manual testing completed

---

## 🎯 ACCEPTANCE CRITERIA VALIDATION

### Functional Requirements
- [ ] **Batch lineage tracked correctly** - Parent-child relationships maintained
- [ ] **Mother plant health logs retrievable** - Health history accessible
- [ ] **Strain-specific blueprints associable** - Strain-to-batch relationships
- [ ] **Configurable batch stages operational** - Site-defined stages & transitions drive lifecycle
- [ ] **Regulatory batch stages enforced** - Required compliance stages captured
- [ ] **Propagation limits enforced with approvals** - Overrides require explicit approval
- [ ] **Batch code rules satisfy jurisdictional formatting** - Rule engine configured per site
- [ ] **RLS blocks cross-site access** - Security validated

### Non-Functional Requirements
- [ ] **API p95 response time < 200ms** - Performance validated
- [ ] **Unit test coverage ≥90%** - Coverage verified
- [ ] **Integration tests passing** - E2E scenarios validated
- [ ] **Error responses follow ProblemDetails RFC** - Error handling validated
- [ ] **Structured logging throughout** - Logging validated

### Security Requirements
- [ ] **Row-Level Security enforced** - RLS policies validated
- [ ] **ABAC policy evaluation** - Authorization validated
- [ ] **Audit trail complete** - Event logging validated
- [ ] **Generic error messages** - Security validated

---

## 📊 QUALITY GATES

### Gate 1: Infrastructure with RLS ✅
- [ ] All repositories with Row-Level Security
- [ ] RLS policies enforced at database level
- [ ] Cross-site access blocked
- [ ] Audit trail with event logging

### Gate 2: Unit Test Coverage ✅
- [ ] 7 comprehensive test files
- [ ] Domain entity tests (Genetics, Strain, Batch, MotherPlant)
- [ ] Service tests (GeneticsManagement, BatchLifecycle, MotherHealth)
- [ ] Coverage: ≥90% for all application services

### Gate 3: API Endpoints Operational ✅
- [ ] 4 controllers (Genetics, Strains, Batches, MotherPlants)
- [ ] Batch lifecycle state machine
- [ ] Mother plant health tracking
- [ ] OpenAPI/Swagger documentation

### Gate 4: Integration Tests Passing ✅
- [ ] 4 integration test files
- [ ] E2E genetics → strain → batch flow
- [ ] Batch lifecycle state transitions
- [ ] Mother plant health tracking
- [ ] RLS security scenarios

### Gate 5: Background Jobs Scheduled ✅
- [ ] Health check reminders (if implemented)
- [ ] Batch stage transition notifications (if implemented)
- [ ] Mother plant propagation tracking (if implemented)

### Gate 6: Health Checks Passing ✅
- [ ] Database connectivity check
- [ ] Migration status validation
- [ ] Integration with ASP.NET Health Checks

### Gate 7: Swagger Documentation Published ✅
- [ ] Full OpenAPI specification
- [ ] Request/response examples
- [ ] Authentication documentation
- [ ] API versioning ready

### Gate 8: Production Polish Complete ✅
- [ ] CORS policy configured
- [ ] Error handling middleware (ProblemDetails)
- [ ] FluentValidation (7 validators)
- [ ] Serilog structured logging (JSON)
- [ ] Rate limiting (sliding window)

---

## 🚀 DEPLOYMENT READINESS

### Pre-Deployment Checklist
- [ ] All code committed to repository
- [ ] All tests passing in CI/CD
- [ ] Database migrations ready
- [ ] Environment variables documented
- [ ] Health checks configured
- [ ] Logging configured
- [ ] Error handling complete
- [ ] Rate limiting active
- [ ] CORS configured
- [ ] OpenAPI docs published

### Post-Deployment Validation
- [ ] Health checks passing
- [ ] API endpoints responding
- [ ] Database connections stable
- [ ] RLS policies enforced
- [ ] Performance metrics within targets
- [ ] Error rates within acceptable limits

---

## 📈 SUCCESS METRICS

### Delivery Metrics (Targets)
- **Total Files:** 60-70 C# files (target)
- **Total Lines:** ~6,500-8,500 lines of code (target)
- **Quality Gates:** 0 out of 8 completed (template)
- **Test Coverage:** ≥90% for services (target)
- **Performance:** p95 < 200ms (target)

### Functional Metrics
- **Genetics Management:** Complete CRUD operations
- **Batch Lifecycle:** State machine operational
- **Configurable Stages:** Site-defined stage templates & transitions manageable
- **Mother Plant Health:** Health tracking complete
- **Lineage Tracking:** Parent-child relationships maintained
- **Compliance:** Foundation for seed-to-sale reporting

### Technical Metrics
- **API Endpoints:** 30+ endpoints operational
- **Database Tables:** 15 tables with RLS
- **Integration Tests:** 6 test files with E2E scenarios
- **Unit Tests:** 15 test files with ≥90% coverage
- **Documentation:** Complete OpenAPI specification

---

## 🎓 LESSONS LEARNED

### What Went Well
1. **Clean Architecture** - Clear separation of concerns
2. **State Machine Design** - Batch lifecycle well-defined
3. **Configurable Stages** - Customers can tune lifecycle templates per site
4. **Health Monitoring** - Mother plant health tracking complete
5. **Compliance Foundation** - Ready for seed-to-sale reporting

### Technical Highlights
1. **Domain-Driven Design** - Rich domain models
2. **Event Sourcing** - Batch events for audit trail
3. **Stage Configuration** - Dynamic stage definitions + transitions backed by RLS
4. **Health Tracking** - Comprehensive mother plant monitoring
5. **Lineage Queries** - Efficient relationship traversal

### Patterns Established
1. **Batch Lifecycle** - State machine pattern
2. **Stage Configuration** - Site-specific template pattern
3. **Event Logging** - Audit trail pattern
4. **Health Monitoring** - Health tracking pattern
5. **Lineage Management** - Relationship tracking pattern
6. **Compliance Reporting** - Data foundation pattern

---

## 📝 SIGN-OFF

**Feature:** FRP-03 - Genetics, Strains & Batches  
**Status:** 📋 **PLANNING TEMPLATE - WORK NOT STARTED**  
**Quality Gates:** 0 out of 8 completed (template)  
**Recommendation:** **Template prepared for future implementation**

**Planning Status:**

- ⏳ Development: Not started (target: 45-55 files, 5,000-6,500 lines)
- ⏳ Testing: Not started (target: 11 test files)
- ⏳ Security: Not started (RLS + ABAC + Audit trail)
- ⏳ Operations: Not started (Health + Logging + Monitoring)

**Next Actions:**

1. ⏳ Await prioritization of FRP-03 work
2. ⏳ Update this document with actual dates when work begins
3. ⏳ Check off items as they are completed
4. ⏳ Update sign-off section when work is complete

---

**Template Created:** October 2, 2025  
**Actual Start Date:** TBD  
**Estimated Effort:** 18-22 hours  
**Target Completion:** TBD
