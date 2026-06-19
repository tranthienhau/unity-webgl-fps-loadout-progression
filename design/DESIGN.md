# Design System - "Neo-Tactical Protocol"

Design-first source for the Unity build. Generated with Google Stitch (DESKTOP, 16:9),
design system `assets/c4ca8533c8634dbab8f22c8372b52e7d`, project `13180754862116715896`.
See `screens.txt` for screen IDs, `01-loadout.html` + `01-loadout.png` for the rendered reference.

## Brand
Cyber-Brutalist AAA esports HUD. Sharp angles, chamfered corner cuts, neon edge glow,
scanline overlay, data-dense but rigorously aligned.

## Color
| Token | Hex | Use |
|---|---|---|
| surface | `#0E1116` | screen background |
| panel | `#1A1E26` | cards / panels |
| panel-low | `#191C21` | nav rail, recessed |
| primary (cyan) | `#27E1FF` | accent, active nav, primary button, stat fill, glow |
| secondary (orange) | `#FF6A2C` | legendary/new/premium highlights, alerts |
| text | `#E1E2E9` | body |
| text-dim | `#859397` | labels, captions |
| outline | `#3B494C` | 1px borders |

Rarity ramp: Common `#9AA3A8`, Rare `#27A8FF`, Epic `#A35BFF`, Legendary `#FF6A2C`.

## Type
- Display / titles: **Orbitron** 700, tight tracking, UPPERCASE
- Headings / stats labels: **Rajdhani** 600, wide tracking
- Labels / numbers: **JetBrains Mono** (stat values, currency, serial codes)
- Body: **Inter**

## Components
- **Chamfer card**: `clip-path` polygon cutting top-right + bottom-left 12px; 1px outline;
  on focus -> cyan border + outer glow `0 0 15px rgba(39,225,255,.15)`.
- **Primary button**: solid cyan, black text, top-right 10px cut, glow on hover.
- **Outline button**: 1px cyan border, cyan text, transparent fill.
- **Stat bar**: 4px track `rgba(255,255,255,.05)`, cyan fill with `0 0 8px` glow, animates from 0 width.
- **Chip/tag**: small dark box, JetBrains Mono caps; orange variant for LIVE/NEW/LEGENDARY.
- **Scanline overlay**: full-screen 4px horizontal gradient at ~20% opacity.
- **Nav rail**: fixed left 256px, icon + caps label; active item = cyan text + `bg-primary/10` + 4px right border.

## Screens
1. **Loadout** (`01`): left nav rail; center weapon preview panel with weapon name overlay;
   bottom row of 5 loadout slots (PRIMARY/SECONDARY/MELEE/TACTICAL/LETHAL), primary card cyan-bordered
   with LEGENDARY tag; right WEAPON SPECS (6 stat bars: DAMAGE/FIRE RATE/RANGE/ACCURACY/MOBILITY/CONTROL)
   + MODIFICATIONS grid (OPTIC/BARREL/MAGAZINE/GRIP, EQUIPPED chips); CUSTOMIZE + EQUIP LOADOUT buttons.
2. **Cosmetic Store** (`02`): category tabs (ALL/WEAPON SKINS/OPERATORS/CHARMS/BUNDLES); featured
   LEGENDARY bundle hero with countdown + gem price; 3-col grid of item cards with rarity color bar,
   name, price button; top currency counters.
3. **Battle Pass** (`03`): SEASON 4, TIER badge (42/100), large XP bar to next tier, season timer;
   parallel FREE + PREMIUM reward tracks of tier nodes (claimed = cyan check, locked = lock, current
   highlighted); UPGRADE TO PREMIUM CTA; daily/weekly CHALLENGES list with XP + progress bars.
4. **Play Menu** (`04`): BREACHPOINT wordmark; vertical game-mode cards (TEAM DEATHMATCH, CAPTURE THE
   FLAG, FREE-FOR-ALL, RANKED w/ rank badge, LIMITED TIME MODE w/ NEW pulse); mode meta (player count,
   map thumb); big FIND MATCH button; squad row of 3 slots.

The Unity uGUI build reproduces this palette, type hierarchy, chamfer/glow motifs, and per-screen layout.
