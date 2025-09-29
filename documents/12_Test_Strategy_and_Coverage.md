# Test Strategy & Coverage

- **Unit tests:** core services, UoM conversions, dosing math.
- **Integration tests:** Slack, QBO, METRC/BioTrack; DB migrations.
- **Contract tests:** External APIs, outbox/queue semantics.
- **E2E tests:** Critical user journeys (tasking, irrigation run, lot movement, COA hold).
- **Property-based tests:** UoM and dosing calculations.
- **Hardware-in-the-loop:** Irrigation programs & interlocks; valve concurrency policies.
- **Chaos drills:** packet loss, broker flap, latency injection; confirm safe fails.
- **Performance:** load tests monthly with synthetic telemetry; p95 targets enforced.
