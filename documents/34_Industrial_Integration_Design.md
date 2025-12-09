# Industrial Integration & Enterprise Design (v3.3)

## 1. Overview
This document outlines the strategy for integrating Harvestry Edge into **Enterprise Facilities**. Unlike prosumer setups, enterprise facilities often require:
1.  Integration with existing industrial equipment (Fertigation Skids).
2.  Hardware Agnosticism (Running on generic IPCs).
3.  Strict Safety & Security standards.
4.  Local survivability and control during outages.

---

## 2. SkidLink Integration Strategy
**"SkidLink"** is the protocol layer for controlling third-party fertigation systems (HE Anderson, Priva, Argus, Netafim).

### 2.1 Supported Modes
1.  **Supervisor Mode (Recommended):**
    *   **Protocol:** Modbus/TCP.
    *   **Logic:** Harvestry sends high-level setpoints (e.g., "Recipe A, EC 2.5, pH 6.0") to the skid's PLC.
    *   **Responsibility:** The Skid handles the dangerous chemical mixing loops. Harvestry handles the schedule and verification.
2.  **Direct Control Mode (Retrofit):**
    *   **Hardware:** Harvestry HydroCore + EdgePods.
    *   **Logic:** Harvestry completely bypasses the old controller and drives the pumps/solenoids directly.
    *   **Requirement:** Requires "High-Frequency Pulse" logic in the Edge Engine for precision dosing.

### 2.2 Integration Matrix
| Manufacturer | Protocol | Connection | Support Status |
| :--- | :--- | :--- | :--- |
| **HE Anderson** | Modbus/TCP | Ethernet | ✅ Supported |
| **Argus** | Modbus/TCP or BACnet/IP | Ethernet | 🚧 In Development |
| **Priva** | SOAP/XML (Legacy) | Gateway Required | ❌ Backlog |
| **Generic PLC** | Modbus/TCP | Ethernet | ✅ Supported |

---

## 3. Enterprise "Software-Only" Option
For facilities that refuse proprietary hardware or have standardized on specific IT vendors (Dell/Cisco).

### 3.1 Hardware Specs (Reference Architecture)
*   **Device:** OnLogic CL210 or Dell Edge Gateway 3200.
*   **OS:** Ubuntu Core or Harvestry Custom Linux (Yocto).
*   **Connectivity:** Dual Gigabit LAN (1 WAN, 1 OT/LAN).
*   **I/O:** USB-to-RS485 adapters or Ethernet I/O Blocks (Advantech ADAM / Moxa).

### 3.2 Deployment Model
The `Harvestry.Edge` stack is deployed as a **Docker Container**.
*   **Privileges:** Requires `--privileged` or specific capability mapping (`CAP_NET_ADMIN`) for network discovery.
*   **Licensing:** Per-node license key enforced via cloud heartbeat.

---

## 4. Safety Strategy
Safety is critical when controlling high-pressure water and chemicals.

### 4.1 Layers of Safety
1.  **Hardware Interlocks (Layer 0):**
    *   **E-Stop Buttons:** Hard-wired in series with the 24VAC Transformer Common. *Software cannot override this.*
    *   **Door Switches:** Hard-wired to LED Driver Dim+ lines (where applicable).
2.  **Firmware Watchdogs (Layer 1):**
    *   **"Dead Man Switch":** If the EdgePod stops receiving commands from the Controller for >30 seconds, all valves CLOSE.
    *   **Max Runtime:** Absolute hard limit on valve open time (e.g., 15 mins) configured at boot.
3.  **Software Rules (Layer 2):**
    *   **Leak Detection:** Flow meter reading > 0 when no valves are open → Trigger Master Valve CLOSE.
    *   **Pressure Fault:** Pressure > 80 PSI → Trip Main Pump.

---

## 5. Local Access Strategy (Emergency Dashboard)
To support **Full Offline Experience**, every Edge Controller hosts a local web server.

### 5.1 Discovery
*   **Protocol:** mDNS (Bonjour/Avahi).
*   **Hostname:** `harvestry-edge-{serial}.local`.
*   **Mobile App Behavior:** When Cloud API is unreachable, App scans local WiFi for mDNS services and offers "Local Mode."

### 5.2 Emergency Dashboard
*   **URL:** `http://{ip}:5000/`
*   **Auth:** Local PIN code (cached on phone) or Physical QR Code scan (generates session token).
*   **Capabilities:**
    *   View Sensor Live Data.
    *   View Active Alarms.
    *   **Manual Override:** Force Stop / Pause / Resume.
    *   **Note:** Configuration changes (Recipe edits) are *Disabled* in local mode to prevent drift, unless "Emergency Admin" is invoked.
