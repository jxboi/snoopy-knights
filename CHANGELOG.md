# Changelog

## Milestone 3 — 2026-07-04

- A* pathfinding (4-dir); road tiles cost half, so paths prefer roads and
  units move 1.5x faster on them.
- Builder units: auto-claim construction sites (spread across sites), walk
  there, build. Buildings no longer self-construct.
- Production workers: staff Woodcutter/Quarry (walk out, chop/mine nearby
  tiles, deplete them), Farm (work on site). Output buffers on the building
  with visible dots, capped until carriers pick it up.
- Carrier units: haul goods from production buildings to nearest storage;
  resources count only on delivery. Cargo shown above their heads.
- Trees/rocks deplete and vanish; workers report "no trees in range".
- Unit selection + info panel (role, status, HP). Units nudged aside when a
  building is placed on top of them.
- 6 starting units: 2 builders, 2 carriers, 2 workers.

## Milestone 2 — 2026-07-04

- Resource stock (wood/stone/food/gold) with top resource bar UI.
- Building definitions for all 9 buildings; Storehouse/Woodcutter/Quarry and the
  rest are placeable now (production logic lands with workers in M3).
- Town Center pre-placed at mission start; camera centers on it.
- Placement flow: build menu card → ghost with green/red footprint + entrance
  marker → Place/Cancel buttons. Drag still pans while placing.
- Road painting: drag to paint connected road strips, 1 stone per tile.
- Construction states with progress bars (self-building until Builders in M3).
- Building selection panel: state, HP, description, demolish (50% refund).
- Safe-area aware, code-built uGUI HUD (1920x1080 reference, thumb-sized buttons).

## Milestone 1 — 2026-07-04

- Unity 6 project skeleton (no templates, minimal packages, old input manager).
- Procedural placeholder sprites (`SpriteFactory`) — zero art assets.
- 36×26 tile map with grass, forest, rock and water, generated deterministically.
- Grid renderer with per-tile sprites and road-strip rendering (roads land in M2).
- Orthographic camera: one-finger pan, pinch zoom, map clamping; mouse fallback in editor.
- `InputRouter` gesture state machine (tap / drag / pinch, UI-aware) with pluggable input modes.
- Tap-to-select tile highlight.
- Editor tool `Tools > Snoopy Knights > Configure Project`: creates the empty main
  scene, build settings, landscape-only iOS player settings.
