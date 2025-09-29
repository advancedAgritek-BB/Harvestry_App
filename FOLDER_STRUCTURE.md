# Harvestry ERP & Control - Folder Structure

**Version:** 1.0  
**Date:** 2025-09-29  
**Purpose:** Define the enterprise-grade folder structure for the Harvestry cultivation OS platform

---

## Overview

This folder structure is designed to support:

- **Microservices Architecture** with bounded contexts aligned to squad ownership
- **Clean Architecture** principles (separation of concerns, dependency inversion)
- **SOLID principles** with modularity and single responsibility
- **Scalability** for multi-site, high-throughput operations
- **Enterprise requirements** including observability, DR, security, and compliance

---

## Root Structure

```
Harvestry_App/
├── src/                    # All source code
│   ├── backend/           # Backend microservices
│   ├── frontend/          # Next.js web application
│   ├── shared/            # Shared libraries and utilities
│   ├── infrastructure/    # Infrastructure as Code, DevOps
│   ├── database/          # Database schemas, migrations, functions
│   └── edge/              # Edge controllers, firmware, IoT
├── tests/                 # All testing code (unit, integration, e2e, performance)
├── scripts/               # Development, deployment, and operational scripts
├── tools/                 # CLI tools, generators, analyzers
├── docs/                  # Documentation organized by audience and purpose
├── config/                # Configuration files per environment
├── data/                  # Data imports, exports, backups
├── logs/                  # Application and audit logs
├── documents/             # Product requirements, architecture docs (existing)
├── README.md              # Project overview and getting started
└── LICENSE                # License information
```

---

## Backend Structure (`src/backend/`)

The backend is organized into **microservices** aligned with **squad structure** and **bounded contexts**.

### Service Organization

```
src/backend/services/
├── core-platform/              # Squad: Core Platform
│   ├── identity/               # Authentication, authorization, RLS, ABAC, badges
│   ├── organizations/          # Organizations, sites, user roles
│   ├── spatial/                # Sites, rooms, zones, racks, bins, equipment
│   ├── inventory/              # Lots, balances, movements, labels, GS1/UDI
│   └── processing/             # Process definitions, runs, labor, waste
│
├── telemetry-controls/         # Squad: Telemetry & Controls
│   ├── sensors/                # Sensor streams, readings, telemetry ingest
│   ├── irrigation-fertigation/ # Programs, schedules, recipes, mix tanks, injectors
│   ├── environment/            # Air, canopy, substrate monitoring, targets, alerts
│   ├── interlocks/             # Safety interlocks, e-stop, curfews, bounds
│   └── closed-loop/            # EC/pH closed loop, autosteer MPC, crop steering
│
├── workflow-messaging/         # Squad: Workflow & Messaging
│   ├── lifecycle/              # Batch lifecycle, state transitions, blueprints
│   ├── tasks/                  # Task management, dependencies, approvals, SLAs
│   ├── messaging/              # In-app conversations, notifications, escalations
│   └── slack-bridge/           # Slack integration, mirroring, slash commands
│
├── integrations/               # Squad: Integrations
│   ├── compliance-metrc/       # METRC state compliance integration
│   ├── compliance-biotrack/    # BioTrack state compliance integration
│   ├── quickbooks/             # QuickBooks Online item-level & GL summary
│   └── labeling/               # GS1, UDI, jurisdiction-specific label generation
│
├── data-ai/                    # Squad: Data & AI
│   ├── analytics/              # KPIs, rollups, reports, dashboards, OLAP
│   ├── ai-models/              # Anomaly detection, yield prediction, ET0, Copilot
│   ├── sustainability/         # WUE, NUE, kWh/gram, CO2 intensity, ESG reports
│   └── predictive-maintenance/ # Equipment failure prediction, drift detection
│
└── gateway/                    # API Gateway
    ├── API/                    # Routes, middleware, rate limiting
    ├── Configuration/          # Gateway config, service discovery
    ├── Policies/               # Routing policies, circuit breakers
    └── Tests/                  # Gateway integration tests
```

### Service Internal Structure (Clean Architecture)

Each service follows **clean architecture** with clear layer separation:

```
<service-name>/
├── API/                        # Presentation layer
│   ├── Controllers/            # REST API endpoints
│   ├── Middleware/             # Request/response pipeline
│   └── Validators/             # Input validation
│
├── Application/                # Application logic layer
│   ├── Commands/               # CQRS commands (write operations)
│   ├── Queries/                # CQRS queries (read operations)
│   ├── Services/               # Application services, orchestration
│   ├── ViewModels/             # Response models for UI
│   ├── DTOs/                   # Data transfer objects
│   └── Interfaces/             # Application-level abstractions
│
├── Domain/                     # Business logic layer (core)
│   ├── Entities/               # Domain entities with business rules
│   ├── ValueObjects/           # Immutable value objects
│   ├── Enums/                  # Domain enumerations
│   ├── Events/                 # Domain events for event sourcing
│   └── Specifications/         # Domain specifications pattern
│
├── Infrastructure/             # External dependencies layer
│   ├── Persistence/            # Database repositories, EF Core contexts
│   ├── External/               # Third-party API clients, adapters
│   ├── Messaging/              # Message bus publishers/consumers, outbox
│   └── DeviceAdapters/         # IoT device protocol adapters (where applicable)
│
└── Tests/                      # Service-specific tests
    ├── Unit/                   # Unit tests for domain and application logic
    └── Integration/            # Integration tests with external dependencies
```

---

## Frontend Structure (`src/frontend/`)

Next.js application with **feature-based organization** and clean component hierarchy.

```
src/frontend/
├── app/                        # Next.js App Router
│   ├── api/                    # API routes (BFF pattern)
│   ├── auth/                   # Authentication pages (login, register, SSO)
│   ├── dashboard/              # Main dashboard and analytics
│   ├── cultivation/            # Cultivation management (batches, tasks, lifecycle)
│   ├── inventory/              # Inventory and warehouse management
│   ├── processing/             # Processing and manufacturing
│   ├── compliance/             # Compliance, COA, METRC/BioTrack
│   ├── analytics/              # Reports, KPIs, sustainability
│   └── settings/               # Organization, site, user settings
│
├── components/                 # Reusable UI components
│   ├── common/                 # Buttons, inputs, cards, badges
│   ├── forms/                  # Form components, validation
│   ├── charts/                 # Data visualization (SciChart, Chart.js)
│   ├── tables/                 # Data grids, sortable tables
│   ├── modals/                 # Dialogs, confirmations
│   ├── navigation/             # Navbars, sidebars, breadcrumbs
│   └── alerts/                 # Toasts, notifications, banners
│
├── features/                   # Feature-specific modules
│   ├── identity/               # Auth, user profile, badges
│   ├── spatial/                # Site, room, zone, equipment UI
│   ├── telemetry/              # Real-time sensor data, charts
│   ├── irrigation/             # Irrigation programs, schedules
│   ├── tasks/                  # Task lists, approvals, delegation
│   ├── messaging/              # In-app messaging, Slack integration
│   ├── compliance/             # METRC/BioTrack, COA, holds
│   ├── quickbooks/             # QBO sync status, reconciliation
│   ├── ai/                     # Copilot, autosteer, AI recommendations
│   └── sustainability/         # ESG dashboards, WUE, NUE
│
├── layouts/                    # Page layouts
│   ├── MainLayout/             # Default layout with nav, footer
│   ├── DashboardLayout/        # Dashboard-specific layout
│   └── AuthLayout/             # Auth pages layout (centered)
│
├── hooks/                      # Custom React hooks
│   ├── data/                   # Data fetching, caching (React Query)
│   ├── auth/                   # Authentication state
│   ├── realtime/               # WebSocket/SignalR connections
│   └── device/                 # Device state and commands
│
├── services/                   # Frontend services
│   ├── api/                    # API client, request/response handling
│   ├── auth/                   # Auth service, token management
│   ├── realtime/               # Real-time connection management
│   ├── notifications/          # Notification service
│   └── storage/                # Local/session storage utilities
│
├── stores/                     # State management (Zustand/Redux)
│   ├── auth/                   # Auth state store
│   ├── app/                    # Global app state
│   ├── telemetry/              # Telemetry data cache
│   ├── tasks/                  # Task management state
│   └── alerts/                 # Alert and notification state
│
├── types/                      # TypeScript type definitions
│   ├── api/                    # API response types
│   ├── domain/                 # Domain model types
│   └── ui/                     # UI component prop types
│
├── styles/                     # Styling and theming
│   ├── tokens/                 # Design tokens (colors, spacing, typography)
│   ├── themes/                 # Light/dark themes
│   └── components/             # Component-specific styles
│
├── config/                     # Frontend configuration
│   ├── constants/              # App constants, enums
│   ├── environment/            # Environment variables
│   └── routes/                 # Route definitions, permissions
│
└── public/                     # Static assets (images, fonts, icons)
```

---

## Shared Libraries (`src/shared/`)

Cross-cutting concerns and reusable components shared across services.

```
src/shared/
├── kernel/                     # Core abstractions and base classes
│   ├── Abstractions/           # Base interfaces (IEntity, IAggregateRoot)
│   ├── Domain/                 # Base domain classes
│   ├── Events/                 # Event bus, event handlers
│   ├── Exceptions/             # Standard exception types
│   └── Results/                # Result pattern implementations
│
├── messaging/                  # Messaging infrastructure
│   ├── Outbox/                 # Transactional outbox pattern
│   ├── Sagas/                  # Saga orchestration
│   ├── Publishers/             # Message publishers
│   ├── Consumers/              # Message consumers
│   └── Contracts/              # Message contracts, schemas
│
├── security/                   # Security utilities
│   ├── Authentication/         # JWT, token validation
│   ├── Authorization/          # Policy-based authorization
│   ├── RLS/                    # Row-level security helpers
│   ├── ABAC/                   # Attribute-based access control
│   └── Encryption/             # Encryption, hashing utilities
│
├── observability/              # Observability components
│   ├── Tracing/                # OpenTelemetry tracing
│   ├── Metrics/                # Prometheus metrics
│   ├── Logging/                # Structured logging (Serilog)
│   └── HealthChecks/           # Health check endpoints
│
├── data-access/                # Data access patterns
│   ├── Repository/             # Generic repository pattern
│   ├── UnitOfWork/             # Unit of work pattern
│   ├── Specifications/         # Specification pattern
│   └── Migrations/             # Migration utilities
│
├── validation/                 # Validation framework
│   ├── Validators/             # FluentValidation validators
│   ├── Rules/                  # Custom validation rules
│   └── Extensions/             # Validation extensions
│
└── utilities/                  # General utilities
    ├── Helpers/                # Helper classes
    ├── Extensions/             # Extension methods
    ├── Converters/             # Type converters
    └── Formatters/             # Data formatters
```

---

## Database (`src/database/`)

Database schemas, migrations, and database objects for PostgreSQL + TimescaleDB + ClickHouse.

```
src/database/
├── schemas/                    # Schema definitions organized by domain
│   ├── identity/               # Users, roles, badges, sessions, audit
│   ├── spatial/                # Sites, rooms, zones, racks, bins, locations
│   ├── equipment/              # Equipment, sensors, actuators, calibration
│   ├── cultivation/            # Genetics, batches, plants, movements
│   ├── tasking/                # Tasks, blueprints, approvals, delegations
│   ├── environment/            # Targets, overrides, thresholds, alerts
│   ├── irrigation/             # Programs, schedules, recipes, runs, interlocks
│   ├── inventory/              # Lots, balances, movements, labels
│   ├── processing/             # Processes, runs, labor, waste
│   ├── compliance/             # METRC/BioTrack sync, COA, holds, destruction
│   ├── accounting/             # QBO integration, mappings, queue
│   ├── ai/                     # Predictions, feedback, ET0, sustainability
│   └── queues/                 # Outbox, sync_queue, accounting_queue
│
├── migrations/                 # Database migrations
│   ├── timescale/              # TimescaleDB hypertables, continuous aggregates
│   ├── clickhouse/             # ClickHouse tables, materialized views
│   └── baseline/               # Baseline schema and initial data
│
├── seeds/                      # Seed data for different environments
│   ├── dev/                    # Development seed data
│   ├── staging/                # Staging seed data
│   └── test/                   # Test fixtures
│
├── functions/                  # Stored functions and procedures
│   ├── aggregate/              # Custom aggregate functions
│   ├── utility/                # Utility functions
│   └── audit/                  # Audit and hash chain functions
│
├── views/                      # Database views
│   ├── materialized/           # Materialized views for performance
│   ├── standard/               # Standard views
│   └── rollups/                # Telemetry rollups (1m, 5m, 1h)
│
├── triggers/                   # Database triggers
│   ├── audit/                  # Audit trail triggers
│   ├── clickhouse-sync/        # Triggers for ClickHouse sidecar sync
│   └── outbox/                 # Outbox pattern triggers
│
├── indexes/                    # Index definitions
│   └── performance/            # Performance-critical indexes
│
├── RLS-policies/               # Row-level security policies
│   ├── site-scoped/            # Site-scoped RLS policies
│   ├── user-scoped/            # User-scoped RLS policies
│   └── ABAC/                   # Attribute-based access control policies
│
└── scripts/                    # Database utility scripts
    ├── setup/                  # Initial setup scripts
    ├── backup/                 # Backup scripts
    └── maintenance/            # Maintenance and optimization
```

---

## Infrastructure (`src/infrastructure/`)

Infrastructure as Code, containerization, orchestration, and observability stack.

```
src/infrastructure/
├── docker/                     # Docker containerization
│   ├── services/               # Service-specific Dockerfiles
│   ├── compose/                # Docker Compose files (dev, staging)
│   └── dockerfiles/            # Shared Dockerfiles
│
├── kubernetes/                 # Kubernetes manifests
│   ├── deployments/            # Deployment definitions
│   ├── services/               # Service definitions
│   ├── configmaps/             # Configuration maps
│   ├── secrets/                # Secret definitions (sealed/encrypted)
│   ├── ingress/                # Ingress rules
│   ├── jobs/                   # One-time jobs
│   └── cron/                   # CronJobs (scheduled tasks)
│
├── terraform/                  # Terraform IaC
│   ├── modules/                # Reusable Terraform modules
│   ├── environments/           # Environment-specific configs (dev, staging, prod)
│   └── providers/              # Cloud provider configurations
│
├── helm/                       # Helm charts
│   ├── charts/                 # Helm chart definitions
│   └── values/                 # Environment-specific values
│
├── monitoring/                 # Prometheus monitoring
│   ├── prometheus/             # Prometheus configuration
│   ├── grafana/                # Grafana dashboards
│   ├── dashboards/             # Dashboard JSON definitions
│   └── alerts/                 # Alert rules (SLO burn rates, etc.)
│
├── logging/                    # Logging infrastructure
│   ├── loki/                   # Loki configuration
│   ├── fluentd/                # Fluentd/Fluent Bit configs
│   └── configs/                # Log aggregation configs
│
├── tracing/                    # Distributed tracing
│   ├── tempo/                  # Tempo configuration
│   ├── jaeger/                 # Jaeger (alternative)
│   └── configs/                # Tracing configs
│
├── alerting/                   # Alerting infrastructure
│   ├── rules/                  # Alerting rules (Prometheus, custom)
│   ├── receivers/              # Alert receivers (Slack, email, SMS)
│   └── policies/               # Alert routing policies
│
├── service-mesh/               # Service mesh (if used)
│   ├── istio/                  # Istio configurations
│   ├── linkerd/                # Linkerd configurations
│   └── configs/                # Mesh policies, circuit breakers
│
├── secrets/                    # Secrets management
│   ├── vault/                  # HashiCorp Vault configs
│   ├── kms/                    # Cloud KMS integration
│   └── policies/               # Secret access policies
│
├── ci-cd/                      # CI/CD pipelines
│   ├── github-actions/         # GitHub Actions workflows
│   ├── jenkins/                # Jenkins pipelines (if used)
│   └── scripts/                # Deployment scripts
│
└── scripts/                    # Infrastructure automation scripts
    ├── provisioning/           # Environment provisioning
    ├── teardown/               # Resource cleanup
    └── utilities/              # Utility scripts
```

---

## Edge & Firmware (`src/edge/`)

IoT edge controllers, firmware, device communication protocols, and OTA updates.

```
src/edge/
├── controllers/                # Edge controller applications
│   ├── main-controller/        # Primary cultivation controller (irrigation, climate)
│   ├── sensor-nodes/           # Distributed sensor nodes
│   ├── actuator-nodes/         # Actuator control nodes
│   └── safety-interlocks/      # Safety interlock controllers
│
├── adapters/                   # Device protocol adapters
│   ├── mqtt/                   # MQTT client/broker adapters
│   ├── sdi-12/                 # SDI-12 protocol adapter
│   ├── http/                   # HTTP REST adapter
│   ├── modbus/                 # Modbus RTU/TCP adapter
│   └── bacnet/                 # BACnet adapter (HVAC)
│
├── firmware/                   # Firmware source code
│   ├── bootloader/             # Bootloader for secure boot
│   ├── app/                    # Application firmware
│   ├── drivers/                # Hardware drivers
│   └── hal/                    # Hardware abstraction layer
│
├── protocols/                  # Communication protocols
│   ├── command-dispatch/       # Command dispatch protocol
│   ├── telemetry-ingest/       # Telemetry ingestion protocol
│   ├── heartbeat/              # Device heartbeat protocol
│   └── device-discovery/       # Device discovery and registration
│
├── device-registry/            # Device management
│   ├── provisioning/           # Device provisioning
│   ├── enrollment/             # Zero-touch enrollment
│   └── revocation/             # Device revocation and decommissioning
│
└── ota/                        # Over-the-air updates
    ├── updates/                # Update packages and manifests
    ├── rollback/               # Rollback mechanisms
    └── verification/           # Update verification and signing
```

---

## Testing (`tests/`)

Comprehensive testing strategy covering all testing levels.

```
tests/
├── unit/                       # Unit tests
│   ├── backend/                # Backend service unit tests
│   ├── frontend/               # Frontend component unit tests
│   └── shared/                 # Shared library unit tests
│
├── integration/                # Integration tests
│   ├── services/               # Service-to-service integration
│   ├── database/               # Database integration tests
│   ├── messaging/              # Message bus integration
│   └── external-apis/          # Third-party API integration (METRC, QBO)
│
├── e2e/                        # End-to-end tests
│   ├── user-flows/             # User journey tests (Playwright/Cypress)
│   ├── critical-paths/         # Business-critical flows
│   └── regression/             # Regression test suites
│
├── performance/                # Performance testing
│   ├── benchmarks/             # Performance benchmarks
│   ├── profiling/              # Profiling results and analysis
│   └── slos/                   # SLO validation tests (p95, p99)
│
├── security/                   # Security testing
│   ├── penetration/            # Penetration test scenarios
│   ├── fuzz/                   # Fuzz testing
│   ├── rls-abac/               # RLS/ABAC policy tests
│   └── secrets/                # Secret exposure tests
│
├── load/                       # Load testing
│   ├── stress/                 # Stress tests (breaking point)
│   ├── capacity/               # Capacity planning tests
│   └── endurance/              # Endurance/soak tests
│
├── chaos/                      # Chaos engineering
│   ├── network-partition/      # Network failure scenarios
│   ├── service-failure/        # Service crash scenarios
│   └── database-failover/      # DR failover tests
│
├── fixtures/                   # Test fixtures and data
├── helpers/                    # Test helper utilities
└── mocks/                      # Mock objects and stubs
```

---

## Scripts & Tools

### Scripts (`scripts/`)

Operational and development scripts.

```
scripts/
├── dev/                        # Development scripts
│   ├── setup.sh                # Initial dev environment setup
│   ├── run-services.sh         # Start all services locally
│   └── reset-db.sh             # Reset development database
│
├── deploy/                     # Deployment scripts
│   ├── blue-green.sh           # Blue-green deployment
│   ├── rollback.sh             # Rollback to previous version
│   └── smoke-test.sh           # Post-deployment smoke tests
│
├── db/                         # Database scripts
│   ├── migrate.sh              # Run migrations
│   ├── seed.sh                 # Seed database
│   └── optimize.sh             # Database optimization
│
├── backup/                     # Backup scripts
│   ├── db-backup.sh            # Database backup
│   └── verify-backup.sh        # Backup verification
│
├── restore/                    # Restore scripts
│   └── db-restore.sh           # Database restore from backup
│
├── monitoring/                 # Monitoring scripts
│   ├── health-check.sh         # Service health checks
│   └── burn-rate.sh            # SLO burn rate calculator
│
├── security/                   # Security scripts
│   ├── rotate-secrets.sh       # Secret rotation
│   ├── scan.sh                 # Security scanning
│   └── audit.sh                # Audit log verification
│
└── utilities/                  # Utility scripts
    ├── generate-docs.sh        # Generate API docs
    └── clean.sh                # Cleanup temp files
```

### Tools (`tools/`)

Development and operational tools.

```
tools/
├── cli/                        # CLI tool for developers and ops
│   ├── commands/               # CLI command implementations
│   └── templates/              # Code templates
│
├── generators/                 # Code generators
│   ├── service/                # Generate new microservice scaffold
│   ├── component/              # Generate React components
│   ├── migration/              # Generate database migrations
│   └── test/                   # Generate test scaffolds
│
├── validators/                 # Validation tools
│   ├── schema/                 # Schema validators
│   ├── config/                 # Configuration validators
│   └── api-contract/           # API contract validators
│
├── analyzers/                  # Code analysis tools
│   ├── code-quality/           # Code quality analysis
│   ├── dependencies/           # Dependency analysis
│   └── security-scan/          # Security vulnerability scanning
│
├── importers/                  # Data import tools
│   ├── legacy-data/            # Import from legacy systems
│   ├── metrc/                  # METRC data import
│   ├── biotrack/               # BioTrack data import
│   └── qbo/                    # QuickBooks import
│
└── exporters/                  # Data export tools
    ├── compliance/             # Compliance report exports
    ├── financial/              # Financial report exports
    ├── audit/                  # Audit trail exports
    └── reports/                # Custom report exports
```

---

## Documentation (`docs/`)

Structured documentation for different audiences.

```
docs/
├── architecture/               # Architecture documentation
│   ├── adrs/                   # Architecture Decision Records
│   ├── diagrams/               # System diagrams (C4, sequence, ERD)
│   ├── patterns/               # Design patterns used
│   └── design-guide/           # Architecture and design guide
│
├── api/                        # API documentation
│   ├── contracts/              # API contract definitions (OpenAPI/Swagger)
│   ├── schemas/                # JSON schemas
│   ├── examples/               # API usage examples
│   └── postman/                # Postman collections
│
├── deployment/                 # Deployment documentation
│   ├── runbooks/               # Operational runbooks
│   ├── playbooks/              # Incident response playbooks
│   ├── procedures/             # Standard operating procedures
│   └── checklists/             # Deployment and go-live checklists
│
├── user-guides/                # User documentation
│   ├── operators/              # Operator user guides
│   ├── administrators/         # Admin user guides
│   ├── compliance/             # Compliance officer guides
│   └── finance/                # Finance user guides
│
├── development/                # Developer documentation
│   ├── setup/                  # Development environment setup
│   ├── contributing/           # Contribution guidelines
│   ├── standards/              # Coding standards
│   └── style-guide/            # Code style guide
│
├── compliance/                 # Compliance documentation
│   ├── jurisdictions/          # State-by-state requirements
│   ├── label-rules/            # Labeling rules by jurisdiction
│   └── audit-trails/           # Audit trail specifications
│
└── training/                   # Training materials
    ├── sops/                   # Standard Operating Procedures
    ├── quizzes/                # Training quizzes
    └── certifications/         # Certification requirements
```

---

## Configuration (`config/`)

Environment-specific configuration files.

```
config/
├── dev/                        # Development environment config
├── staging/                    # Staging environment config
├── prod/                       # Production environment config
└── feature-flags/              # Feature flag definitions (Unleash)
```

---

## Design Principles Applied

### 1. **Bounded Contexts & Squad Alignment**

Services are organized by domain and aligned with squad ownership:

- **Core Platform** → identity, orgs, spatial, inventory, processing
- **Telemetry & Controls** → sensors, irrigation, environment, interlocks, closed-loop
- **Workflow & Messaging** → lifecycle, tasks, messaging, Slack
- **Integrations** → METRC, BioTrack, QBO, labeling
- **Data & AI** → analytics, AI models, sustainability, PdM

### 2. **Clean Architecture (Onion Architecture)**

Each service follows clean architecture layers:

- **API** (presentation) → Controllers, middleware, validators
- **Application** → Commands, queries, services, DTOs
- **Domain** (core) → Entities, value objects, domain events
- **Infrastructure** → Persistence, external APIs, messaging

### 3. **Single Responsibility Principle**

- Each folder has one clear purpose
- Services are small and focused (not "god services")
- Files are kept under 500 lines per coding rules

### 4. **Dependency Inversion**

- Domain layer has no external dependencies
- Application layer depends on domain abstractions
- Infrastructure implements interfaces defined in application layer

### 5. **Testability**

- Tests are organized by type (unit, integration, e2e, performance, security)
- Each service has its own test folder
- Shared test fixtures, helpers, and mocks

### 6. **Observability First**

- Dedicated observability folders for tracing, metrics, logging
- Structured logs and distributed tracing built into shared libraries
- Health checks and SLO monitoring

### 7. **Security by Design**

- RLS and ABAC policies in database layer
- Security utilities in shared library
- Security testing folder
- Secrets management infrastructure

### 8. **Scalability**

- Microservices architecture allows independent scaling
- Database partitioning and indexing strategies
- Caching and read replicas (analytics/reporting)
- Edge computing for device control

---

## Next Steps

1. **Initialize Services**: Use service generators to scaffold initial microservices
2. **Setup CI/CD**: Implement GitHub Actions workflows in `src/infrastructure/ci-cd/`
3. **Database Schema**: Define baseline schema in `src/database/schemas/`
4. **Shared Libraries**: Implement core abstractions in `src/shared/kernel/`
5. **Frontend Bootstrap**: Initialize Next.js app with base layouts and components
6. **Infrastructure Provisioning**: Setup dev/staging environments with Terraform
7. **Observability Stack**: Deploy Prometheus, Grafana, Loki, Tempo
8. **Documentation**: Populate documentation folders with ADRs, API contracts, runbooks

---

## References

- **Project Charter**: `documents/01_Project_Charter.md`
- **Squad Structure**: `documents/02_RACI_and_Squad_Structure.md`
- **Data Model**: `documents/08_Data_Model_Baseline_and_Migrations.md`
- **PRD**: `documents/Harvestry_ERP_Consolidated_PRD_v1.4.md`
- **ADRs**: `documents/23_ADR_*.md`
- **Coding Rules**: `.cursor/rules/coding-requirements.mdc`
