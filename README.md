# Senior Project 14

My undergraduate senior capstone at Shawnee State University: a top-down co-op dungeon crawler for
1 to 4 players on one screen. It was a team project, built in Unity 4.6 in 2014 and 2015 (the git
history lists everyone who worked on it). In 2026 it was upgraded to Unity 6 so it opens, builds,
and plays in a browser again.

**Play it:** https://jasoneashworth.github.io/Senior-Project/ (desktop browser, keyboard or gamepad)

## Gameplay

- **Four classes**, each with its own attacks and abilities: Warrior, Sorcerer, Rogue, and
  Woodsman (with a hawk companion).
- **Rooms** are typed (starter, horde, puzzle, reaction, treasure, boss) and joined as a graph by
  `MapManager`. The shipped dungeon is a preset run of four rooms: a starter room (find the key
  to open the door), a reaction room, a horde room, and a boss room.
- **Enemies:** melee, ranged, and horde types, plus bosses (King Restless and King Firestorm).
- **Hazards:** spike, flame, and projectile traps, moving platforms, and pits.
- **Pickups:** gold and coins for score, and health, haste, and attack potions.
- Players must stay near each other: the camera frames the whole party.

## Controls

Choose the number of players and each player's class on the title screen, then press Enter or
Start.

| Action | Player 1 | Player 2 | Gamepad |
| --- | --- | --- | --- |
| Move | W A S D | Arrow keys | Left stick or d-pad |
| Jump | Space | Right Shift or Numpad 0 | A (south) |
| Attack | J | Comma or Numpad 1 | X (west) |
| Class ability | K | Period or Numpad 2 | B (east) |
| Use potion | L | Slash or Numpad 3 | Y (north) |

Gamepad N belongs to player N, so players 3 and 4 need gamepads.

## Open and build

1. Install **Unity 6.3 LTS (6000.3.29f1)** with **Web Build Support** from Unity Hub.
2. Open this folder in Unity Hub. The first import takes several minutes.
3. Open `Assets/Scenes/Title.unity` and press Play.

To build, use the **Tools > Senior Project** menu (Build WebGL, Build Windows), or run it from the
command line:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.29f1\Editor\Unity.exe" -batchmode -quit -projectPath . -buildTarget WebGL -executeMethod ProjectTools.BuildWebGL
```

The WebGL build goes to `Build/WebGL`. The live site is that folder, published on the `gh-pages`
branch.

## What changed in 2026

- **Unity 4.6 to Unity 6.3.** Assets are now text (YAML) instead of binary, so git can diff them.
  Unity's API updater rewrote the removed Unity 4 shortcuts (`rigidbody`, `renderer`, and so on).
- **Paid Asset Store packages removed.** Rewired (input) and Exploder (breakable props) were
  deleted from the whole git history, together with a committed copy of Unity's own
  `UnityEditor.dll`. Input now uses Unity's Input System. Props now break with a small
  `Explodable` script written for this upgrade, which keeps the old script's GUID so the props
  link to it unchanged.
- **Title screen.** It picks 1 to 4 players and their classes. Before, the scene always spawned
  four heroes, and since nobody may stray far from the group, one player alone could not move.
- **Movement follows the screen.** The camera looks along -Z, so the new input is turned to match
  it.
- **Bug fixes:** enemies never dropped gold, because the drop count was rolled in a field
  initializer, which Unity does not allow; the attack timers had the same problem; a camera script
  wrote to the console every physics tick.
- **Cleanup:** 319 components that pointed at deleted scripts, four empty prefabs, generated
  `.csproj` files, and empty folders are gone. `Assets/Map_4_2.unity` was corrupt in its last
  2015 commit and was removed; older versions are still in the history.
- **WebGL:** textures use Crunch compression for WebGL only, which brings the download to about
  80 MB.

## Known gaps from 2015

- The **Rogue** and **Woodsman** textures were never committed. They now use plain placeholder
  colors (charcoal and green) instead of white.
- The **potions** are untextured white models; their halo color shows the type.
- `MapManager`'s procedural mode reads room prefabs from the `Assets` folder on disk, so it works
  only in the editor. The shipped scene uses the preset dungeon.

## License

The team's scripts, shaders, and the 2026 changes are [MIT licensed](LICENSE). Shader Forge (in
`Assets/Plugins/ShaderForge`) was later released as open source under the MIT license by its
author. Simple Particle Pack (in `Assets/Plugins/SimpleParticlePack`) and the Unity engine keep
their own licenses.
