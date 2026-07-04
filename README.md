# Snoopy Knights

An iPhone-first medieval logistics defense game built with Unity 6, inspired by
the living-economy feel of *Knights and Merchants*. Build a small settlement,
connect it with roads, keep your people fed, and survive 5 enemy waves.

## Status

**Milestone 5** — combat. The Barracks trains Guards (melee) and Archers
(ranged, with arrows). Soldiers hold a post, auto-engage enemies that come
close, and return afterwards; tap a selected soldier's destination to move
the post. Enemy raiders/brutes fight back, hunt workers, and raze buildings —
the wave system that sends them arrives in M6.

## How to run

1. Open the project with **Unity 6000.5.x**.
2. Open `Assets/Scenes/Main.unity` and press Play.
   The whole game builds itself from code at startup — the scene is empty on
   purpose, and all graphics are procedural placeholders (no art assets).
3. For iOS: `File > Build Settings > iOS`. Player settings (landscape-only,
   bundle id) are pre-configured via `Tools > Snoopy Knights > Configure Project`.

## Controls (touch-first)

| Gesture | Action |
|---|---|
| Drag | Pan camera |
| Pinch | Zoom |
| Tap | Select building / unit |
| Build button | Open build menu, tap a card, tap the map to aim, Place/Cancel |
| Road card | Drag on the map to paint roads, then Done |

In the editor: left-drag pans, mouse wheel zooms, click selects.

## Architecture

Everything is created from code at runtime (`GameBootstrap` →
`Game` composition root), so there are no prefabs or scene wiring to maintain.

```
Assets/Scripts/
  Core/          Bootstrap, composition root, input routing, selection
  Grid/          Tile map data, map generation, tile rendering
  CameraControl/ Pan/zoom/clamp camera
  Rendering/     Procedural placeholder sprites
  Pathfinding/   (M3) A* over the tile grid, roads preferred
  Resources/     (M2) Wood / Stone / Food / Gold stock
  Buildings/     (M2) Building defs, construction, production
  Units/         (M3+) Workers, soldiers, enemies
  Economy/       (M3+) Hauling jobs, food consumption
  Combat/        (M5) Health, attacks, projectiles
  Waves/         (M6) Enemy wave scheduling
  Mission/       (M6) Objectives, win/lose
  UI/            (M2+) Mobile HUD, menus, panels
  Save/          (M7) JSON save/load
  Audio/         (M7) Procedural placeholder SFX
```

## Design constraints

- One polished mission, small map, no multiplayer/editor/large armies.
- Touch only: no right-click, no box selection, big buttons.
- Simulation and UI stay decoupled; UI observes game state via events.
