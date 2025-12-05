# Typography Requirements

## Overview
Harvestry uses a clean, highly legible, and data-optimized typography system. The goal is to ensure readability in high-density dashboard environments (Cultivation Control) while maintaining a modern, professional aesthetic.

## Primary Font Family
**Inter** is the primary typeface for the application. It provides:
*   Exceptional legibility at small sizes.
*   Neutral character that doesn't distract from data.
*   Extensive features (tabular figures, case-sensitive forms).

### Usage
*   **UI Elements**: Labels, buttons, navigation.
*   **Data**: Metrics, table data (using tabular figures).
*   **Body Text**: General content.

## Monospace Font Family
**JetBrains Mono** is used for:
*   Code snippets.
*   JSON data viewers.
*   Log outputs.
*   Specific identifiers (e.g., Device IDs, Sensor Hashes) where character distinction is critical.

## Styles & Weights

### Headings
*   **Uppercase Headers** (e.g., Widget Titles): Bold, Uppercase, Tracking Wider.
    *   `font-bold uppercase tracking-wider text-xs/sm`
*   **Metric Values**: Bold/ExtraBold, Tight Tracking.
    *   `font-bold tracking-tight text-2xl/3xl`

### Data
*   Use `tabular-nums` class for all tables and real-time metrics to prevent layout jitter.

## Implementation
The fonts are configured in `src/frontend/app/layout.tsx` via `next/font` and mapped to CSS variables in `src/frontend/styles/tokens/typography.css`.

*   `--font-primary`: Inter
*   `--font-mono`: JetBrains Mono






