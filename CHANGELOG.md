# Changelog

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
