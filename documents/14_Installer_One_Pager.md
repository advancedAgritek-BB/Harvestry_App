# Installer One-Pager

- **Power:** Use AC mains or PoE+. AC indicator shows mains present. PoE/AC are ideal-ORed; no backfeed.
- **Valves (INT transformer variants):** Set INT/EXT selector to INT for integrated transformer. Default concurrency caps (enforced by firmware): INT-100VA → ≤1 HL + 4 STD; INT-150VA → ≤1 HL + 6 STD. For higher concurrency, use EXT 24 VAC and update policy.
- **Pumps:** Drive contactors; **never** route motor mains through controller PCB.
- **Safety:** Wire E-STOP loop and door interlock; outputs drop to OFF on interlock open or network/broker loss.
- **Indicators:** PWR/NET/BROKER/FAULT status; per-channel LEDs for activity.
- **Commissioning:** Verify flow pulses, pressure/level 4–20 mA, and SDI-12 chains; run valve step test.
