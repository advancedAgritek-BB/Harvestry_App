# Planner Labor Navigation & Views

## Navigation Model
- Lives under `dashboard/planner` with in-section tabs: Home, Batch Planning, Shift Board, Time Approvals, Productivity, Settings.
- Context is shared by navigation; batch planning is now `/planner/batch-planning` and linked from Home + Shift Board.

## Home
- Coverage cards by phase/room, quick links to shift gaps and time approvals.
- Budget vs actual snapshot, productivity widgets, integration status, and employee compliance surface.

## Batch Planning (Gantt)
- Dedicated page keeps existing Gantt interaction for batches and phases.
- Planner toolbar remains for new batch + conflict count; detail panel persists.
- Intent: deep timeline work here; Home links directly into batches that need attention.

## Shift Board
- Board of shift needs with assign/swap/auto-fill actions; ties back to demand from Gantt.

## Time Approvals
- Review punches/exceptions, approve or request edits; feeds payroll export and compliance.

## Productivity
- Units/hour and efficiency metrics with short insights; telemetry correlation planned via hooks.

## Settings
- Integrations (HRIS/payroll/telemetry) status list; compliance rule inputs (OT, breaks, certification blocking).

## Backend Contracts (draft)
- Domain models added under `labor/Domain`: Employee, ShiftTemplate, ShiftAssignment, TimeEntry, SchedulingDemand, LaborBudget, ProductivityRecord plus enums.
- Application interfaces under `labor/Application/Interfaces` for employee, scheduling engine, timekeeping, costing, productivity, compliance, reporting, HRIS adapter, telemetry hook.
- DTO contracts in `labor/Application/DTOs/LaborContracts.cs` define request/response shapes for employees, shifts, time entries, and budgets.



