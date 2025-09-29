# Harvestry ERP & Control

> **Enterprise-grade Cultivation OS** unifying ERP, compliance, and prescriptive control

[![License](https://img.shields.io/badge/license-Proprietary-red)](LICENSE)
![Version](https://img.shields.io/badge/version-1.0.0-blue)
![Status](https://img.shields.io/badge/status-In%20Development-yellow)

---

## 🌱 Overview

**Harvestry** is a modern, automation-first Cultivation Operating System that delivers:

- **Operational Excellence**: Deterministic tasking, safe automations, rapid exception handling
- **Data-Driven Cultivation**: Real-time telemetry, precision irrigation, closed-loop crop steering
- **Compliance & Auditability**: Regulator-ready traceability, COA gating, jurisdictional label rules
- **Financial Integrity**: QuickBooks integration with item-level and GL summary pathways
- **Enterprise NFRs**: Performance SLOs, disaster recovery, observability, RLS/ABAC security

---

## 🏗️ Architecture

### System Components

- **Backend Microservices** (.NET Core) - Bounded contexts aligned to squad ownership
- **Frontend** (Next.js) - Modern, responsive web application
- **Database** (PostgreSQL + TimescaleDB + ClickHouse) - Hot store + OLAP analytics sidecar
- **Edge Controllers** - IoT device control with safety interlocks
- **Integrations** - METRC, BioTrack, QuickBooks, Slack
- **Observability Stack** - Prometheus, Grafana, Loki, Tempo, Sentry

### Architectural Principles

✅ **Clean Architecture** - Separation of concerns with dependency inversion  
✅ **Microservices** - Independently deployable, scalable services  
✅ **Event-Driven** - Outbox pattern, sagas, domain events  
✅ **Security by Design** - Row-level security (RLS), ABAC, audit trails  
✅ **Observability First** - Distributed tracing, structured logging, SLO monitoring  
✅ **SOLID & DRY** - Modular, testable, maintainable code  

---

## 📁 Project Structure

```text
Harvestry_App/
├── src/
│   ├── backend/         # Microservices by bounded context
│   ├── frontend/        # Next.js web application
│   ├── shared/          # Shared libraries (kernel, messaging, security)
│   ├── infrastructure/  # IaC, Docker, Kubernetes, Terraform
│   ├── database/        # Schemas, migrations, functions, RLS policies
│   └── edge/            # Edge controllers, firmware, IoT adapters
├── tests/               # Unit, integration, e2e, performance, security
├── scripts/             # Development, deployment, operations
├── tools/               # CLI, generators, validators, analyzers
├── docs/                # Architecture, API, runbooks, user guides
├── config/              # Environment-specific configurations
└── documents/           # Product requirements, PRDs, ADRs
```

**📖 For detailed folder structure**: See [`FOLDER_STRUCTURE.md`](FOLDER_STRUCTURE.md)

---

## 🚀 Quick Start

### Prerequisites

- **.NET 8 SDK** (backend services)
- **Node.js 20+** (frontend)
- **Docker & Docker Compose** (local development)
- **PostgreSQL 15 + TimescaleDB** (database)
- **Git** (version control)

### Getting Started

```bash
# 1. Clone the repository
git clone https://github.com/your-org/Harvestry_App.git
cd Harvestry_App

# 2. Setup development environment
./scripts/dev/setup.sh

# 3. Run database migrations
./scripts/db/migrate.sh

# 4. Seed development data
./scripts/db/seed.sh

# 5. Start all services
./scripts/dev/run-services.sh

# 6. Access the application
# Frontend: http://localhost:3000
# API Gateway: http://localhost:5000
# Grafana: http://localhost:3001
```

**📖 For detailed setup instructions**: See [`QUICK_START.md`](QUICK_START.md)

---

## 📚 Documentation

### For Developers

- **[Quick Start Guide](QUICK_START.md)** - Get up and running quickly
- **[Folder Structure](FOLDER_STRUCTURE.md)** - Understand the codebase organization
- **[Development Standards](docs/development/standards/)** - Coding standards and best practices
- **[API Documentation](docs/api/)** - API contracts and examples

### For Product & Business

- **[Project Charter](documents/01_Project_Charter.md)** - Project goals and scope
- **[Product Requirements](documents/Harvestry_ERP_Consolidated_PRD_v1.4.md)** - Full PRD
- **[Squad Structure](documents/02_RACI_and_Squad_Structure.md)** - Team organization

### For Architecture

- **[Architecture Decision Records](documents/23_ADR_*.md)** - Key architectural decisions
- **[Data Model](documents/08_Data_Model_Baseline_and_Migrations.md)** - Database schema
- **[ERD](documents/28_ERD.md)** - Entity-relationship diagram

### For Operations

- **[Runbooks](documents/26_Runbooks/)** - Operational procedures
- **[Deployment Guide](docs/deployment/)** - Deployment and rollback procedures
- **[Observability](documents/06_Observability_and_SLOs.md)** - Monitoring and SLOs
- **[DR Plan](documents/07_DR_and_Backup_Plan.md)** - Disaster recovery

---

## 🧱 Microservices

### Core Platform Squad
- **Identity** - Authentication, authorization, RLS, ABAC, badges
- **Organizations** - Orgs, sites, roles, teams
- **Spatial** - Sites, rooms, zones, racks, bins, equipment
- **Inventory** - Lots, movements, labels, GS1/UDI
- **Processing** - Manufacturing, labor, waste tracking

### Telemetry & Controls Squad
- **Sensors** - Telemetry ingest, sensor streams, rollups
- **Irrigation-Fertigation** - Programs, schedules, recipes, mix tanks
- **Environment** - Air, canopy, substrate monitoring, alerts
- **Interlocks** - Safety interlocks, e-stop, curfews
- **Closed-Loop** - EC/pH control, autosteer MPC, crop steering

### Workflow & Messaging Squad
- **Lifecycle** - Batch lifecycle, state transitions, blueprints
- **Tasks** - Task management, dependencies, approvals
- **Messaging** - In-app notifications, escalations
- **Slack-Bridge** - Slack integration, mirroring, slash commands

### Integrations Squad
- **Compliance-METRC** - METRC state compliance integration
- **Compliance-BioTrack** - BioTrack state compliance integration
- **QuickBooks** - QBO item-level and GL summary sync
- **Labeling** - GS1, UDI, jurisdiction-specific labels

### Data & AI Squad
- **Analytics** - KPIs, rollups, reports, dashboards
- **AI-Models** - Anomaly detection, yield prediction, ET0, Copilot
- **Sustainability** - WUE, NUE, kWh/gram, ESG reporting
- **Predictive-Maintenance** - Equipment failure prediction, drift detection

---

## 🧪 Testing

### Test Strategy

- **Unit Tests** - Domain logic, services, value objects
- **Integration Tests** - API endpoints, database, messaging, external APIs
- **E2E Tests** - Full user flows (Playwright/Cypress)
- **Performance Tests** - SLO validation (p95, p99)
- **Security Tests** - Penetration, fuzz testing, RLS/ABAC validation
- **Load Tests** - Stress, capacity, endurance testing
- **Chaos Tests** - Network partition, service failure, database failover

### Running Tests

```bash
# Run all tests
npm run test

# Run specific test suites
npm run test:unit
npm run test:integration
npm run test:e2e

# Run performance tests
npm run test:performance

# Run security tests
npm run test:security
```

**Coverage Target**: 80% overall, 90% for critical paths

---

## 🔐 Security

### Security Model

- **Row-Level Security (RLS)**: All data is site-scoped by default
- **Attribute-Based Access Control (ABAC)**: High-risk actions require additional permissions
- **Audit Trail**: Tamper-evident hash chain with nightly verification
- **Secrets Management**: Vault/KMS for production, encrypted at rest
- **Token Rotation**: Automated credential rotation
- **DPIA/PIA**: Data protection impact assessments for PII features

### Compliance

- **METRC Integration**: State-compliant cannabis traceability
- **BioTrack Integration**: Alternative state compliance system
- **COA Gating**: Certificate of Analysis enforcement
- **Label Rules**: Jurisdiction-specific labeling requirements
- **Audit Exports**: Regulator-ready audit trail exports

---

## 📊 Observability

### Monitoring Stack

- **Prometheus** - Metrics collection and alerting
- **Grafana** - Dashboards and visualization
- **Loki** - Log aggregation and querying
- **Tempo** - Distributed tracing
- **Sentry** - Error tracking and alerting

### Key Metrics

- **Telemetry Ingest**: p95 < 1.0s, p99 < 2.5s
- **Realtime Push**: p95 < 1.5s
- **Command Dispatch**: p95 < 800ms, p99 < 1.8s
- **Task Round-Trip**: p95 < 300ms
- **Availability**: 99.9% monthly uptime

### Dashboards

- System health and SLOs
- Service-level metrics
- Business KPIs (yield, throughput, COGS)
- Compliance and audit metrics

---

## 🛠️ Development Tools

### CLI Tool

```bash
# Generate new service
harvestry generate service --name my-service --squad core-platform

# Generate component
harvestry generate component --name MyComponent --type common

# Create migration
harvestry db migration --name add_user_preferences

# Validate configuration
harvestry validate config --env staging

# Import legacy data
harvestry import --source metrc --file data.csv
```

### Code Generators

- Service scaffold generator
- Component generator (React)
- Database migration generator
- Test scaffold generator

---

## 🚢 Deployment

### Environments

- **Development** - Local development with Docker Compose
- **Staging** - Pre-production environment for testing
- **Production** - Live production environment with DR

### Deployment Strategy

- **Blue-Green Deployment** - Zero-downtime deployments
- **Feature Flags** - Gradual rollout of high-risk features
- **Rollback Plan** - Automated rollback on health check failure
- **DR Drills** - Quarterly disaster recovery testing

### CI/CD Pipeline

1. **Build** - Compile, lint, unit tests
2. **Test** - Integration tests, security scans
3. **Deploy to Staging** - Automated staging deployment
4. **E2E Tests** - Full user flow validation
5. **Deploy to Production** - Manual approval + blue-green deploy
6. **Smoke Tests** - Post-deployment validation
7. **Monitor** - SLO validation and alerting

---

## 🤝 Contributing

### Getting Involved

1. Read the [Quick Start Guide](QUICK_START.md)
2. Review [Development Standards](docs/development/standards/)
3. Pick up a ticket from your squad's backlog
4. Create a feature branch: `git checkout -b feature/my-feature`
5. Make changes following coding standards
6. Write tests (unit + integration)
7. Submit a pull request

### Code Review Checklist

- [ ] Follows clean architecture principles
- [ ] Files are < 500 lines
- [ ] Single responsibility per class/function
- [ ] Tests added (unit + integration)
- [ ] RLS policies applied (for new tables)
- [ ] Observability added (logs, metrics, traces)
- [ ] Documentation updated
- [ ] No linter warnings

---

## 📅 Release Roadmap

### MVP (Q1 2026)
- Identity/RLS, Spatial model, Genetics/Propagation
- Batch lifecycle, Tasks, Messaging (Slack notify-only)
- Telemetry ingest, Environment monitoring
- Irrigation open-loop, Interlocks
- Inventory, Labels, Processing basics
- Compliance queues, COA gating, Holds
- QBO item-level sync
- Reporting, Observability, DR

### Phase 2 (Q2 2026)
- Closed-loop EC/pH control
- Dryback auto-shots
- Slack two-way integration
- AI yield prediction
- GL summary JE
- Sustainability dashboards
- ET0 recommendations

### Phase 3 (Q3 2026)
- Autosteer MPC (climate + lighting)
- Copilot Ask-to-Act
- Vision baseline
- Cultivar Marketplace
- Predictive maintenance at scale
- Lab API integration
- Mobile offline support

---

## 📞 Support

### For Development Issues
- **Slack**: #dev-help
- **Email**: [dev-support@harvestry.com](mailto:dev-support@harvestry.com)

### For Architecture Questions
- **Slack**: #arch-council
- **Email**: [architecture@harvestry.com](mailto:architecture@harvestry.com)

### For Operations/SRE
- **Slack**: #sre
- **PagerDuty**: On-call rotation
- **Email**: [sre@harvestry.com](mailto:sre@harvestry.com)

---

## 📄 License

Proprietary - All Rights Reserved

Copyright © 2025 Harvestry, Inc.

---

## 🙏 Acknowledgments

Built with care by the Harvestry engineering team.

Special thanks to our design partners for their invaluable feedback during the pilot phase.

---

**🌱 Let's grow something great together!**
