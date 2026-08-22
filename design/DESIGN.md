---
name: Sonic Precision
colors:
  surface: '#131315'
  surface-dim: '#131315'
  surface-bright: '#39393B'
  surface-container-lowest: '#0e0e10'
  surface-container-low: '#1b1b1d'
  surface-container: '#1F1F21'
  surface-container-high: '#2a2a2c'
  surface-container-highest: '#353437'
  on-surface: '#e5e1e4'
  on-surface-variant: '#BBCABF'
  inverse-surface: '#e5e1e4'
  inverse-on-surface: '#303032'
  outline: '#86948a'
  outline-variant: '#3C4A42'
  surface-tint: '#4edea3'
  primary: '#4edea3'
  on-primary: '#003824'
  primary-container: '#10b981'
  on-primary-container: '#00422b'
  inverse-primary: '#006c49'
  secondary: '#adc6ff'
  on-secondary: '#002e6a'
  secondary-container: '#0566d9'
  on-secondary-container: '#e6ecff'
  tertiary: '#f9bd22'
  on-tertiary: '#402d00'
  tertiary-container: '#ce9a00'
  on-tertiary-container: '#4a3500'
  error: '#ffb4ab'
  on-error: '#690005'
  error-container: '#93000a'
  on-error-container: '#ffdad6'
  primary-fixed: '#6ffbbe'
  primary-fixed-dim: '#4edea3'
  on-primary-fixed: '#002113'
  on-primary-fixed-variant: '#005236'
  secondary-fixed: '#d8e2ff'
  secondary-fixed-dim: '#adc6ff'
  on-secondary-fixed: '#001a42'
  on-secondary-fixed-variant: '#004395'
  tertiary-fixed: '#ffdf9f'
  tertiary-fixed-dim: '#f9bd22'
  on-tertiary-fixed: '#261a00'
  on-tertiary-fixed-variant: '#5c4300'
  background: '#131315'
  on-background: '#e5e1e4'
  surface-variant: '#353437'
typography:
  display-lg:
    fontFamily: Geist
    fontSize: 48px
    fontWeight: '700'
    lineHeight: 52px
    letterSpacing: -0.02em
  headline-md:
    fontFamily: Geist
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 28px
  body-base:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 21px
  body-sm:
    fontFamily: Inter
    fontSize: 13px
    fontWeight: '400'
    lineHeight: 18px
  data-mono:
    fontFamily: JetBrains Mono
    fontSize: 13px
    fontWeight: '500'
    lineHeight: 13px
  data-lg:
    fontFamily: JetBrains Mono
    fontSize: 18px
    fontWeight: '600'
    lineHeight: 18px
  label-caps:
    fontFamily: JetBrains Mono
    fontSize: 11px
    fontWeight: '600'
    lineHeight: 11px
    letterSpacing: 0.05em
rounded:
  sm: 0.125rem
  DEFAULT: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  full: 9999px
spacing:
  unit: 4px
  gutter: 16px
  margin-mobile: 16px
  margin-desktop: 32px
  row-height: 40px
---

## Brand & Style

This design system is engineered for high-stakes technical environments, specifically live performance, audio production, and signal analysis. It prioritizes **technical precision, high information density, and visual focus**. The aesthetic is defined by "Darkroom Modern"—a hybrid of **Minimalism** and **Geist-inspired Technicality**.

The interface is designed to disappear, allowing waveform visualizations and metadata to take center stage. It utilizes a high-contrast dark mode to reduce eye strain in low-light environments, with surgical applications of sophisticated emerald, electric blue, and amber tones to denote state and critical performance data. The goal is to evoke a sense of professional reliability and "pro-tool" authority.

## Reference Package Use

This document, screenshots, and local assets describe the intended visual language. Treat generated Stitch HTML as explanatory reference only: do not copy it into the application or retain its remote assets, CDN imports, sample content, or unapproved future controls. Rebuild approved workflows with semantic Blazor markup, local CSS, known component APIs, and system-font fallbacks. Preserve product boundaries and existing safe flows over visual similarity.

## Colors

The palette is optimized for OLED displays and low-light professional environments, utilizing a deep-dark base with high-contrast functional accents.

- **Primary (Emerald):** A refined green (#10B981) reserved for active playback states, successful analysis, and primary action buttons.
- **Secondary (Refined Blue):** A precise, high-contrast blue (#3B82F6) designed for technical tags, progress bars, and secondary interactive elements. It provides a cool counterpoint to the warmer emerald and amber.
- **Tertiary (Amber Energy):** A light yellow-orange (#FBBF24) used for high-energy indicators, rhythmic metadata, and warning states.
- **Base Neutrals:** A high-contrast gray scale. The global background uses the deepest black (#131315), with containers and surfaces tiered through subtle shifts in gray to maintain structural clarity without relying on heavy borders.

## Typography

The typography strategy separates interface language from performance-critical data to improve scan-rates during high-pressure use.

1. **Geist** is used for headings and navigation to provide a clean, Swiss-style technical appearance.
2. **Inter** handles all UI body text and descriptions, ensuring maximum legibility at small sizes.
3. **JetBrains Mono** is strictly reserved for technical metadata: BPM, Key, Duration, and Timestamps. The monospaced nature ensures that numeric values do not "jitter" or shift layouts during active updates.

## Layout & Spacing

This system employs a **Condensed Fluid Grid** designed for high-information density. Whitespace is used strategically as a grouping mechanism rather than a separator.

- **Layout Model:** A 12-column fluid grid for dashboard views.
- **Density:** Vertical spacing is minimized (40px standard row height) to maximize the data visible on a single screen.
- **Sidebars:** Fixed-width left navigation (240px) with a collapsible right-hand panel for track metadata.
- **Mobile Adaption:** Use a 768px breakpoint to shift from multi-column dashboards to a single-column stacked view optimized for library management on the go.

## Elevation & Depth

To maintain a sleek, hardware-like appearance, this system avoids traditional drop shadows in favor of **Tonal Layering** and **Ghost Borders**.

- **Level 0 (Background):** Solid black-neutral base (#131315).
- **Level 1 (Cards/Panels):** Surface-container (#1F1F21) with a 1px solid border in `outline-variant`.
- **Level 2 (Modals):** Lighter gray surfaces (#39393B) with a more pronounced border to separate them from the background.
- **Active State:** Elements in an "Active" or "Playing" state lose their border in favor of a 1px solid stroke using the Primary Emerald color.

## Shapes

The shape language is **Soft**, maintaining a professional, engineered feel that mimics hardware rack equipment.

- **Functional Components:** Buttons, Checkboxes, and Inputs use the standard 4px radius (`rounded`).
- **Large Containers:** Track Cards and Waveform Containers use an 8px radius (`rounded-lg`).
- **Data Tags:** Use a Pill shape for Genre and Technical metadata tags to visually differentiate them from functional buttons.

## Components

### Buttons & Controls
- **Primary Button:** Solid Primary Emerald (#10B981) with black text.
- **Secondary Button:** Refined Blue (#3B82F6) outline or ghost style; provides clear contrast against dark surfaces.
- **Transport Controls:** Larger hit areas for Play/Pause. Use the Primary color only when the track is in an active state.

### Technical Tables
- **Header cells:** Use `label-caps` typography at 40% opacity.
- **Row hover:** Apply a subtle highlight shift (`surface-bright`) to indicate interactivity.
- **Alignment:** Numeric data (BPM, Key) must be right-aligned for vertical scanning; text data is left-aligned.

### Status Badges & Indicators
- **Technical Tags:** Use the Refined Blue (#3B82F6) for non-critical metadata tags.
- **Progress Bars:** Use the Refined Blue for neutral progress (loading, buffering) and Primary Emerald for active playback progress.
- **Energy Meters:** A 5-segment bar component. Higher energy levels (4-5) transition to the **Tertiary Amber** (#FBBF24); lower levels remain in a muted gray.
- **Input Fields:** Dark background (#131315) with a 1px `outline-variant` border, shifting to Primary Emerald on focus.
