# Changelog

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
