---
name: Obsidian Reserve
colors:
  surface: '#111317'
  surface-dim: '#111317'
  surface-bright: '#37393d'
  surface-container-lowest: '#0c0e11'
  surface-container-low: '#1a1c1f'
  surface-container: '#1e2023'
  surface-container-high: '#282a2d'
  surface-container-highest: '#333538'
  on-surface: '#e2e2e6'
  on-surface-variant: '#c4c7c9'
  inverse-surface: '#e2e2e6'
  inverse-on-surface: '#2f3034'
  outline: '#8e9193'
  outline-variant: '#444749'
  surface-tint: '#c5c7c8'
  primary: '#ffffff'
  on-primary: '#2e3132'
  primary-container: '#e1e2e4'
  on-primary-container: '#626566'
  inverse-primary: '#5c5f60'
  secondary: '#bac8d5'
  on-secondary: '#25323c'
  secondary-container: '#3b4853'
  on-secondary-container: '#a9b7c3'
  tertiary: '#ffffff'
  on-tertiary: '#003823'
  tertiary-container: '#56feb7'
  on-tertiary-container: '#00734d'
  error: '#ffb4ab'
  on-error: '#690005'
  error-container: '#93000a'
  on-error-container: '#ffdad6'
  primary-fixed: '#e1e2e4'
  primary-fixed-dim: '#c5c7c8'
  on-primary-fixed: '#191c1e'
  on-primary-fixed-variant: '#444749'
  secondary-fixed: '#d6e4f1'
  secondary-fixed-dim: '#bac8d5'
  on-secondary-fixed: '#101d26'
  on-secondary-fixed-variant: '#3b4853'
  tertiary-fixed: '#56feb7'
  tertiary-fixed-dim: '#2be19d'
  on-tertiary-fixed: '#002113'
  on-tertiary-fixed-variant: '#005235'
  background: '#111317'
  on-background: '#e2e2e6'
  surface-variant: '#333538'
typography:
  display-lg:
    fontFamily: Space Grotesk
    fontSize: 56px
    fontWeight: '400'
    lineHeight: 64px
    letterSpacing: -0.04em
  display-lg-mobile:
    fontFamily: Space Grotesk
    fontSize: 36px
    fontWeight: '400'
    lineHeight: 44px
    letterSpacing: -0.03em
  headline-lg:
    fontFamily: Space Grotesk
    fontSize: 32px
    fontWeight: '500'
    lineHeight: 40px
    letterSpacing: -0.02em
  headline-lg-mobile:
    fontFamily: Space Grotesk
    fontSize: 24px
    fontWeight: '500'
    lineHeight: 32px
    letterSpacing: -0.02em
  headline-sm:
    fontFamily: Space Grotesk
    fontSize: 20px
    fontWeight: '500'
    lineHeight: 28px
    letterSpacing: -0.01em
  body-lg:
    fontFamily: Hanken Grotesk
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 26px
    letterSpacing: -0.01em
  body-md:
    fontFamily: Hanken Grotesk
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 22px
    letterSpacing: 0em
  label-md:
    fontFamily: JetBrains Mono
    fontSize: 12px
    fontWeight: '500'
    lineHeight: 16px
    letterSpacing: 0.06em
  label-sm:
    fontFamily: JetBrains Mono
    fontSize: 10px
    fontWeight: '500'
    lineHeight: 14px
    letterSpacing: 0.08em
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  gutter: 1.5rem
  gutter-sm: 0.75rem
  margin: 2.5rem
  margin-sm: 1rem
  space-xs: 0.25rem
  space-sm: 0.5rem
  space-md: 1rem
  space-lg: 1.5rem
  space-xl: 2.5rem
---

## Brand & Style

This design system delivers an ultra-exclusive, high-net-worth wealth management and treasury environment. It avoids playful or hyper-gamified crypto tropes in favor of restrained architectural elegance, private banking discretion, and surgical precision. 

The aesthetic sits at the intersection of **Architectural Minimalism** and **Liquid Glassmorphism**. The visual world is defined by deep obsidian planes, dark graphite backdrops, precision-milled hairline strokes, and luminous silver specular reflections. Micro-interactions are deliberate and dampened, creating an atmosphere of institutional security, timeless refinement, and effortless control.

## Colors

The palette operates on a disciplined monochromatic spectrum anchored by deep obsidian voids and illuminated by platinum and crisp liquid light.

- **Primary (`#F5F6F8` - Platinum White):** Used for peak-emphasis typography, critical figures, and primary tactile triggers.
- **Secondary (`#9EACB8` - Frosted Mist):** Used for secondary labels, directional indicators, metric context, and subtle specular borders.
- **Tertiary (`#38E8A3` - Sovereign Mint):** A highly restrained, hyper-refined accent reserved strictly for positive yield performance and verified security states.
- **Neutral (`#0D0F12` - Deep Obsidian):** The foundational ground tone, backed by layered tones of graphite (`#16191E`) and charcoal slate (`#1D2128`).

Apply color conservatively: 90% of surfaces exist within the graphite-to-pure-white continuum to let data clarity and glass textures dictate perceived value.

## Typography

Typography balances Swiss structural rigour with mathematical precision:

- **Display & Headlines (Space Grotesk):** Delivers sculptural, confident geometric headers with tight negative tracking.
- **Body & Operational Copy (Hanken Grotesk):** Provides neutral, hyper-legible structural reading across varied data densities.
- **Metrics, Data & Labels (JetBrains Mono):** Drives balance statements, account coordinates, ledger states, and tickers. Set in uppercase for labels with deliberate positive tracking to evoke precision chronometer engravings.

## Layout & Spacing

The layout philosophy uses a structured 12-column responsive fluid grid bounded by generous margins that frame the workspace like a bespoke gallery:

- **Desktop (1200px+):** 12 columns, 24px gutters, 40px outer canvas margins. Multi-pane panels support synchronous asset tracking and detailed analytics without modal interruption.
- **Tablet (768px - 1199px):** 8 columns, 16px gutters, 24px outer margins. Secondary analytical panels collapse into swipeable drawer mechanics.
- **Mobile (<768px):** 4 columns, 12px gutters, 16px canvas margins. Linear single-column flow with persistent fixed bottom navigation docks.

Component padding strictly adopts an 8-point baseline rhythm. Large breathing rooms (`space-xl`) between financial instrument clusters enforce focus and executive serenity.

## Elevation & Depth

Visual hierarchy is built with **Tonal Glass Layers** and **Specular Edge Radiance** rather than traditional drop shadows:

1. **Ground Base (`Layer 0`):** Pure obsidian `#0D0F12` with an ultra-subtle radial gradient illumination (`#16191E` at 15% opacity positioned in upper horizons).
2. **Sub-surface Tiers (`Layer 1`):** Solid charcoal slate `#16191E` with a 1px border of `rgba(255, 255, 255, 0.05)`.
3. **Floating Glass Panels (`Layer 2`):** Translucent fill `rgba(22, 25, 30, 0.65)` layered with `backdrop-filter: blur(24px) saturate(140%)`. Outlined with a top-lit gradient hairline stroke: `linear-gradient(180deg, rgba(255, 255, 255, 0.18) 0%, rgba(255, 255, 255, 0.02) 100%)`.
4. **Specular Focus & Overlays (`Layer 3`):** High glass sheets `rgba(29, 33, 40, 0.82)` with directional ambient bounce: `0 24px 48px -12px rgba(0, 0, 0, 0.65), 0 0 1px 1px rgba(255, 255, 255, 0.12) inset`.

## Shapes

The design system maintains a refined, architectural curvature profile:

- Standard structural panels, cards, and modal containers use `rounded-lg` (16px / 1rem).
- Interactive inputs, buttons, and segmented pickers scale with standard `rounded` (8px / 0.5rem).
- Metric chips, status pills, and micro-badges use contextual capsule rounding (`9999px`) to visually delineate dynamic metadata from foundational containers.

## Components

### Buttons
- **Primary:** Crisp pure platinum background (`#F5F6F8`) with pure dark graphite typography (`#0D0F12`), semi-bold. On hover, apply a subtle silver specular wash (`#FFFFFF`) with `box-shadow: 0 0 20px rgba(255, 255, 255, 0.25)`.
- **Secondary Glass:** Backdrop blur `16px`, fill `rgba(255, 255, 255, 0.04)`, 1px outline of `rgba(255, 255, 255, 0.12)`. Pure white typography.
- **Tertiary Ghost:** No fill or border. Platinum-mist text transitioning to crisp white on interaction.

### Cards & Glass Panels
Crafted with frosted liquid glass panels: `backdrop-filter: blur(20px)`, dark slate fill with 60% opacity, bordered by an asymmetrical 1px hairline stroke catching simulated light from above (15% white opacity at top fading to 2% at bottom).

### Input Fields & Controls
- **Inputs:** Dark graphite inset fill (`#121418`) with 1px border of `rgba(255, 255, 255, 0.08)`. Input values set in monospace. On focus: border shifts to platinum white (`rgba(255, 255, 255, 0.4)`) with an ambient diffused glow `0 0 12px rgba(255, 255, 255, 0.08)`.
- **Checkboxes & Radios:** Minimal graphite squares/circles with a 1px border. Checked state fills with platinum `#F5F6F8` housing an obsidian center glyph.

### Metric Chips & Status Indicators
Micro-pills with monospaced label typography (`label-sm`). Surfaces use subtle translucent mist (`rgba(255, 255, 255, 0.05)`). Positive financial yield states leverage Sovereign Mint (`#38E8A3`) at 10% fill with a 1px matching border.

### Financial Telemetry & Balance Ledger Tables
Table structures feature hairline divider rows (`rgba(255, 255, 255, 0.04)`), muted column headers in tracked JetBrains Mono, zero vertical column rules, and generous vertical cell padding (`space-md`). Hovered rows reveal a smooth, low-contrast illumination (`rgba(255, 255, 255, 0.02)`).