# Manufacturing SOW & Factory Test Plan (Prototype Lot)

> **Important:** Provisioning of device certificates is mandatory. See [35_Edge_Security_Model.md](./35_Edge_Security_Model.md) for the Key Generation & CSR workflow.

## Deliverables
- Fabrication pack (Gerber X2, drills, stack/impedance, panelization, isolation slots).
- Assembly pack (BOM/AVL; XYRS; assembly drawings; stencil; conformal coat map).
- Enclosure pack (mains bay layout; inlet/transformer/selector; gland legend).
- Programming & test (bootloader + MFG-TEST; SWD scripts; fixtures; logs).
- Quality & shipping (IPC-A-610 Class 2; AOI coverage; labels).

## Factory Tests (must run and log)
- PoE PD classification Class 4; logic rails within spec; Ethernet link 100 Mb.
- MCU/RTC/FRAM/microSD; secure element presence.
- Isolation hi-pot between domains; no leakage.
- SDI-12 chain aI!/M!/C!; Analog 4–20 mA and 0–10 V span/linearity; DI pulse 1–50 Hz.
- Valves: INT caps enforced; EXT load test (4 HL + 8 STD ≥60 s) without brownout.
- Relays drive contactor coil load; flow pulses counted; pressure/level correct.
- Interlocks: E-STOP/Door force OFF; FAULT latched/logged.
- Failover chaos: PoE↔AC switchover without spurious actuation; broker loss recovery.
- Environmental soak: 72 h @ 40–60 °C, 90–95% RH; coatings & labels intact.
- **Security Provisioning:** Key pair generation in Secure Element; CSR signing; Cert injection.

## Acceptance Criteria
- Electrical & isolation pass; dual-source failover proven; Ethernet stable.
- Functional I/O pass; concurrency policy enforced; no transformer over-temp.
- Environmental soak passed; test logs returned with firmware hashes.
- Device uniquely identified and cryptographically verified.
