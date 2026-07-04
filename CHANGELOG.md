# Changelog

## Milestone 7 — 2026-07-04

- Pause menu (top-right): resume, save, load, restart. Pausing freezes the sim.
- Basic save/load: JSON snapshot of resources, tiles/roads, buildings
  (construction progress, health, output, training queues), units, wave state
  and camera; restored through a clean scene reload. Worker jobs re-acquire.
- Placeholder audio: all SFX synthesized at startup (taps, placement,
  completion chime, arrows, hits, wave horn, victory/defeat jingles) — zero
  audio assets, rate-limited to avoid spam.
- Headless play-mode smoke test (`SmokeTest.Run`): boots the game in batch
  mode, fast-forwards past wave 1, fails on any logged exception or if the
  simulation didn't progress. Passing at 200 sim-seconds with 0 errors.
- Fix: storage "alive" check no longer counts the building currently being
  destroyed (defeat detection during the destruction event).

## Milestone 6 — 2026-07-04

- Wave system: 5 hand-tuned waves (raiders, later brutes) spawning at the
  north/east/west map edges; ~2-2.5 minute countdowns between waves; waves
  never overlap; gold bonus per cleared wave.
- Watchtower combat: auto-targets the nearest enemy in 5.5 tiles, fires arrows.
- Mission controller: objectives checklist (economy, food stock, defender,
  survive waves), victory on final wave cleared, defeat when the Town Center
  or all storage is destroyed.
- UI: wave countdown banner (turns red during attacks), objectives panel,
  full-screen victory/defeat overlay with restart.
- Restart reloads the scene cleanly (bootstrap re-runs on scene load).

## Milestone 5 — 2026-07-04

- `IDamageable` shared by units and buildings; buildings use footprint-edge
  distance for range checks.
- Guards (melee) and Archers (homing arrow projectiles) trainable at the
  Barracks. Post-and-leash AI: engage nearby enemies, don't chase forever,
  return to post. Fight back when shot.
- One-finger soldier orders: with a soldier selected, tap open ground to move
  its post (green marker feedback). No box selection, no right-click.
- Enemy AI: raiders and brutes chase nearby defenders/workers, otherwise march
  on buildings (watchtowers preferred) and raze them; retarget onto attackers;
  drop gold bounties when killed (4/10).

## Milestone 4 — 2026-07-04

- Food consumption: the settlement eats 1 food per unit per 45s; starvation
  deals slow damage to all units and flashes the food counter red.
- Kitchen bonus: farms yield 2 food per cycle while a staffed Kitchen operates.
- Population cap from housing (Town Center +6, House +3), shown in the top bar.
- Unit training: Town Center trains Builder/Carrier/Worker, Barracks will train
  Guard/Archer (combat AI lands in M5). Queue with progress, costs paid upfront,
  capped by population.

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
