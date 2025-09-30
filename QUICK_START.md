# Harvestry ERP & Control - Quick Start Guide

**Version:** 1.0  
**Date:** 2025-09-29

---

## Welcome to Harvestry! 👋

This guide will help you navigate the folder structure and get started with development.

---

## 📁 Folder Structure Overview

The project is organized into clear, logical sections:

### **`src/`** - All Source Code

- **`backend/services/`** - Microservices organized by bounded context
  - `core-platform/` - Identity, orgs, spatial, inventory, processing
  - `telemetry-controls/` - Sensors, irrigation, environment, closed-loop control
  - `workflow-messaging/` - Lifecycle, tasks, messaging, Slack bridge
  - `integrations/` - METRC, BioTrack, QuickBooks, labeling
  - `data-ai/` - Analytics, AI models, sustainability, predictive maintenance
  - `gateway/` - API Gateway for routing and authentication

- **`frontend/`** - Next.js web application
  - `app/` - Next.js App Router pages
  - `components/` - Reusable UI components
  - `features/` - Feature-specific modules
  - `hooks/` - Custom React hooks
  - `services/` - API clients and services
  - `stores/` - State management

- **`shared/`** - Shared libraries across services
  - `kernel/` - Core abstractions and base classes
  - `messaging/` - Outbox, sagas, event bus
  - `security/` - Authentication, authorization, RLS, ABAC
  - `observability/` - Tracing, metrics, logging, health checks
  - `data-access/` - Repository, UnitOfWork, specifications

- **`database/`** - Database schemas and migrations
  - `schemas/` - Domain-organized schemas
  - `migrations/` - Versioned migrations (TimescaleDB, ClickHouse)
  - `functions/`, `views/`, `triggers/` - Database objects
  - `RLS-policies/` - Row-level security policies

- **`infrastructure/`** - Infrastructure as Code
  - `docker/` - Dockerfiles and compose files
  - `kubernetes/` - K8s manifests
  - `terraform/` - Infrastructure provisioning
  - `monitoring/` - Prometheus, Grafana dashboards
  - `ci-cd/` - CI/CD pipelines

- **`edge/`** - Edge controllers and firmware
  - `controllers/` - Edge controller applications
  - `adapters/` - Protocol adapters (MQTT, SDI-12, Modbus)
  - `firmware/` - Firmware source code
  - `ota/` - Over-the-air update system

### **`tests/`** - All Testing Code

Organized by test type: `unit/`, `integration/`, `e2e/`, `performance/`, `security/`, `load/`, `chaos/`

### **`scripts/`** - Operational Scripts

- `dev/` - Development setup and utilities
- `deploy/` - Deployment and rollback scripts
- `db/` - Database operations
- `backup/`, `restore/` - Backup and restore
- `monitoring/` - Health checks and monitoring

### **`tools/`** - Development Tools

- `cli/` - CLI tool for developers
- `generators/` - Code generators (service, component, migration)
- `validators/` - Schema and config validators
- `importers/`, `exporters/` - Data import/export tools

### **`docs/`** - Documentation

- `architecture/` - ADRs, diagrams, design guide
- `api/` - API contracts (OpenAPI/Swagger)
- `deployment/` - Runbooks and deployment guides
- `user-guides/` - End-user documentation
- `development/` - Developer setup and contributing

### **`config/`**, **`data/`**, **`logs/`**

Environment configs, data files, and application logs

---

## 🏗️ Architecture Principles

### 1. **Microservices by Bounded Context**

Each service owns a specific domain and is independently deployable.

### 2. **Clean Architecture (Layers)**

Every service follows this structure:

```
service-name/
├── API/            # Controllers, middleware (presentation)
├── Application/    # Commands, queries, services (use cases)
├── Domain/         # Entities, value objects, business logic (core)
└── Infrastructure/ # Persistence, external APIs, messaging
```

### 3. **Squad Alignment**

Services are organized by squad ownership (see `documents/02_RACI_and_Squad_Structure.md`)

### 4. **SOLID & DRY**

- Single Responsibility: Each file/class does one thing
- Files < 500 lines (rule enforced)
- Shared logic in `src/shared/`

---

## 🚀 Getting Started

### Step 1: Review Documentation

Start with these key documents in the `documents/` folder:

1. **`01_Project_Charter.md`** - Project goals and scope
2. **`Harvestry_ERP_Consolidated_PRD_v1.4.md`** - Full product requirements
3. **`02_RACI_and_Squad_Structure.md`** - Team organization
4. **`08_Data_Model_Baseline_and_Migrations.md`** - Database entities
5. **`23_ADR_*.md`** - Architecture decision records

### Step 2: Understand Your Squad

Identify which squad you're on and which services you'll work with:

- **Core Platform** → `src/backend/services/core-platform/`
- **Telemetry & Controls** → `src/backend/services/telemetry-controls/`
- **Workflow & Messaging** → `src/backend/services/workflow-messaging/`
- **Integrations** → `src/backend/services/integrations/`
- **Data & AI** → `src/backend/services/data-ai/`
- **DevOps/SRE** → `src/infrastructure/`

### Step 3: Setup Development Environment

```bash
# 1. Install dependencies (TBD: will be automated)
# 2. Setup local database
cd scripts/dev
./setup.sh

# 3. Run migrations
cd scripts/db
./migrate.sh

# 4. Seed development data
./seed.sh

# 5. Start services (development mode)
cd scripts/dev
./run-services.sh
```

### Step 4: Explore a Service

Let's explore the **Identity** service as an example:

```
src/backend/services/core-platform/identity/
├── API/
│   ├── Controllers/        # AuthController, UserController
│   ├── Middleware/         # JWT validation, RLS enforcement
│   └── Validators/         # Input validation
├── Application/
│   ├── Commands/           # LoginCommand, CreateUserCommand
│   ├── Queries/            # GetUserQuery, GetRolesQuery
│   ├── Services/           # AuthService, TokenService
│   ├── DTOs/               # UserDto, LoginDto
│   └── Interfaces/         # IAuthService, ITokenService
├── Domain/
│   ├── Entities/           # User, Role, Badge
│   ├── ValueObjects/       # Email, BadgeCredential
│   ├── Enums/              # UserStatus, RoleType
│   └── Events/             # UserCreatedEvent, UserLoggedInEvent
├── Infrastructure/
│   ├── Persistence/        # UserRepository, DbContext
│   ├── External/           # External auth providers (SAML, OAuth)
│   └── Messaging/          # Event publishers
└── Tests/
    ├── Unit/               # Domain and application logic tests
    └── Integration/        # API and database tests
```

### Step 5: Frontend Development

Navigate to the frontend structure:

```
src/frontend/
├── app/                    # Next.js pages (App Router)
├── components/             # Reusable UI components
├── features/               # Feature modules (one per domain)
├── hooks/                  # Custom hooks (useAuth, useTelemetry)
├── services/               # API clients
└── stores/                 # State management (Zustand/Redux)
```

Example: Working on the cultivation dashboard:

- Page: `src/frontend/app/cultivation/page.tsx`
- Components: `src/frontend/components/charts/`, `src/frontend/components/tables/`
- Feature logic: `src/frontend/features/telemetry/`, `src/frontend/features/irrigation/`

---

## 🧪 Testing

### Running Tests

```bash
# Unit tests
npm run test:unit

# Integration tests
npm run test:integration

# E2E tests
npm run test:e2e

# Performance tests
npm run test:performance
```

### Writing Tests

- **Unit tests**: Test domain logic, value objects, services in isolation
- **Integration tests**: Test API endpoints, database interactions, messaging
- **E2E tests**: Test full user flows (Playwright/Cypress)

Test files live in:

- Service-specific: `src/backend/services/<service>/Tests/`
- Cross-service: `tests/integration/`
- User flows: `tests/e2e/`

---

## 📊 Observability

### Monitoring Stack

- **Prometheus** - Metrics collection
- **Grafana** - Dashboards and visualization
- **Loki** - Log aggregation
- **Tempo** - Distributed tracing
- **Sentry** - Error tracking

Dashboards and configs: `src/infrastructure/monitoring/`

### Health Checks

Every service exposes:

- `/health` - Liveness probe
- `/health/ready` - Readiness probe
- `/metrics` - Prometheus metrics

---

## 🔐 Security

### Row-Level Security (RLS)

All data is **site-scoped** by default. RLS policies are in:
`src/database/RLS-policies/site-scoped/`

### Attribute-Based Access Control (ABAC)

High-risk actions (destruction, closed-loop enable) use ABAC policies:
`src/database/RLS-policies/ABAC/`

### Secrets Management

- Development: `.env` files (never commit!)
- Staging/Prod: Vault/KMS
- Configuration: `src/infrastructure/secrets/`

---

## 🛠️ Common Tasks

### Create a New Service

```bash
# Use the service generator
cd tools/generators/service
./generate.sh --name my-service --squad core-platform
```

### Create a New Database Migration

```bash
cd scripts/db
./create-migration.sh --name add_user_preferences
```

### Add a New Frontend Component

```bash
cd tools/generators/component
./generate.sh --name MyComponent --type common
```

### Deploy to Staging

```bash
cd scripts/deploy
./deploy-staging.sh --service identity --version 1.2.3
```

---

## 📖 Key Concepts

### Outbox Pattern

All side effects (messages, external API calls) go through the transactional outbox to ensure exactly-once delivery. See `src/shared/messaging/Outbox/`

### Sagas

Multi-step operations (e.g., compliance sync + inventory update) use sagas for orchestration. See `src/shared/messaging/Sagas/`

### Feature Flags

High-risk features are gated by site-level feature flags:

- `closed_loop_ecph_enabled`
- `autosteer_mpc_enabled`
- `ai_auto_apply_enabled`

Configuration: `config/feature-flags/`

### Telemetry Ingest

Device telemetry flows:

1. Device → MQTT/HTTP Adapter
2. Adapter → Normalizer
3. Normalizer → Queue (NATS/Kafka)
4. Writer → PostgreSQL + TimescaleDB
5. Trigger → ClickHouse (for analytics)

See `src/backend/services/telemetry-controls/sensors/`

---

## 🔗 Useful Links

- **Project Charter**: `documents/01_Project_Charter.md`
- **PRD**: `documents/Harvestry_ERP_Consolidated_PRD_v1.4.md`
- **Data Model**: `documents/08_Data_Model_Baseline_and_Migrations.md`
- **ADRs**: `documents/23_ADR_*.md`
- **Folder Structure**: `FOLDER_STRUCTURE.md`
- **Coding Rules**: `.cursor/rules/coding-requirements.mdc`

---

## 🤝 Contributing

### Before You Code

1. Review the **Definition of Ready** and **Definition of Done** in `docs/development/standards/`
2. Ensure your IDE is configured with:
   - Linter (ESLint for frontend, analyzer for backend)
   - Formatter (Prettier for frontend, standard formatter for backend)
   - Pre-commit hooks (run linter + tests)

### Code Review Checklist

- [ ] Follows clean architecture principles
- [ ] Files are < 500 lines
- [ ] Single responsibility per class/function
- [ ] Tests added (unit + integration)
- [ ] RLS policies applied (for new database tables)
- [ ] Observability added (logs, metrics, traces)
- [ ] Documentation updated
- [ ] No linter warnings

---

## 🆘 Getting Help

- **Architecture questions**: See `docs/architecture/` or ask in #arch-council Slack
- **Squad-specific questions**: See `documents/02_RACI_and_Squad_Structure.md` for squad leads
- **Development issues**: Ask in #dev-help Slack channel
- **CI/CD issues**: Ask in #sre Slack channel

---

## 🎯 Next Steps

1. ✅ **You are here**: Understanding the folder structure
2. 🚧 **Next**: Setup your development environment (`scripts/dev/setup.sh`)
3. 📖 **Then**: Read the PRD for your squad's domain
4. 💻 **Finally**: Pick up your first ticket and start coding!

---

**Happy Coding! 🚀**
