# Snoopy Knights

An iPhone-first medieval logistics defense game built with Unity 6, inspired by
the living-economy feel of *Knights and Merchants*. Build a small settlement,
connect it with roads, keep your people fed, train defenders, and survive
5 enemy waves.

**Complete** — one polished mission, playable start to finish with touch only.

## The mission

You start with a Town Center and six villagers. Build Woodcutters and a Quarry,
feed everyone with Farms (a staffed Kitchen doubles their yield), raise Houses
to grow your population, and train Guards, Archers and Watchtowers before the
raiders arrive. Waves attack roughly every 2–2.5 minutes and get harder; kills
and cleared waves pay gold, which funds more soldiers. Survive all 5 waves to
win. Lose the Town Center (or all your storage) and it's over.

## How to run

1. Open the project with **Unity 6000.5.x**.
2. Open `Assets/Scenes/Main.unity` and press Play.
   The whole game builds itself from code at startup — the scene is empty on
   purpose, and all graphics/audio are procedural placeholders (zero assets).

### Building for iPhone

1. `File > Build Settings`, switch platform to **iOS** (scene list and player
   settings — landscape-only, bundle id `com.jx.snoopyknights`, IL2CPP — are
   already configured; re-apply anytime via `Tools > Snoopy Knights > Configure Project`).
2. Build, open the generated Xcode project, set your signing team, run on device.

## Controls (touch only)

| Gesture | Action |
|---|---|
| Drag | Pan camera |
| Pinch | Zoom |
| Tap | Select building / unit; tap again or tap grass to deselect |
| Build button | Open build menu → tap a card → tap the map to aim → Place |
| Road card | Drag on the map to paint roads (1 stone/tile), then Done |
| Selected soldier + tap | Move that soldier's guard post |
| II button | Pause: resume / save / load / restart |

In the editor: left-drag pans, mouse wheel zooms, click taps.

## How the economy works

- Production buildings need a **Worker**. Woodcutter/Quarry workers walk out,
  chop trees / mine rocks (tiles deplete!), and bring goods home. Farmers work
  on site.
- Output piles up at the building (visible dots) until a **Carrier** hauls it
  to the Town Center or a Storehouse — resources only count once delivered.
- **Builders** construct whatever you place, automatically.
- Everyone eats: 1 food per villager per 45s. An empty pantry slowly starves
  your people.
- Roads: workers path along them (half path cost) and walk 1.5× faster.
- Workers manage themselves; the only units you ever command are soldiers.

## Architecture

Everything is created from code at runtime (`GameBootstrap` → `Game`
composition root), so there are no prefabs and no scene wiring. Simulation and
UI are decoupled: UI observes events / polls read-only state.

```
Assets/Scripts/
  Core/          Bootstrap, composition root, gesture input router, selection
  Grid/          Tile map data, deterministic map generation, tile rendering
  CameraControl/ Pan/zoom/clamp orthographic camera
  Rendering/     Procedural sprites, world-space bars, fade FX
  Pathfinding/   A* over the tile grid, roads preferred
  Resources/     Wood/Stone/Food/Gold stock
  Buildings/     Defs, construction, placement/road input modes, training
  Units/         Worker AIs (builder/carrier/worker), soldiers, enemies
  Economy/       Population cap, food consumption, kitchen bonus
  Combat/        IDamageable, projectiles, watchtower fire
  Waves/         5-wave scheduler with edge spawning
  Mission/       Objectives, victory/defeat rules
  UI/            Code-built mobile HUD (safe-area aware, thumb-sized buttons)
  Save/          JSON save/load via scene reload
  Audio/         Synthesized placeholder SFX
Assets/Editor/   Project setup tool + headless play-mode smoke test
```

## Testing

`Assets/Editor/SmokeTest.cs` boots the game headlessly, fast-forwards past the
first enemy wave (~200 sim-seconds), asserts the simulation actually progressed
and that nothing threw:

```
Unity -batchmode -projectPath . \
  -executeMethod SnoopyKnights.EditorTools.SmokeTest.Run -logFile -
```

## Design constraints

- One mission, small map (36×26). No multiplayer, editor, big armies, box
  selection, or right-click — by design.
- Save is a snapshot of stock/map/buildings/units/waves; in-flight worker jobs
  are not saved (workers re-acquire them on load).
