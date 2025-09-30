# HIL Chaos Drill Playbook — Track B FRP-06 (W6)

**Version:** 1.0  
**Date:** 2025-09-29  
**Owner:** Hardware/Firmware + Telemetry & Controls  
**Purpose:** Validate irrigation hardware interlocks and safety behavior under fault conditions  
**Requirement:** Firmware sign-off gate before enabling irrigation on pilot site

---

## Executive Summary

This playbook defines the **Hardware-in-the-Loop (HIL) chaos drill matrix** required for FRP-06 irrigation go-live. All tests must pass with **zero unsafe actuations** before firmware sign-off is granted.

**Devices Under Test:**
- 5x HydroCore (simulated for now; physical devices ordered)
- 3x RoomHub (simulated for now; physical devices ordered)

**Test Fixture:** Golden harness with relay boards, fault injection, logging infrastructure

---

## Golden Harness Specifications

### Hardware Components
1. **Relay Board Array** (E-STOP/door simulation)
   - 8x SPST relays for interlock loop injection
   - Manual switches for E-STOP/door override
   - Status LEDs (green=closed, red=open)

2. **Power Injection Board**
   - PoE+ (IEEE 802.3at, 25.5W) with kill switch
   - 24 VAC transformer (100VA) with kill switch
   - Failover logic (PoE primary, AC secondary)

3. **Network Chaos Controller**
   - MQTT broker kill switch (software)
   - VLAN flap simulator (network disconnect/reconnect)
   - Packet loss injector (configurable %)

4. **Load Simulator**
   - Dummy valve loads (solenoid coils, 0.5A @ 24VAC)
   - Current measurement (verify concurrency cap)
   - Thermal monitoring (ensure transformer stays < 60°C)

5. **Logging Infrastructure**
   - Local FRAM capture (device-side logs)
   - Timeseries DB (InfluxDB) for event correlation
   - Video recording (optional, for forensics)

### Software
- **Test Orchestrator:** Python script (`scripts/test/hil-chaos-orchestrator.py`)
- **Device Firmware:** Latest release candidate with MFG-TEST mode
- **MQTT Broker:** Mosquitto with kill script
- **Log Aggregator:** Fluent Bit → InfluxDB

---

## Test Matrix (All Must Pass)

### Test 1: E-STOP Hard-OFF
**Objective:** Verify E-STOP immediately stops all irrigation outputs

| Step | Action | Expected Behavior | Pass Criteria |
|------|--------|-------------------|---------------|
| 1 | Start irrigation program (3-zone sequence) | Zones activate per schedule | All zones open on time |
| 2 | Open E-STOP loop during Zone 2 | **Immediate OFF** all outputs, FAULT latched | Valves close < 100ms; FAULT log created |
| 3 | Close E-STOP loop | Device remains in FAULT state | No auto-resume; requires re-arm |
| 4 | Manual re-arm via API | FAULT clears; ready for next run | Log shows "ESTOP_REARMED" |

**Pass Criteria:**
- ✅ All valves OFF within 100ms of E-STOP open
- ✅ FAULT latched (no auto-resume)
- ✅ Audit log entry: `interlock_event` with `reason="ESTOP_OPEN"`
- ✅ Alert raised to monitoring

**Failure Mode:** If any valve remains open OR device auto-resumes → **FAIL, block firmware sign-off**

---

### Test 2: Door Interlock
**Objective:** Verify door opening stops irrigation (safety for personnel)

| Step | Action | Expected Behavior | Pass Criteria |
|------|--------|-------------------|---------------|
| 1 | Start irrigation program (2-zone sequence) | Zones activate | Zones open on schedule |
| 2 | Open door input during Zone 1 | **Immediate OFF**, FAULT latched | Valves close < 100ms; FAULT log |
| 3 | Close door input | Device remains in FAULT state | No auto-resume |
| 4 | Manual re-arm | FAULT clears | Ready for next run |

**Pass Criteria:**
- ✅ All valves OFF within 100ms of door open
- ✅ FAULT latched
- ✅ Audit log: `interlock_event` with `reason="DOOR_OPEN"`
- ✅ Alert raised

**Failure Mode:** If valves stay open → **FAIL**

---

### Test 3: Tank Level Low
**Objective:** Verify low tank level aborts run safely

| Step | Action | Expected Behavior | Pass Criteria |
|------|--------|-------------------|---------------|
| 1 | Start irrigation program | Program runs | Zones activate |
| 2 | Inject low tank level signal | **Safe abort**, close valves | Valves close; log "TANK_LOW" |
| 3 | Restore tank level | Device ready for retry | No FAULT latch; queue accepts new command |

**Pass Criteria:**
- ✅ Valves close within 5 seconds of low signal
- ✅ No FAULT latch (soft abort)
- ✅ Log: `abort_reason="TANK_LOW"`
- ✅ Alert raised: "Tank low - refill required"

**Auto-Retry:** Configurable per site policy; default = manual restart

---

### Test 4: EC/pH Bounds Violation
**Objective:** Verify out-of-bounds nutrient mix aborts run

| Step | Action | Expected Behavior | Pass Criteria |
|------|--------|-------------------|---------------|
| 1 | Start irrigation with EC target 2.0 ± 0.2 | Program runs | Zones activate |
| 2 | Inject EC reading 2.5 (out of bounds) | **Safe abort**, log reason | Valves close; log "EC_OUT_OF_BOUNDS" |
| 3 | Operator reviews, manually overrides | Run resumes after approval | ABAC override logged |

**Pass Criteria:**
- ✅ Abort within 5 seconds of bounds violation
- ✅ Log: `abort_reason="EC_OUT_OF_BOUNDS"`, `ec_value=2.5`, `ec_target=2.0`, `ec_tolerance=0.2`
- ✅ Alert raised with manual review link

---

### Test 5: CO₂ Exhaust Lockout
**Objective:** Verify CO₂ enrichment prevents irrigation start (prevent leaf burn)

| Step | Action | Expected Behavior | Pass Criteria |
|------|--------|-------------------|---------------|
| 1 | Set room CO₂ enrichment ON (simulated) | Lockout active | Device shows "CO2_LOCKOUT" |
| 2 | Attempt to start irrigation program | **Start rejected**, log reason | Command rejected; log "CO2_LOCKOUT_ACTIVE" |
| 3 | Turn CO₂ enrichment OFF | Lockout cleared | Next start command succeeds |

**Pass Criteria:**
- ✅ Start command rejected (HTTP 409 Conflict)
- ✅ Log: `start_rejected_reason="CO2_LOCKOUT_ACTIVE"`
- ✅ No water delivered

---

### Test 6: Max Runtime Exceeded
**Objective:** Verify runaway programs stop after max runtime

| Step | Action | Expected Behavior | Pass Criteria |
|------|--------|-------------------|---------------|
| 1 | Start program with max_runtime=60s | Program runs | Zones activate |
| 2 | Simulate stuck valve (remains open) | Device detects timeout at 60s | Force close at 60s; log "MAX_RUNTIME_EXCEEDED" |
| 3 | Review logs | Manual intervention required | Alert raised; next run requires approval |

**Pass Criteria:**
- ✅ Force close at exactly max_runtime (±2s tolerance)
- ✅ Log: `abort_reason="MAX_RUNTIME_EXCEEDED"`, `runtime_actual=60`, `runtime_max=60`
- ✅ FAULT latch (requires review)

---

### Test 7: PoE → AC Failover (No Spurious Actuation)
**Objective:** Verify power source failover doesn't cause spurious valve openings

| Step | Action | Expected Behavior | Pass Criteria |
|------|--------|-------------------|---------------|
| 1 | Start irrigation with PoE primary | Program runs on PoE | Zones activate |
| 2 | Pull PoE (AC present) | Failover to AC, run continues | No valve glitch; log "POE_LOSS" |
| 3 | Restore PoE | Return to PoE primary | No interruption |

**Pass Criteria:**
- ✅ Zero spurious actuations during failover
- ✅ Run completes successfully
- ✅ Logs persisted across failover

**Failure Mode:** If valve opens/closes unexpectedly → **FAIL**

---

### Test 8: AC → PoE Failover
**Objective:** Same as Test 7, reverse direction

| Step | Action | Expected Behavior | Pass Criteria |
|------|--------|-------------------|---------------|
| 1 | Start irrigation with AC primary | Program runs on AC | Zones activate |
| 2 | Pull AC (PoE present) | Failover to PoE, run continues | No valve glitch; log "AC_LOSS" |
| 3 | Restore AC | Return to AC primary | No interruption |

**Pass Criteria:**
- ✅ Zero spurious actuations
- ✅ Run completes
- ✅ Logs intact

---

### Test 9: MQTT Broker Loss (Offline-First Behavior)
**Objective:** Verify device continues local program when broker unavailable

| Step | Action | Expected Behavior | Pass Criteria |
|------|--------|-------------------|---------------|
| 1 | Start irrigation program | Program runs | Zones activate |
| 2 | Kill MQTT broker | Device continues local program | Zones complete per local schedule |
| 3 | Restore broker | Device reconciles queue | Logs sync; next command accepted |

**Pass Criteria:**
- ✅ Local program completes despite broker loss
- ✅ Logs buffered locally (FRAM)
- ✅ Queue reconciliation on restore (idempotent)

**Timeout:** Device retries broker connection every 30s; logs persisted for 7 days

---

### Test 10: VLAN Flap (Network Disconnect 30s)
**Objective:** Verify network interruption doesn't corrupt state

| Step | Action | Expected Behavior | Pass Criteria |
|------|--------|-------------------|---------------|
| 1 | Start irrigation program | Program runs | Zones activate |
| 2 | Disconnect network for 30s | Local program continues | Valves operate per local schedule |
| 3 | Reconnect network | Logs sync, state intact | No data loss; queue reconciles |

**Pass Criteria:**
- ✅ No state corruption
- ✅ Logs sync after reconnect
- ✅ Device reports correct run status

---

### Test 11: Concurrency Cap Enforcement (INT-150VA)
**Objective:** Verify device rejects commands exceeding INT-150VA thermal limits

**INT-150VA Limits:**
- High-Load (HL) valves: 0.8A @ 24VAC each
- Standard (STD) valves: 0.5A @ 24VAC each
- **Policy:** ≤ 1 HL + 6 STD simultaneously (0.8 + 3.0 = 3.8A < 4.2A max)

| Step | Action | Expected Behavior | Pass Criteria |
|------|--------|-------------------|---------------|
| 1 | Command 1 HL + 6 STD | All open | Within spec; thermal < 60°C |
| 2 | Command additional 1 HL (exceeds cap) | **Reject excess**, log reason | HTTP 429 Too Many Requests; log "CONCURRENCY_CAP_EXCEEDED" |
| 3 | Close 1 STD, retry excess HL | Accept command | Opens successfully |

**Pass Criteria:**
- ✅ Firmware rejects commands exceeding 1 HL + 6 STD
- ✅ Transformer thermal < 60°C under max load
- ✅ Log: `reject_reason="CONCURRENCY_CAP_EXCEEDED"`, `requested=2HL+6STD`, `limit=1HL+6STD`

**Measurement:** Current clamp on 24VAC transformer secondary

---

### Test 12: Chaos Combo (E-STOP + Broker Loss)
**Objective:** Verify interlock priority over network state

| Step | Action | Expected Behavior | Pass Criteria |
|------|--------|-------------------|---------------|
| 1 | Start irrigation, kill broker | Local program continues | Zones activate |
| 2 | Open E-STOP (broker still down) | **Immediate OFF**, local FAULT | Valves close; FAULT latched |
| 3 | Restore broker | FAULT state syncs | Cloud sees FAULT; no retry |

**Pass Criteria:**
- ✅ E-STOP takes priority (hardware failsafe)
- ✅ FAULT state persists across broker restore
- ✅ Logs complete after sync

---

## Test Execution Procedure

### Pre-Test Setup
1. **Deploy firmware:** Flash latest RC to all devices
2. **Configure golden harness:** Verify relay board, power injection, load simulator
3. **Start logging:** InfluxDB + Fluent Bit + video recording (optional)
4. **Seed test data:** Irrigation programs, zones, schedules in test DB
5. **Baseline check:** All systems green, no existing FAULT states

### During Test
1. **Run test script:** `python scripts/test/hil-chaos-orchestrator.py --test-id <N>`
2. **Monitor outputs:** Real-time valve state, current draw, thermal
3. **Inject fault:** Per test matrix step
4. **Capture logs:** Device local logs + cloud logs + video
5. **Verify pass criteria:** Automated assertions + manual review

### Post-Test
1. **Review logs:** Search for `ERROR`, `FAULT`, `UNSAFE_ACTUATION`
2. **Generate report:** `hil-report-<timestamp>.md` with pass/fail per test
3. **Sign-off:** Hardware/Firmware Lead reviews and signs

---

## Pass/Fail Criteria Summary

### Overall Pass Requirements
- ✅ All 12 tests pass
- ✅ Zero unsafe actuations (valves open when should be closed)
- ✅ All FAULT states latch correctly (no auto-resume on E-STOP/door)
- ✅ Logs complete and correlated
- ✅ Thermal limits respected (transformer < 60°C)

### Firmware Sign-Off Gate
**Required for FRP-06 go-live:**
- ✅ HIL report generated
- ✅ All tests passed
- ✅ Hardware/Firmware Lead signature
- ✅ Report attached to pilot site device record

**If ANY test fails:**
- ❌ Firmware sign-off **DENIED**
- 🚧 Block irrigation enablement on pilot site
- 📝 Issue firmware bug report with logs
- 🔄 Re-test after firmware fix

---

## Logging & Forensics

### Device-Side Logs (FRAM)
```json
{
  "event": "interlock_trip",
  "reason": "ESTOP_OPEN",
  "timestamp": "2025-09-29T14:32:18Z",
  "run_id": "uuid",
  "step_id": "uuid",
  "valves_open": ["V-001", "V-003"],
  "close_latency_ms": 87,
  "fault_latched": true
}
```

### Cloud-Side Logs (InfluxDB)
```influxdb
measurement: interlock_events
tags: { device_id, run_id, reason }
fields: { close_latency_ms, fault_latched, valves_affected }
timestamp: unix_nano
```

### Video Recording
- **Frame Rate:** 30 fps
- **Storage:** 24h retention (per test run)
- **Purpose:** Forensic analysis of physical valve behavior

---

## Safety Protocols

### Human Safety
- ⚠️ **E-STOP accessible** at all times during tests
- ⚠️ **Personnel clear** of water spray zones
- ⚠️ **Eye protection** if using high-pressure lines

### Equipment Safety
- ⚠️ **Thermal monitoring:** Abort if transformer > 70°C
- ⚠️ **Current limiting:** Circuit breaker set to 5A (24VAC)
- ⚠️ **Water containment:** Catch basins for dummy loads

---

## Test Schedule

| Week | Activity | Owner |
|------|----------|-------|
| **W0-W2** | Golden harness fabrication | Hardware/Firmware |
| **W3-W4** | Firmware RC1 release | Firmware Lead |
| **W5** | HIL rehearsal (dry run, no faults) | Telemetry & Controls |
| **W6** | HIL chaos drill execution (full matrix) | Hardware/Firmware + Controls |
| **W6 EOW** | Report generation + sign-off | Hardware/Firmware Lead |

---

## Deliverables

1. **HIL Test Report** (`docs/hardware/hil-report-<timestamp>.md`)
   - Test matrix results (pass/fail per test)
   - Logs (device + cloud)
   - Video links (if applicable)
   - Firmware version tested
   - Sign-off signature

2. **Firmware Sign-Off Certificate**
   ```text
   I certify that the firmware version <X.Y.Z> has passed all HIL chaos drills
   and is approved for deployment to pilot site irrigation systems.
   
   Signature: ____________________
   Name: Hardware/Firmware Lead
   Date: 2025-09-XX
   ```

3. **Attachment to Site Device Record**
   - Link HIL report to pilot site's equipment registry
   - Required before `frp_06_irrigation_enabled = true`

---

## Appendix: Automation Scripts

### Test Orchestrator
**File:** `scripts/test/hil-chaos-orchestrator.py`

```python
#!/usr/bin/env python3
"""
HIL Chaos Drill Orchestrator
Executes test matrix and generates report
"""

import argparse
import time
from mqtt_client import MqttClient
from golden_harness import GoldenHarness
from influxdb_logger import InfluxLogger

def test_estop_hard_off(harness, mqtt, logger):
    """Test 1: E-STOP Hard-OFF"""
    print("[TEST-1] Starting E-STOP test...")
    
    # Step 1: Start irrigation
    mqtt.publish("irrigation/command", {"action": "start", "program_id": "test-001"})
    time.sleep(5)  # Wait for zones to activate
    
    # Step 2: Inject E-STOP open
    harness.open_estop()
    close_latency = harness.measure_valve_close_time()
    
    # Verify
    assert close_latency < 0.1, f"Close latency {close_latency}s exceeds 100ms"
    assert harness.is_fault_latched(), "FAULT not latched"
    
    logger.log_event("test_1_estop", {"result": "PASS", "close_latency": close_latency})
    print("[TEST-1] PASS")

# ... additional test functions ...

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--test-id", type=int, help="Run specific test (1-12)")
    args = parser.parse_args()
    
    harness = GoldenHarness()
    mqtt = MqttClient("test-broker.local")
    logger = InfluxLogger("hil-results")
    
    if args.test_id:
        test_functions[args.test_id](harness, mqtt, logger)
    else:
        # Run all tests
        for i, test_fn in enumerate(test_functions, start=1):
            test_fn(harness, mqtt, logger)
            time.sleep(10)  # Cool-down between tests
    
    # Generate report
    logger.generate_report("docs/hardware/hil-report-{timestamp}.md")
```

---

**Last Updated:** 2025-09-29  
**Version:** 1.0  
**Status:** ✅ Approved for W6 Execution  
**Next Review:** After W6 HIL drill completion
