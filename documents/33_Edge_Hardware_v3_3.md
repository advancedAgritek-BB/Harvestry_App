# Harvestry Edge Hardware v3.3 Specifications

**Turnkey Design • Fabrication • Assembly Requirements (v3.3)**
**Doc ID:** HVRY-EDGE-CM-REQ-v3.3
**Status:** Active Draft

## 1. Overview
The v3.3 hardware line focuses on a **"Turnkey & IP-Based"** architecture. It moves away from RS-485 home-runs for expansion, favoring standard Ethernet/PoE for scalability and ease of installation.

**Core Philosophy:**
*   **Offline-First:** All controllers and pods run independent logic/micro-programs.
*   **One-Cable Install:** PoE or AC Mains with QR-code claiming.
*   **IP-Based Expansion:** No proprietary bus cabling; standard CAT6 structured cabling.

---

## 2. HydroCore v2 (Water Room Controller)
**Role:** The central brain for the irrigation/fertigation room. Manages main pumps, valves, and chemical injection.

| Feature | Specification |
| :--- | :--- |
| **Enclosure** | NEMA 4X / IP66 Polycarbonate (300×250×160 mm) w/ Metal Backplate |
| **Power** | AC Mains (110/220V) or PoE++ (Type 4) |
| **Transformer** | INT-150VA (Integrated 24VAC for Valves) |
| **Connectivity** | Ethernet (Gigabit), WiFi 6 (Backup), LTE (Optional Dongle) |
| **Inputs** | 8x Analog (4-20mA / 0-10V), 8x Digital (Flow/Level) |
| **Outputs** | 16x 24VAC Valve Drivers (High-Inrush), 4x Relay (Pump Start) |
| **SkidLink** | **Modbus/TCP** (Primary) & Hardwired IO (Secondary) for HE Anderson / Argus Integration |
| **Capabilities** | Offline Recipe Execution, Safety Interlocks, Local Web Dashboard |

**Key Integration: SkidLink**
HydroCore v2 is designed to take over *control* of existing fertigation skids.
*   **Mode A (Smart):** Sends "Set Target EC/pH" commands via Modbus/TCP.
*   **Mode B (Dumb):** Directly drives injection solenoids via expansion pods.

---

## 3. RoomHub v2 (Grow Room Controller)
**Role:** Dedicated controller for a single grow environment (Flower Room, Veg Room).

| Feature | Specification |
| :--- | :--- |
| **Enclosure** | NEMA 4X / IP66 (240×190×110 mm) |
| **Power** | PoE+ (Type 4) |
| **Lighting** | 8x 0-10V Dimming Channels + Relay Enable |
| **Sensing** | Temp/RH/VPD/CO2 (Onboard or Remote), PAR/PPFD (0-10V/SDI-12) |
| **Substrate** | Interface for Teros-12 or similar substrate sensors (SDI-12) |
| **HVAC** | 24VAC Thermostat Interface (R/G/Y1/Y2/W1/W2/C) |
| **Safety** | Door Interlock Input (shuts off CO2/Lights) |

---

## 4. EdgePods-IP (Distributed Expansion)
**Role:** Network-attached I/O expansion. Placed near the equipment it controls.

**Common Specs:**
*   **Form Factor:** DIN Rail Mount (DIN 6-module 106×90×62 mm)
*   **Power/Data:** PoE (802.3af)
*   **Protocol:** CoAP over UDP (Low latency, discovery-based)
*   **Resilience:** "Micro-Programs" (e.g., "Keep valve open for 5s then close" runs locally on the Pod, ensuring safety if network drops).

**Variants:**
*   **Pod-VAL8:** 8x 24VAC Valve Outputs (Irrigation Zones)
*   **Pod-AI4:** 4x Analog Inputs (4-20mA Sensors)
*   **Pod-SDI4:** 4x SDI-12 Channels (Soil Sensors)
*   **Pod-DI8:** 8x Digital Inputs (Security/Door/Level Switches)
*   **Pod-AO4:** 4x Analog Outputs (0-10V Dimming/VFD Control)

---

## 5. Certification & Compliance
*   **Safety:** UL 508A Listed Component (Planned).
*   **EMI/EMC:** FCC Part 15 Class B.
*   **Ingress:** IP66 / NEMA 4X.





