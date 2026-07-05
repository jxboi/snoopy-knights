# Animated unit art (KaM-style clips)

Milestone 1 of the graphics upgrade adds frame-based animation (idle / walk /
work / attack) on top of the existing static-sprite pipeline. It is **inactive
until you add art** — every unit falls back to its current static sprite or
procedural shape, so the game runs unchanged with no files present.

## Where the code lives

- `Assets/Scripts/Rendering/SpriteAnimator.cs` — plays a `Sprite[]` clip on a
  `SpriteRenderer`. Frames advance on `Time.deltaTime`, so animation pauses with
  the game and speeds up under 2× fast-forward.
- `Assets/Scripts/Rendering/SpriteBank.cs` — `Clip(type, action)` loads a clip;
  `HasClips(type)` gates everything; `UnitArtFolder(type)` maps a game unit type
  to its art folder.
- `Assets/Editor/ArtImporter.cs` — auto-slices animation strips.
- `Assets/Scripts/Units/Unit.cs` — picks idle/walk/work each frame; exposes
  `PlayAttackAnim()`. `SoldierUnit` calls it on each swing.

## Folder layout

```
Assets/Resources/Art/units/
  carrier.png            ← existing STATIC sprites still work (flat, no subfolder)
  guard.png
  warrior/               ← an ANIMATED unit: one subfolder = one unit's clips
    idle.png
    walk.png
    work.png             ← optional (workers); falls back to idle if absent
    attack.png           ← optional (soldiers/enemies)
```

`SpriteBank.UnitArtFolder` maps game types to these folders:

| Game `UnitType`            | Art folder |
|----------------------------|------------|
| Guard                      | `warrior`  |
| Archer                     | `archer`   |
| Raider                     | `goblin`   |
| Brute                      | `orc`      |
| Builder / Carrier / Farmer | `worker`   |

For **Milestone 1**, only `units/warrior/` needs to exist — that animates the
Guard end-to-end.

## Two ways to author each clip

**A. Horizontal strip (recommended).** One PNG per clip, laid out as a row of
**square** frames (frame size = image height). The importer slices it
automatically into `idle_0, idle_1, …`, bottom-center pivot.
Example: a 24×24 walk cycle of 6 frames → a 144×24 PNG named `walk.png`.

**B. Numbered frames.** If you'd rather not make strips, drop individual frames
named `walk_0.png`, `walk_1.png`, … in the same folder. The importer leaves
numbered files alone and `SpriteBank` loads them in order.

> Tiny Swords (and most CC0 packs) ship **grid** sheets — rows are different
> animations. Cut one horizontal strip per animation (option A) or export frames
> (option B). Either drops straight into the folders above.

## Team colour

Player units (warrior/archer) and enemies (goblin/orc) use different art, so
they read as opposing sides without recolouring. Enemies keep a light red wash.
True KaM-style mask recolouring is a later milestone if we want same-sprite
factions.

## Getting the CC0 art

- **Tiny Swords** — Pixel Frog, CC0. Animated medieval RTS units + buildings.
  <https://pixelfrog-assets.itch.io/tiny-swords>
- **Kenney** — CC0. Medieval RTS / tower-defense packs for buildings, tiles,
  icons. <https://kenney.nl/assets>

All CC0 (public domain) — safe to ship in the App Store build. Add any new
credits to `Assets/Resources/Art/CREDITS.txt`.
