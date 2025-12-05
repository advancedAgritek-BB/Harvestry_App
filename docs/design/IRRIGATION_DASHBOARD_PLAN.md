# Irrigation Dashboard Design & Implementation Plan

## 1. Overview
This document outlines the design and implementation plan for the **Irrigation Management Dashboard**. While the existing *Cultivation Dashboard* provides high-level monitoring and "Quick Shot" capabilities, this dedicated dashboard serves as the comprehensive interface for the **FRP-06 Irrigation Orchestration System**.

## 2. Goals & Scope
The primary goal is to provide a user interface for:
1.  **Orchestration Management**: Creating and assigning Irrigation Programs and Schedules.
2.  **System Configuration**: Defining Irrigation Groups (Zones) and hardware mapping.
3.  **Advanced Operations**: Monitoring active runs, handling interlocks/alarms, and performing complex manual overrides.
4.  **Audit & History**: detailed logs of past irrigation runs and volume delivery.

## 3. Information Architecture

We propose adding a new top-level section or a sub-module under Dashboard: `/dashboard/irrigation`.

### 3.1. Main Views

| Route | View Name | Description |
| :--- | :--- | :--- |
| `/dashboard/irrigation` | **Overview** | High-level system status, active runs, tank levels, and safety interlock status. |
| `/dashboard/irrigation/programs` | **Program Manager** | CRUD interface for `IrrigationProgram` (Sequences of steps: Prime -> Shot -> Flush). |
| `/dashboard/irrigation/schedules` | **Scheduler** | Calendar or list view to map Programs to Groups via `IrrigationSchedule` (Time or Sensor triggers). |
| `/dashboard/irrigation/zones` | **Zone Configuration** | Admin view to manage `IrrigationGroup` definitions and valve assignments. |
| `/dashboard/irrigation/history` | **Run History** | Detailed logs of `IrrigationRun` execution, including interlock trip events. |

## 4. Feature Breakdown

### 4.1. Overview Dashboard
*   **System Health**: Traffic light indicators for the 7 Safety Interlocks (E-Stop, Door, Tank Level, EC/pH, etc.).
*   **Active Runs**: "Now Playing" card showing current active Programs, progress bar for current Step, and ETA.
*   **Tank Status**: Visual representation of Mix Tanks (Level, EC, pH, Temp).

### 4.2. Program Editor (The "Brain")
*   **Structure**: A Program is a sequence of Steps.
*   **UI Pattern**: List/Detail view.
*   **Step Types**:
    *   **Standard Shot**: Volume (mL) or Duration (sec).
    *   **Cycle & Soak**: Repeat X times with Y delay.
    *   **Flush**: Water only (no nutrients).
*   **Visual Builder**: Drag-and-drop reordering of steps.

### 4.3. Scheduler
*   **Triggers**:
    *   **Time-Based**: "Daily at 08:00".
    *   **Sensor-Based**: "If VWC < 35% AND Time is between 08:00-20:00" (Future/Advanced).
*   **Visualization**: Weekly calendar grid showing scheduled events.

### 4.4. Zone/Group Config
*   **Concept**: Grouping physical valves into logical "Zones" (e.g., "Flower Room 1 - Bench A").
*   **Hardware Map**: Assigning specific PLC/IO points to a Group.

## 5. UI/UX Design Patterns
*   **Consistency**: Reusing the `CultivationLayout` or a similar 12-column grid.
*   **Components**:
    *   Reuse `SidebarWidgets` for Alerts.
    *   Reuse `EnvironmentalTrends` components for history visualization.
*   **Theme**: Dark mode, consistent with Cultivation Dashboard. High-contrast safety indicators (Red/Green/Amber).

## 6. Integration with FRP-06
This frontend will consume the API endpoints defined in the FRP-06 backend implementation:
*   `GET /api/irrigation/groups`
*   `GET /api/irrigation/programs`
*   `POST /api/irrigation/commands/run` (to trigger manual programs)
*   `GET /api/irrigation/interlocks` (Real-time status)

## 7. Implementation Phases
1.  **Scaffold**: Create routes and layout.
2.  **Read-Only Views**: List Groups, Programs, and History.
3.  **Program/Schedule Editors**: Forms for creating/updating logic.
4.  **Live Control**: WebSocket integration for "Active Runs" and Interlock status.
5.  **Visual Polish**: Tank visualizations and Calendar view.





