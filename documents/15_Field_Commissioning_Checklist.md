# Field Commissioning Checklist

> **Security Note:** Verify device identity and certificate provisioning as per [35_Edge_Security_Model.md](./35_Edge_Security_Model.md).

- [ ] Mount NEMA 4X/IP66 enclosure; glands and strain relief installed.
- [ ] Power: AC mains present; PoE link verified; OR-ing switchover tested (no spurious actuation).
- [ ] Selector: INT/EXT set appropriately; transformer VA rating confirmed for manifold.
- [ ] Safety: E-STOP and Door interlock verified (forces outputs OFF).
- [ ] SDI-12: bus length within design; address set; aI!/M!/C! cycle passes.
- [ ] 4–20 mA: pressure/level calibrated at 4/12/20 mA; readings within spec.
- [ ] Valves: concurrency caps enforced; HL thermals within spec during soak.
- [ ] Flow: pulse inputs counted correctly 1–50 Hz.
- [ ] Broker loss simulation: autonomous recovery; persisted logs intact.
- [ ] **Security:** Device Certificate verified; mTLS connection established.
- [ ] **Local Mode:** Verify Emergency Dashboard is accessible via mDNS.
