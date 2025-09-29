# Harvestry App - Visual Project Structure

**Created:** 2025-09-29  
**Total Directories:** 617

---

## 📊 High-Level Organization

```
Harvestry_App/
│
├── 🔧 src/                    → All source code
│   ├── backend/               → Backend microservices (.NET Core)
│   ├── frontend/              → Next.js web application
│   ├── shared/                → Shared libraries and utilities
│   ├── infrastructure/        → IaC, Docker, Kubernetes, monitoring
│   ├── database/              → Schemas, migrations, functions, RLS
│   └── edge/                  → IoT controllers, firmware, protocols
│
├── 🧪 tests/                  → All testing (unit, integration, e2e)
├── 📜 scripts/                → Dev, deploy, db, monitoring scripts
├── 🛠️  tools/                  → CLI, generators, validators, analyzers
├── 📚 docs/                   → Structured documentation
├── ⚙️  config/                 → Environment-specific configs
├── 💾 data/                   → Imports, exports, backups
├── 📋 logs/                   → Application, audit, security logs
└── 📖 documents/              → PRDs, ADRs, runbooks (existing)
```

---

## 🏢 Backend Microservices (by Squad)

```
src/backend/services/
│
├── 🟦 core-platform/          [Squad: Core Platform]
│   ├── identity/              → Auth, RLS, ABAC, badges, audit
│   ├── organizations/         → Orgs, sites, roles, teams
│   ├── spatial/               → Rooms, zones, racks, bins, equipment
│   ├── inventory/             → Lots, movements, labels, GS1/UDI
│   └── processing/            → Manufacturing, labor, waste
│
├── 🟩 telemetry-controls/     [Squad: Telemetry & Controls]
│   ├── sensors/               → Telemetry ingest, streams, rollups
│   ├── irrigation-fertigation/→ Programs, recipes, mix tanks, injectors
│   ├── environment/           → Air, canopy, substrate monitoring
│   ├── interlocks/            → Safety interlocks, e-stop, curfews
│   └── closed-loop/           → EC/pH control, autosteer MPC
│
├── 🟨 workflow-messaging/     [Squad: Workflow & Messaging]
│   ├── lifecycle/             → Batch lifecycle, state transitions
│   ├── tasks/                 → Task management, approvals, SLAs
│   ├── messaging/             → Notifications, escalations
│   └── slack-bridge/          → Slack integration, commands
│
├── 🟧 integrations/           [Squad: Integrations]
│   ├── compliance-metrc/      → METRC state compliance
│   ├── compliance-biotrack/   → BioTrack state compliance
│   ├── quickbooks/            → QBO item-level + GL summary
│   └── labeling/              → GS1, UDI, jurisdiction labels
│
├── 🟪 data-ai/                [Squad: Data & AI]
│   ├── analytics/             → KPIs, reports, dashboards, OLAP
│   ├── ai-models/             → Anomaly, yield, ET0, Copilot
│   ├── sustainability/        → WUE, NUE, kWh/gram, ESG
│   └── predictive-maintenance/→ Equipment failure prediction
│
└── 🔀 gateway/                [API Gateway]
    └── API/                   → Routing, rate limiting, auth
```

### Service Internal Structure (Clean Architecture)

Every service follows this pattern:

```
<service-name>/
├── API/                       [Presentation Layer]
│   ├── Controllers/           → REST endpoints
│   ├── Middleware/            → Request/response pipeline
│   └── Validators/            → Input validation
│
├── Application/               [Application Logic Layer]
│   ├── Commands/              → CQRS write operations
│   ├── Queries/               → CQRS read operations
│   ├── Services/              → Application services
│   ├── ViewModels/            → UI response models
│   ├── DTOs/                  → Data transfer objects
│   └── Interfaces/            → Application abstractions
│
├── Domain/                    [Business Logic Layer - CORE]
│   ├── Entities/              → Domain entities with business rules
│   ├── ValueObjects/          → Immutable value objects
│   ├── Enums/                 → Domain enumerations
│   ├── Events/                → Domain events
│   └── Specifications/        → Specification pattern
│
├── Infrastructure/            [External Dependencies Layer]
│   ├── Persistence/           → Database, repositories, EF Core
│   ├── External/              → Third-party APIs, adapters
│   ├── Messaging/             → Outbox, sagas, event bus
│   └── DeviceAdapters/        → IoT protocols (if applicable)
│
└── Tests/
    ├── Unit/                  → Domain/app logic tests
    └── Integration/           → API, database, messaging tests
```

---

## 🎨 Frontend Structure (Next.js)

```
src/frontend/
│
├── 📄 app/                    [Next.js App Router - Pages]
│   ├── api/                   → BFF API routes
│   ├── auth/                  → Login, register, SSO
│   ├── dashboard/             → Main dashboard
│   ├── cultivation/           → Batches, tasks, lifecycle
│   ├── inventory/             → Inventory, warehouse
│   ├── processing/            → Manufacturing
│   ├── compliance/            → METRC, BioTrack, COA
│   ├── analytics/             → Reports, KPIs
│   └── settings/              → Org, site, user settings
│
├── 🧩 components/             [Reusable UI Components]
│   ├── common/                → Buttons, inputs, cards
│   ├── forms/                 → Form components, validation
│   ├── charts/                → Data visualization (SciChart)
│   ├── tables/                → Data grids, sortable tables
│   ├── modals/                → Dialogs, confirmations
│   ├── navigation/            → Navbars, sidebars, breadcrumbs
│   └── alerts/                → Toasts, notifications
│
├── 🎯 features/               [Feature-Specific Modules]
│   ├── identity/              → Auth, user profile, badges
│   ├── spatial/               → Site, room, zone, equipment
│   ├── telemetry/             → Real-time sensor data, charts
│   ├── irrigation/            → Programs, schedules
│   ├── tasks/                 → Task lists, approvals
│   ├── messaging/             → In-app messaging, Slack
│   ├── compliance/            → METRC/BioTrack UI
│   ├── quickbooks/            → QBO sync, reconciliation
│   ├── ai/                    → Copilot, autosteer UI
│   └── sustainability/        → ESG dashboards
│
├── 🎭 layouts/                [Page Layouts]
│   ├── MainLayout/            → Default layout
│   ├── DashboardLayout/       → Dashboard-specific
│   └── AuthLayout/            → Auth pages (centered)
│
├── 🪝 hooks/                  [Custom React Hooks]
│   ├── data/                  → Data fetching (React Query)
│   ├── auth/                  → Auth state
│   ├── realtime/              → WebSocket/SignalR
│   └── device/                → Device state, commands
│
├── 🔌 services/               [Frontend Services]
│   ├── api/                   → API client
│   ├── auth/                  → Auth service, tokens
│   ├── realtime/              → Real-time connections
│   ├── notifications/         → Notification service
│   └── storage/               → Local/session storage
│
├── 📦 stores/                 [State Management]
│   ├── auth/                  → Auth state
│   ├── app/                   → Global app state
│   ├── telemetry/             → Telemetry cache
│   ├── tasks/                 → Task management
│   └── alerts/                → Notifications
│
├── 🏷️  types/                  [TypeScript Types]
│   ├── api/                   → API response types
│   ├── domain/                → Domain model types
│   └── ui/                    → UI component props
│
├── 🎨 styles/                 [Styling & Theming]
│   ├── tokens/                → Design tokens
│   ├── themes/                → Light/dark themes
│   └── components/            → Component styles
│
├── ⚙️  config/                 [Frontend Config]
│   ├── constants/             → App constants, enums
│   ├── environment/           → Environment variables
│   └── routes/                → Route definitions
│
└── 🖼️  public/                 [Static Assets]
```

---

## 🔄 Shared Libraries

```
src/shared/
│
├── 🧬 kernel/                 → Core abstractions, base classes
│   ├── Abstractions/          → IEntity, IAggregateRoot
│   ├── Domain/                → Base domain classes
│   ├── Events/                → Event bus, handlers
│   ├── Exceptions/            → Standard exception types
│   └── Results/               → Result pattern
│
├── 📨 messaging/              → Messaging infrastructure
│   ├── Outbox/                → Transactional outbox pattern
│   ├── Sagas/                 → Saga orchestration
│   ├── Publishers/            → Message publishers
│   ├── Consumers/             → Message consumers
│   └── Contracts/             → Message schemas
│
├── 🔐 security/               → Security utilities
│   ├── Authentication/        → JWT, token validation
│   ├── Authorization/         → Policy-based authz
│   ├── RLS/                   → Row-level security
│   ├── ABAC/                  → Attribute-based access control
│   └── Encryption/            → Encryption, hashing
│
├── 📊 observability/          → Observability components
│   ├── Tracing/               → OpenTelemetry tracing
│   ├── Metrics/               → Prometheus metrics
│   ├── Logging/               → Structured logging (Serilog)
│   └── HealthChecks/          → Health check endpoints
│
├── 💾 data-access/            → Data access patterns
│   ├── Repository/            → Generic repository
│   ├── UnitOfWork/            → Unit of work pattern
│   ├── Specifications/        → Specification pattern
│   └── Migrations/            → Migration utilities
│
├── ✅ validation/             → Validation framework
│   ├── Validators/            → FluentValidation
│   ├── Rules/                 → Custom validation rules
│   └── Extensions/            → Validation extensions
│
└── 🧰 utilities/              → General utilities
    ├── Helpers/               → Helper classes
    ├── Extensions/            → Extension methods
    ├── Converters/            → Type converters
    └── Formatters/            → Data formatters
```

---

## 🗄️ Database Structure

```
src/database/
│
├── 📐 schemas/                [Domain-Organized Schemas]
│   ├── identity/              → Users, roles, badges, sessions, audit
│   ├── spatial/               → Sites, rooms, zones, racks, bins
│   ├── equipment/             → Equipment, sensors, actuators, calibration
│   ├── cultivation/           → Genetics, batches, plants, movements
│   ├── tasking/               → Tasks, blueprints, approvals, delegations
│   ├── environment/           → Targets, overrides, thresholds, alerts
│   ├── irrigation/            → Programs, schedules, recipes, interlocks
│   ├── inventory/             → Lots, balances, movements, labels
│   ├── processing/            → Processes, runs, labor, waste
│   ├── compliance/            → METRC/BioTrack, COA, holds, destruction
│   ├── accounting/            → QBO integration, mappings, queue
│   ├── ai/                    → Predictions, feedback, ET0, sustainability
│   └── queues/                → Outbox, sync_queue, accounting_queue
│
├── 🔄 migrations/             [Database Migrations]
│   ├── timescale/             → TimescaleDB hypertables, aggregates
│   ├── clickhouse/            → ClickHouse tables, materialized views
│   └── baseline/              → Baseline schema, initial data
│
├── 🌱 seeds/                  [Seed Data]
│   ├── dev/                   → Development seed data
│   ├── staging/               → Staging seed data
│   └── test/                  → Test fixtures
│
├── ⚡ functions/              [Stored Functions]
│   ├── aggregate/             → Custom aggregates
│   ├── utility/               → Utility functions
│   └── audit/                 → Audit, hash chain
│
├── 👁️  views/                  [Database Views]
│   ├── materialized/          → Materialized views
│   ├── standard/              → Standard views
│   └── rollups/               → Telemetry rollups (1m, 5m, 1h)
│
├── ⚡ triggers/               [Database Triggers]
│   ├── audit/                 → Audit trail triggers
│   ├── clickhouse-sync/       → ClickHouse sidecar sync
│   └── outbox/                → Outbox pattern triggers
│
├── 🔍 indexes/                [Index Definitions]
│
├── 🔒 RLS-policies/           [Row-Level Security]
│   ├── site-scoped/           → Site-scoped RLS
│   ├── user-scoped/           → User-scoped RLS
│   └── ABAC/                  → Attribute-based access control
│
└── 🛠️  scripts/               [DB Utility Scripts]
```

---

## 🏗️ Infrastructure (DevOps)

```
src/infrastructure/
│
├── 🐳 docker/                 → Docker containerization
│   ├── services/              → Service Dockerfiles
│   ├── compose/               → Docker Compose files
│   └── dockerfiles/           → Shared Dockerfiles
│
├── ☸️  kubernetes/             → Kubernetes manifests
│   ├── deployments/           → Deployments
│   ├── services/              → Services
│   ├── configmaps/            → ConfigMaps
│   ├── secrets/               → Secrets (sealed)
│   ├── ingress/               → Ingress rules
│   ├── jobs/                  → Jobs
│   └── cron/                  → CronJobs
│
├── 🏗️  terraform/              → Terraform IaC
│   ├── modules/               → Reusable modules
│   ├── environments/          → Dev, staging, prod
│   └── providers/             → Cloud providers
│
├── ⎈  helm/                   → Helm charts
│   ├── charts/                → Chart definitions
│   └── values/                → Environment values
│
├── 📊 monitoring/             → Prometheus + Grafana
│   ├── prometheus/            → Prometheus config
│   ├── grafana/               → Grafana setup
│   ├── dashboards/            → Dashboard JSONs
│   └── alerts/                → Alert rules (SLO burn rates)
│
├── 📋 logging/                → Logging infrastructure
│   ├── loki/                  → Loki config
│   ├── fluentd/               → Fluentd/Fluent Bit
│   └── configs/               → Log aggregation
│
├── 🔍 tracing/                → Distributed tracing
│   ├── tempo/                 → Tempo config
│   ├── jaeger/                → Jaeger (alternative)
│   └── configs/               → Tracing configs
│
├── 🚨 alerting/               → Alerting infrastructure
│   ├── rules/                 → Alert rules
│   ├── receivers/             → Slack, email, SMS
│   └── policies/              → Routing policies
│
├── 🌐 service-mesh/           → Service mesh (optional)
│   ├── istio/                 → Istio configs
│   ├── linkerd/               → Linkerd configs
│   └── configs/               → Mesh policies
│
├── 🔑 secrets/                → Secrets management
│   ├── vault/                 → HashiCorp Vault
│   ├── kms/                   → Cloud KMS
│   └── policies/              → Access policies
│
├── 🔄 ci-cd/                  → CI/CD pipelines
│   ├── github-actions/        → GitHub Actions
│   ├── jenkins/               → Jenkins pipelines
│   └── scripts/               → Deployment scripts
│
└── 🛠️  scripts/               → Automation scripts
```

---

## 🤖 Edge & Firmware

```
src/edge/
│
├── 🎛️  controllers/            → Edge controller apps
│   ├── main-controller/       → Primary cultivation controller
│   ├── sensor-nodes/          → Distributed sensor nodes
│   ├── actuator-nodes/        → Actuator control nodes
│   └── safety-interlocks/     → Safety interlock controllers
│
├── 🔌 adapters/               → Device protocol adapters
│   ├── mqtt/                  → MQTT client/broker
│   ├── sdi-12/                → SDI-12 protocol
│   ├── http/                  → HTTP REST adapter
│   ├── modbus/                → Modbus RTU/TCP
│   └── bacnet/                → BACnet (HVAC)
│
├── 💾 firmware/               → Firmware source
│   ├── bootloader/            → Secure boot
│   ├── app/                   → Application firmware
│   ├── drivers/               → Hardware drivers
│   └── hal/                   → Hardware abstraction layer
│
├── 📡 protocols/              → Communication protocols
│   ├── command-dispatch/      → Command dispatch
│   ├── telemetry-ingest/      → Telemetry ingestion
│   ├── heartbeat/             → Device heartbeat
│   └── device-discovery/      → Discovery, registration
│
├── 📝 device-registry/        → Device management
│   ├── provisioning/          → Device provisioning
│   ├── enrollment/            → Zero-touch enrollment
│   └── revocation/            → Decommissioning
│
└── 🔄 ota/                    → Over-the-air updates
    ├── updates/               → Update packages
    ├── rollback/              → Rollback mechanisms
    └── verification/          → Update verification
```

---

## 🧪 Testing Structure

```
tests/
│
├── 🧪 unit/                   → Unit tests (fast, isolated)
│   ├── backend/               → Backend service units
│   ├── frontend/              → Frontend component units
│   └── shared/                → Shared library units
│
├── 🔗 integration/            → Integration tests (external deps)
│   ├── services/              → Service-to-service
│   ├── database/              → Database integration
│   ├── messaging/             → Message bus integration
│   └── external-apis/         → METRC, QBO, Slack
│
├── 🎭 e2e/                    → End-to-end tests (full flows)
│   ├── user-flows/            → User journeys (Playwright)
│   ├── critical-paths/        → Business-critical flows
│   └── regression/            → Regression suites
│
├── ⚡ performance/            → Performance testing
│   ├── benchmarks/            → Performance benchmarks
│   ├── profiling/             → Profiling results
│   └── slos/                  → SLO validation (p95, p99)
│
├── 🔐 security/               → Security testing
│   ├── penetration/           → Penetration tests
│   ├── fuzz/                  → Fuzz testing
│   ├── rls-abac/              → RLS/ABAC policy tests
│   └── secrets/               → Secret exposure tests
│
├── 📈 load/                   → Load testing
│   ├── stress/                → Stress tests (breaking point)
│   ├── capacity/              → Capacity planning
│   └── endurance/             → Endurance/soak tests
│
├── 💥 chaos/                  → Chaos engineering
│   ├── network-partition/     → Network failures
│   ├── service-failure/       → Service crashes
│   └── database-failover/     → DR failover tests
│
├── 📦 fixtures/               → Test fixtures
├── 🛠️  helpers/                → Test helpers
└── 🎭 mocks/                  → Mock objects
```

---

## 📜 Scripts & 🛠️ Tools

### Scripts

```
scripts/
├── dev/            → setup.sh, run-services.sh, reset-db.sh
├── deploy/         → blue-green.sh, rollback.sh, smoke-test.sh
├── db/             → migrate.sh, seed.sh, optimize.sh
├── backup/         → db-backup.sh, verify-backup.sh
├── restore/        → db-restore.sh
├── monitoring/     → health-check.sh, burn-rate.sh
├── security/       → rotate-secrets.sh, scan.sh, audit.sh
└── utilities/      → generate-docs.sh, clean.sh
```

### Tools

```
tools/
├── cli/            → Developer CLI (commands, templates)
├── generators/     → Service, component, migration, test generators
├── validators/     → Schema, config, API contract validators
├── analyzers/      → Code quality, dependencies, security scans
├── importers/      → Legacy data, METRC, BioTrack, QBO imports
└── exporters/      → Compliance, financial, audit exports
```

---

## 📚 Documentation

```
docs/
├── architecture/       → ADRs, diagrams, patterns, design guide
├── api/                → OpenAPI contracts, schemas, examples
├── deployment/         → Runbooks, playbooks, checklists
├── user-guides/        → Operators, admins, compliance, finance
├── development/        → Setup, contributing, standards
├── compliance/         → Jurisdictions, label rules, audits
└── training/           → SOPs, quizzes, certifications
```

---

## 🎯 Key Design Principles

### ✅ Bounded Contexts & Squad Alignment
Services map to squads and domain boundaries

### ✅ Clean Architecture (Onion)
API → Application → Domain ← Infrastructure

### ✅ Single Responsibility
Each file/class does one thing, < 500 lines

### ✅ Dependency Inversion
Domain has no external dependencies

### ✅ Testability
Tests organized by type, co-located with services

### ✅ Observability First
Tracing, metrics, logging built-in

### ✅ Security by Design
RLS, ABAC, audit trails, secrets management

### ✅ Scalability
Microservices, partitioning, caching, edge computing

---

## 📈 Statistics

- **Total Directories**: 617
- **Backend Services**: 6 bounded contexts, 20+ microservices
- **Frontend Features**: 10+ feature modules
- **Shared Libraries**: 7 cross-cutting libraries
- **Database Schemas**: 13 domain schemas
- **Test Categories**: 7 test types
- **Infrastructure Components**: 11 infrastructure areas

---

## 📖 Quick Links

- **[README](README.md)** - Project overview
- **[Quick Start](QUICK_START.md)** - Get started quickly
- **[Folder Structure](FOLDER_STRUCTURE.md)** - Detailed structure documentation
- **[PRD](documents/Harvestry_ERP_Consolidated_PRD_v1.4.md)** - Product requirements
- **[Project Charter](documents/01_Project_Charter.md)** - Project goals

---

**Created with ❤️ for the Harvestry team**
