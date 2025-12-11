# Crop Steering & Irrigation Quick Reference

This document provides a structured reference for crop steering decisions, including vegetative vs generative steering levers, irrigation signals by phase, and dryback management strategies.

## Overview

**Crop steering** is the practice of manipulating environmental and irrigation parameters to push cannabis plants toward either:

- **Vegetative growth** (leaf/stem development) - promotes plant structure and canopy
- **Generative growth** (flower/fruit development) - promotes bud production and terpene expression

The key is understanding which "levers" to adjust and in which direction.

---

## 1. Steering Levers at a Glance

Use these "dials" to push plants more vegetative or generative.

| Lever / Metric | Vegetative Trend | Generative Trend |
|----------------|------------------|------------------|
| **Substrate EC** | ↓ Lower (1.5-2.5 mS/cm) | ↑ Higher (2.5-4.0 mS/cm) |
| **VWC (Volumetric Water Content)** | ↑ Higher (55-70%) | ↓ Lower (40-55%) |
| **VPD** | ↓ Lower (0.8-1.0 kPa) | ↑ Higher (1.2-1.5 kPa) |
| **Temperature** | ↑ Higher (78-84°F) | ↓ Lower (72-78°F) |
| **Irrigation Frequency** | ↑ More frequent | ↓ Less frequent |
| **Feed Duration** | ↑ Longer | ↓ Shorter |

### Understanding Each Lever

#### Substrate EC (Electrical Conductivity)
- **Lower EC (vegetative)**: Promotes rapid cell division and vegetative growth
- **Higher EC (generative)**: Creates osmotic stress that signals the plant to focus on reproduction

#### VWC (Volumetric Water Content)
- **Higher VWC (vegetative)**: Keeps plants well-hydrated for cell expansion and leaf growth
- **Lower VWC (generative)**: Controlled water stress triggers flowering hormones

#### VPD (Vapor Pressure Deficit)
- **Lower VPD (vegetative)**: Reduces transpiration stress, allows maximum photosynthesis
- **Higher VPD (generative)**: Increases transpiration, concentrates nutrients, promotes flowering

#### Temperature
- **Higher temp (vegetative)**: Accelerates enzymatic activity and cell division
- **Lower temp (generative)**: Slows vegetative growth, promotes terpene retention

---

## 2. Daily Irrigation Phases (P1/P2/P3)

Each day of a crop's life follows a three-phase irrigation cycle:

### P1: Ramp Phase (Post Lights-On)
- **Goal**: Saturate substrate after overnight dryback
- **Timing**: First 2-4 hours after lights on
- **Strategy**: 
  - Frequent, smaller shots
  - Build VWC from overnight low to target saturation
  - Vegetative: Reach 60-70% VWC quickly
  - Generative: Reach 50-60% VWC more gradually

### P2: Maintenance Phase (Mid-Day)
- **Goal**: Sustain target VWC during peak photosynthesis
- **Timing**: Middle of the light period (may be skipped in aggressive generative steering)
- **Strategy**:
  - Trigger shots when VWC drops below threshold
  - Maintain consistent VWC range
  - Vegetative: Maintain 55-65% VWC
  - Generative: Maintain 45-55% VWC

### P3: Dryback Phase (Pre Lights-Off)
- **Goal**: Controlled drying before night period
- **Timing**: Final 2-4 hours before lights off
- **Strategy**:
  - No irrigation (or very limited)
  - Allow VWC to drop to overnight target
  - Vegetative: Dryback to 45-55% VWC
  - Generative: Dryback to 35-45% VWC

---

## 3. Typical Irrigation Signals

### Shot Size (% of substrate volume)
| Steering | Value | Notes |
|----------|-------|-------|
| Vegetative | 2-4% | Smaller, more frequent shots |
| Generative | 4-10% | Larger, less frequent shots |

### Irrigation Interval (during lights-on)
| Steering | Value | Notes |
|----------|-------|-------|
| Vegetative | Every 15-40 min | Keep substrate consistently wet |
| Generative | Every 40 min - 2 hr | Allow dryback between shots |

### Daily Dryback (max → min VWC)
| Steering | Value | Field Capacity Framing |
|----------|-------|------------------------|
| Vegetative | 10-20% | ~25% of max field capacity |
| Generative | 25-50% | ~50% of max field capacity |

### Intershot Dryback (drop between events)
| Steering | Value | Notes |
|----------|-------|-------|
| Vegetative | 1-4% | Minimal drying between shots |
| Generative | 4-6% | Significant drying between shots |

---

## 4. Phase-Specific Configuration Defaults

### Vegetative Steering Profiles

| Phase | Target VWC | Shot Size | Interval | Dryback Target |
|-------|------------|-----------|----------|----------------|
| P1 Ramp | 60-70% | 3% | 15-30 min | N/A |
| P2 Maintenance | 55-65% | 3% | 20-40 min | 5% intershot |
| P3 Dryback | 45-55% | 0% | N/A | 15% daily |

### Generative Steering Profiles

| Phase | Target VWC | Shot Size | Interval | Dryback Target |
|-------|------------|-----------|----------|----------------|
| P1 Ramp | 50-60% | 6% | 30-60 min | N/A |
| P2 Maintenance | 45-55% | 7% | 45-90 min | 10% intershot |
| P3 Dryback | 35-45% | 0% | N/A | 37.5% daily |

---

## 5. Decision Framework

### When to Steer Vegetative
- Early growth stages (seedling, early veg)
- Building plant structure before flip
- Recovering from stress
- Plants showing deficiencies
- When canopy coverage is insufficient

### When to Steer Generative
- Pre-flip conditioning (2-3 weeks before flower)
- During flowering (especially weeks 3-6)
- When stretch control is needed
- To increase resin production
- When plants are overly vegetative

### Balanced Approach
- Transition periods
- When plants are expressing ideal growth
- Unknown strain responses
- Mixed canopy with varying needs

---

## 6. Common Adjustments

### If Plants Are Too Vegetative
1. Increase substrate EC
2. Reduce VWC targets
3. Increase VPD
4. Lower temperature
5. Reduce irrigation frequency
6. Allow more dryback

### If Plants Are Too Generative
1. Decrease substrate EC
2. Increase VWC targets
3. Reduce VPD
4. Raise temperature
5. Increase irrigation frequency
6. Reduce dryback

---

## 7. Monitoring & Evaluation

### Key Metrics to Track
- **VWC trends**: Monitor saw-tooth pattern during light hours
- **Daily dryback %**: Calculate max-to-min VWC daily
- **EC runoff**: Compare to feed EC for accumulation
- **VPD consistency**: Check against target ranges
- **Plant response**: Stretch rate, internode spacing, leaf color

### Warning Signs

| Symptom | Possible Cause | Adjustment |
|---------|----------------|------------|
| Excessive stretch | Too vegetative | Increase EC, reduce VWC |
| Tight internodes | Too generative | Decrease EC, increase VWC |
| Leaf curl down | VPD too high | Lower temp or raise humidity |
| Slow growth | VWC too low | Increase irrigation frequency |
| Root issues | VWC too high | Increase dryback |

---

## 8. Integration with Harvestry

### Related System Components
- **StreamType.SoilMoisture (20)**: VWC sensor data
- **StreamType.SoilEc (22)**: Substrate EC sensor data
- **StreamType.Vpd (4)**: VPD calculation from temp/humidity
- **StreamType.Temperature (1)**: Air temperature

### Profiles & Overrides
- Site-wide default profiles define baseline steering parameters
- Strain-specific profiles can override any parameter
- Profiles are phase-aware (P1/P2/P3 configurations)

### Automation Support
- Recommendation engine compares telemetry against profile targets
- Suggestions generated when metrics deviate from steering mode
- Autosteer MPC uses cultivar response curves for optimization

---

## References

- Crop Steering & Irrigation Quick Reference (Combined) PDF
- Irrigation Adjustment Strategies: Dryback & EC PDF
- Industry best practices for controlled environment agriculture
