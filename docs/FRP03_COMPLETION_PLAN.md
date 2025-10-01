# FRP-03 Completion Plan - Genetics, Strains & Batches

**Document Type:** Planning Template  
**Created:** October 2, 2025  
**Status:** 📋 **Template / Not Started**  
**Target Completion:** TBD  
**Owner:** Core Platform/Genetics Squad

**NOTE:** This is a planning template for FRP-03 implementation. All checklist items below are unchecked as this work has not yet begun. Update this header and all checkboxes as work progresses.

---

## 📊 COMPLETION OVERVIEW

### Scope Summary
FRP-03 establishes the genetics and batch management foundation for the Harvestry ERP system. This includes strain definitions, batch lifecycle tracking, mother plant health monitoring, and lineage traceability for compliance reporting.

### Key Deliverables
- **Genetics Management System** - Strain definitions, phenotypes, genetic profiles
- **Batch Lifecycle Engine** - State machine with event tracking and lineage
- **Mother Plant Registry** - Health logs, propagation tracking, genetic source
- **Compliance Foundation** - Lineage tracking for seed-to-sale reporting

### Success Criteria
- ✅ All 21 checklist items completed
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
- [ ] `batches` table with lifecycle tracking, state machine, lineage
- [ ] `batch_events` table with event logging and audit trail
- [ ] `batch_relationships` table for splits, merges, transformations
- [ ] `batch_code_settings` table for user-configurable batch code generation
- [ ] `mother_plants` table with propagation tracking, location management
- [ ] `mother_health_logs` table with health status, treatments, observations
- [ ] `mother_health_reminder_settings` table for user-configurable health reminders
- [ ] RLS policies for all batch tables
- [ ] Indexes for performance optimization

### Phase 2: Domain Layer ✅

#### Domain Entities
- [ ] `Genetics.cs` - Genetic profiles, cannabinoid ranges, terpene profiles
- [ ] `Phenotype.cs` - Expression characteristics, visual traits, aroma profiles
- [ ] `Strain.cs` - Named combinations, breeder info, cultivation notes
- [ ] `Batch.cs` - Lifecycle state machine, plant count tracking, location management, batch code generation
- [ ] `BatchEvent.cs` - Event logging, state changes, audit trail
- [ ] `BatchRelationship.cs` - Parent-child relationships, splits, merges
- [ ] `BatchCodeSettings.cs` - User-configurable batch code generation settings
- [ ] `MotherPlant.cs` - Propagation tracking, health monitoring, location management, configurable limits
- [ ] `MotherHealthLog.cs` - Health status, treatments, observations
- [ ] `MotherHealthReminderSettings.cs` - User-configurable health reminder settings

#### Value Objects
- [ ] `BatchCode.cs` - Unique batch identifier generation
- [ ] `PlantId.cs` - Mother plant identifier
- [ ] `GeneticProfile.cs` - Growth characteristics, environmental preferences
- [ ] `TerpeneProfile.cs` - Aroma and flavor profiles
- [ ] `HealthStatus.cs` - Health assessment values

#### Enums
- [ ] `GeneticType.cs` - Indica, Sativa, Hybrid, Autoflower, Hemp
- [ ] `BatchType.cs` - Seed, Clone, TissueCulture, MotherPlant
- [ ] `BatchStage.cs` - Germination, Seedling, Veg, PreFlower, Flower, Harvest, Cure, Packaged, Shipped, Destroyed
- [ ] `BatchStatus.cs` - Active, Quarantine, Hold, Destroyed, Completed
- [ ] `EventType.cs` - Created, StageChange, LocationChange, PlantCountChange, Harvest, Split, Merge, Quarantine, Hold, Destroy, NoteAdded, PhotoAdded, MeasurementRecorded
- [ ] `RelationshipType.cs` - Split, Merge, Propagation, Transformation
- [ ] `HealthStatus.cs` - Excellent, Good, Fair, Poor, Critical

### Phase 3: Application Layer ✅

#### Application Services
- [ ] `GeneticsManagementService.cs` - Genetics, phenotype, and strain CRUD operations
- [ ] `BatchLifecycleService.cs` - Batch lifecycle management, state transitions, event tracking, batch code generation
- [ ] `MotherHealthService.cs` - Mother plant health logging, propagation tracking, reminder management

#### DTOs
- [ ] `CreateGeneticsRequest.cs` - Genetics creation with validation
- [ ] `CreatePhenotypeRequest.cs` - Phenotype creation with genetics reference
- [ ] `CreateStrainRequest.cs` - Strain creation with genetics and phenotype
- [ ] `CreateBatchRequest.cs` - Batch creation with strain reference
- [ ] `BatchStageChangeRequest.cs` - Stage transition with validation
- [ ] `BatchSplitRequest.cs` - Batch splitting with plant count (partial/complete)
- [ ] `BatchMergeRequest.cs` - Batch merging with validation
- [ ] `MotherPlantHealthLogRequest.cs` - Health log entry with observations
- [ ] `UpdateBatchCodeSettingsRequest.cs` - Batch code generation settings
- [ ] `UpdateHealthReminderSettingsRequest.cs` - Health reminder settings
- [ ] `GeneticsResponse.cs` - Genetics data response
- [ ] `StrainResponse.cs` - Strain data response
- [ ] `BatchResponse.cs` - Batch data response
- [ ] `BatchLineageResponse.cs` - Lineage relationship response
- [ ] `BatchCodeResponse.cs` - Generated batch code response
- [ ] `BatchCodeSettingsResponse.cs` - Batch code settings response
- [ ] `MotherPlantResponse.cs` - Mother plant data response
- [ ] `MotherHealthReminderSettingsResponse.cs` - Health reminder settings response

#### Interfaces
- [ ] `IGeneticsManagementService.cs` - Genetics management contract
- [ ] `IBatchLifecycleService.cs` - Batch lifecycle contract
- [ ] `IMotherHealthService.cs` - Mother plant health contract

### Phase 4: Infrastructure Layer ✅

#### Repositories
- [ ] `GeneticsDbContext.cs` - Database context with RLS integration
- [ ] `GeneticsRepository.cs` - Genetics CRUD with RLS
- [ ] `PhenotypeRepository.cs` - Phenotype CRUD with RLS
- [ ] `StrainRepository.cs` - Strain CRUD with RLS
- [ ] `BatchRepository.cs` - Batch CRUD with RLS and state queries
- [ ] `BatchEventRepository.cs` - Event logging with RLS
- [ ] `BatchRelationshipRepository.cs` - Relationship tracking with RLS
- [ ] `BatchCodeSettingsRepository.cs` - Batch code settings with RLS
- [ ] `MotherPlantRepository.cs` - Mother plant CRUD with RLS
- [ ] `MotherHealthLogRepository.cs` - Health log CRUD with RLS
- [ ] `MotherHealthReminderSettingsRepository.cs` - Health reminder settings with RLS

### Phase 5: API Layer ✅

#### Controllers
- [ ] `GeneticsController.cs` - Genetics CRUD endpoints
- [ ] `StrainsController.cs` - Strain CRUD endpoints
- [ ] `BatchesController.cs` - Batch lifecycle endpoints, batch code generation, settings management
- [ ] `MotherPlantsController.cs` - Mother plant health endpoints, reminder settings management

#### Validators
- [ ] `CreateGeneticsRequestValidator.cs` - Genetics creation validation
- [ ] `CreateStrainRequestValidator.cs` - Strain creation validation
- [ ] `CreateBatchRequestValidator.cs` - Batch creation validation
- [ ] `BatchStageChangeRequestValidator.cs` - Stage transition validation
- [ ] `BatchSplitRequestValidator.cs` - Batch splitting validation (partial/complete)
- [ ] `BatchMergeRequestValidator.cs` - Batch merging validation
- [ ] `MotherPlantHealthLogRequestValidator.cs` - Health log validation
- [ ] `UpdateBatchCodeSettingsRequestValidator.cs` - Batch code settings validation
- [ ] `UpdateHealthReminderSettingsRequestValidator.cs` - Health reminder settings validation

### Phase 6: Testing ✅

#### Unit Tests
- [ ] `GeneticsTests.cs` - Genetics domain logic tests
- [ ] `StrainTests.cs` - Strain domain logic tests
- [ ] `BatchTests.cs` - Batch state machine tests
- [ ] `MotherPlantTests.cs` - Mother plant health tests
- [ ] `GeneticsManagementServiceTests.cs` - Service logic tests
- [ ] `BatchLifecycleServiceTests.cs` - Batch lifecycle tests
- [ ] `MotherHealthServiceTests.cs` - Mother plant health tests

#### Integration Tests
- [ ] `GeneticsManagementTests.cs` - Genetics E2E tests
- [ ] `BatchLifecycleTests.cs` - Batch lifecycle E2E tests
- [ ] `MotherPlantTests.cs` - Mother plant E2E tests
- [ ] `RlsGeneticsTests.cs` - RLS security tests

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
- [ ] **Batch state machine enforces valid transitions** - Invalid transitions blocked
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
- **Total Files:** 45-55 C# files (target)
- **Total Lines:** ~5,000-6,500 lines of code (target)
- **Quality Gates:** 0 out of 8 completed (template)
- **Test Coverage:** ≥90% for services (target)
- **Performance:** p95 < 200ms (target)

### Functional Metrics
- **Genetics Management:** Complete CRUD operations
- **Batch Lifecycle:** State machine operational
- **Mother Plant Health:** Health tracking complete
- **Lineage Tracking:** Parent-child relationships maintained
- **Compliance:** Foundation for seed-to-sale reporting

### Technical Metrics
- **API Endpoints:** 25+ endpoints operational
- **Database Tables:** 8 tables with RLS
- **Integration Tests:** 4 test files with E2E scenarios
- **Unit Tests:** 7 test files with ≥90% coverage
- **Documentation:** Complete OpenAPI specification

---

## 🎓 LESSONS LEARNED

### What Went Well
1. **Clean Architecture** - Clear separation of concerns
2. **State Machine Design** - Batch lifecycle well-defined
3. **Lineage Tracking** - Parent-child relationships maintained
4. **Health Monitoring** - Mother plant health tracking complete
5. **Compliance Foundation** - Ready for seed-to-sale reporting

### Technical Highlights
1. **Domain-Driven Design** - Rich domain models
2. **Event Sourcing** - Batch events for audit trail
3. **State Machine** - Validated batch transitions
4. **Health Tracking** - Comprehensive mother plant monitoring
5. **Lineage Queries** - Efficient relationship traversal

### Patterns Established
1. **Batch Lifecycle** - State machine pattern
2. **Event Logging** - Audit trail pattern
3. **Health Monitoring** - Health tracking pattern
4. **Lineage Management** - Relationship tracking pattern
5. **Compliance Reporting** - Data foundation pattern

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

