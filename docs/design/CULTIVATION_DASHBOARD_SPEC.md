# Cultivation Dashboard Design Specification

## 1. Overview
This document outlines the design and technical specification for the "Cultivation Dashboard". The dashboard provides a comprehensive, real-time view of a **single cultivation room's** environment, irrigation status, and operational alerts. It is high-density and data-rich, targeted at facility managers and growers.

## 2. Layout Strategy
The layout utilizes a **12-column CSS Grid** system to accommodate the dense information hierarchy.

*   **Global Container**: `flex flex-col h-screen bg-background text-foreground`
*   **Grid System**: `grid grid-cols-12 gap-4 p-4`

### Areas
1.  **Header (App Level)**: Navigation, Search, Context Selectors (Site/Room/Range).
2.  **Metrics Row (Top)**: Spans full width (12 cols). Contains 5 key metric cards.
3.  **Main Content (Left - 9 cols)**:
    *   Environmental Trends Chart (Full width of this section).
    *   Split Row: Irrigation Windows (approx 60%) + Zone Heatmap (approx 40%).
    *   Rooms Footer (Full width of this section).
4.  **Sidebar (Right - 3 cols)**:
    *   Active Alerts (Top).
    *   Targets vs Current (Middle).
    *   Quick Actions (Bottom).

## 3. Component Breakdown & Behaviors

### 3.1. Key Metrics Row
*   **Location**: Top Row.
*   **Components**: 5 Independent Cards.
*   **Behaviors**:
    1.  **Temperature**: Current Value, VPD.
    2.  **RH (Relative Humidity)**: Current %, DL.
    3.  **DLI / PPFD**: Toggle between DLI (accumulated) and PPFD (instant). "Tap to view PPFD" triggers switch.
    4.  **CO₂**: Current ppm, Band range (e.g., 900-1200). Visual alert if out of band.
    5.  **Substrate EC & pH**:
        *   **EC**: Real-time substrate EC.
        *   **pH**: Displays the **last recorded irrigation pH average** across all zones in the room OR a specific user-defined zone.

### 3.2. Environmental Trends Chart
*   **Location**: Main Content, Top.
*   **Description**: Multi-line chart showing historical data.
*   **Data Strategy**: Lazy-load series. Load primary (Temp/RH) by default; fetch others (VPD, CO₂, etc.) on toggle.
*   **VWC Customization**:
    *   User defines scope: Room Average OR Specific Sensors (subset).
    *   **Legend Interaction**: Clicking "VWC" in legend allows assigning a custom color to each sensor feed.
    *   **Persistence**: Colors are saved to the sensor config and recallable as "My Colors".
*   **Setpoints & Overrides Panel (Overlay)**:
    *   **State**: Collapsed by default.
    *   **Auto-Expand**: Expands automatically if an active issue/override exists.
    *   **Interaction**: Clicking an item opens its detailed settings/editor.

### 3.3. Irrigation Windows
*   **Location**: Main Content, Middle Left.
*   **Description**: Visualization of irrigation events (executed and planned).
*   **Tabs**: P1 Ramp, P2 Maintenance, P3 Dryback, **All**.
*   **Visualization**: **Dual Bar Chart**.
    *   **Bar 1**: Volume (mL) - actual amount fed for executed events, expected amount for planned events.
    *   **Bar 2**: VWC % - actual end VWC for executed events, expected VWC for planned events.
    *   **Color Coding**:
        *   Executed events: Blue (Automated) / Amber (Manual) for volume, Emerald for VWC.
        *   Planned events: Slate/Light Emerald (ghost bars) with high transparency (15-20% opacity) for subtle visual distinction.
*   **Event States**:
    *   **Executed**: Events that have occurred (before current time). Show actual readings with solid bars.
    *   **Planned**: Scheduled events that haven't occurred yet. Shown as ghost bars with expected values.
*   **Controls**:
    *   Zone Selectors (Zone A-F).
    *   Action: "+ Add shot" (Manual Modal).
    *   Action: "Quick pick" (50mL, etc.). **Requirement**: Triggers a **Confirmation Modal** before firing.
*   **Schedule Performance Panel** (Target icon button):
    *   **Purpose**: Historical analysis of expected vs actual irrigation events to identify alignment issues.
    *   **Alignment Score**: 0-100 score showing overall schedule accuracy.
    *   **Summary Metrics**:
        *   Volume variance (% difference from expected).
        *   VWC accuracy (% of events within ±2% of expected).
        *   On-time execution rate (% within 5 min of scheduled).
        *   Missed/extra event counts.
    *   **Event Details**: Expandable list showing individual event comparisons (expected vs actual volume, VWC, timing).
    *   **Recommendations**: AI-generated suggestions for schedule adjustments based on variance patterns.

### 3.4. Zone Heatmap
*   **Location**: Main Content, Middle Right.
*   **Description**: Grid representation of the room's spatial data.
*   **Features**:
    *   Metric Selector (Dropdown: Temp, RH, etc.).
    *   Color-coded cells based on value intensity.
    *   Indicator for specific status (e.g., red dot for alert).
    *   Click interaction: Opens Sensor/Zone details.

### 3.5. Rooms Footer (Room Navigation)
*   **Location**: Main Content, Bottom.
*   **Description**: Summary cards for *other* rooms/zones to allow quick switching.
*   **Features**:
    *   Room Name.
    *   Mini metrics summary (Temp, RH, EC).
    *   Status Health Dots.
    *   **Interaction**: Click navigates to that room's dashboard.

### 3.6. Sidebar Widgets
*   **Active Alerts**:
    *   Badge count, List of alerts.
    *   Actions: Ack, Delegate, Runbook.
*   **Targets vs Current**:
    *   Table layout comparing metrics.
    *   Click opens Recipe/Target editor.
*   **Quick Actions**:
    *   **Nudge EC**: creates a **Temporary Override** (does not modify permanent recipe).
    *   **Pause Irrigation**: Global safety stop (with confirmation).

## 4. Data Requirements & State

### Global State (`useDashboardStore` / Context)
*   **Selected Context**: Site ID, Room ID (Single Room View).
*   **User Preferences**: Saved VWC colors, selected representative pH zone.

### Data Sources
*   **Real-time Telemetry**: WebSocket connection for live updates.
*   **TimeSeries Data**: API fetch for Charts (Lazy loaded).
*   **Irrigation History**: specific endpoint for event logs (Vol + End VWC).

## 5. Theme & Styling (Tailwind)
*   **Background**: Dark mode default (`bg-slate-950`).
*   **Cards**: `bg-slate-900/50 border border-slate-800 rounded-xl`.
*   **Typography**: Sans-serif, dense.
*   **Colors**:
    *   **Accents**: Cyan (`text-cyan-400`), Blue (`text-blue-500`), Green (`text-emerald-500`).
    *   **Status**: Critical (Rose), Warning (Amber), Normal (Emerald).

## 6. Implementation Phasing
1.  **Scaffold**: Grid Layout & Store updates.
2.  **Core Metrics**: Top row widgets.
3.  **Complex Visuals**:
    *   Trends Chart (with VWC coloring logic).
    *   Irrigation Dual Bar Chart.
    *   Heatmap.
4.  **Interactivity**: Quick Actions (Modals/Overrides), Setpoints Overlay logic.
5.  **Navigation**: Rooms Footer.

## 7. Technical Implementation
A demonstration implementation has been created at `src/frontend/app/dashboard/cultivation/page.tsx`.

### Widgets
*   **EnvironmentalMetricsWidget**: Handles the top row cards with toggle logic for DLI/PPFD and Substrate/pH display.
*   **EnvironmentalTrendsWidget**: Implements Recharts area chart with lazy loading capability and a custom legend interaction for VWC coloring.
*   **IrrigationWindowsWidget**: Dual bar chart (Volume + End VWC) with manual shot entry and "Quick Pick" buttons protected by confirmation modals.
*   **ZoneHeatmapWidget**: Spatial grid visualization with metric selector (Temp, RH, VWC, EC, PPFD).
*   **SidebarWidgets**:
    *   `ActiveAlertsListWidget`: Scrollable list with Ack/Delegate actions.
    *   `TargetsVsCurrentWidget`: Dense table comparing live values to targets.
    *   `QuickActionsWidget`: Temporary override buttons ("Nudge EC") and "Pause Irrigation" safety stop.
*   **RoomsStatusWidget**: Navigation cards for switching between rooms.

### Layout
A dedicated `CultivationLayout` component enforces the 12-column grid structure required for this high-density view. The widgets are registered in `widgetRegistry.ts` with the `cultivation-` prefix.

## 8. LLM Assistant Integration for Cultivation
- Scope: operator/grower copilot focused on cultivation workflows (plants, environment, irrigation, IPM) with citations and quick actions back into dashboard widgets.
- Safety: only uses canonical data from existing services (no alternate truth), redacts PII before OpenAI calls, refuses unsafe actuation, and logs prompt/response metadata without storing raw PII.
- Primary interactions:
  - Inline “Ask” on cultivation dashboard sections (metrics, trends, irrigation, alerts) to explain anomalies, suggest actions, or summarize changes.
  - Chat panel for multi-turn reasoning; returns links to relevant widgets/records.
  - Command palette macros (e.g., “Run plant health check for Room A”).
- High-value use cases:
  - Environment & plant health watch: explain sensor anomalies (temp/RH/VPD/EC/pH), phase transition risks, mortality spikes; propose SOP-linked actions.
  - Irrigation/fertigation validation: check schedules vs. recipes and stage; flag over/under-watering signals from VWC/EC deltas; summarize last 24h irrigation.
  - IPM readiness: surface overdue/next IPM tasks by zone/stage; generate checklists; highlight coverage gaps.
  - Harvest readiness & QA: identify blocks nearing harvest, estimate readiness windows, and validate weight deltas once harvest starts.
  - Report interpretation: summarize cultivation KPIs (trend deltas, outliers) with cited filters/time ranges.
- Guardrails:
  - Deterministic context assembly from existing APIs/stores; no unverified free-text facts.
  - Refusal policy for actions that would change recipes/actuation; recommendations only.
  - Confidence + provenance in every suggestion; suppress low-confidence/noisy alerts.

## 9. Testing & Rollout (LLM Assistant)
- Unit: prompt renderer (variable validation), redactor/safety filters, data QA filter thresholds.
- Contract/integration: assistant API route returns streamed text; Telemetry insights worker runs without throwing; OpenAI gateway gracefully handles 4xx/5xx.
- Shadow mode: cultivate insights worker runs log-only; assistant API responds with fallback when key missing; feature flag gates UI entry.
- Metrics: token usage, latency, success rate, redaction applied, safety flags, noise rate (QA filter pass ratio).
- Rollout: enable for staging with test API key → canary single site/room → expand after stability; keep feature flag per persona/domain.

