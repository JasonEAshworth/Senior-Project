# Rise of the Restless

A top-down co-op arcade dungeon crawler for 1 to 4 players on one screen. It began as my
undergraduate senior capstone at Shawnee State University, "Senior Project 14": a team project
built in Unity 4.6 in 2014 and 2015 (the git history lists everyone who worked on it). In 2026 it
was upgraded to Unity 6 and finished as a complete arcade game under its original name.

**Play it:** https://jasoneashworth.github.io/Senior-Project/ (desktop browser, keyboard or gamepads)

## The game

Clear four chapters, in any order, to earn four seals. The seals open the throne room, where only
King Restless waits.

| Chapter | Look | Enemies | Mini-boss |
| --- | --- | --- | --- |
| The Catacombs | Blue ghostfire and mist | Restless, Crawlers, Bone Archers | The Gravewarden |
| The Torture Halls | Red light and dust | Charging Jailers, Prisoners, Crossbowmen | The Warden |
| The Cellars | Lamplight | Ghouls that split in two, Gnawers, Spitters | The Butcher |
| The Forge Depths | Fire and embers | Exploding Embers, charging Smiths, Cinder Throwers | The Forgemaster |
| The Throne Room | Violet | His army, in the last phase | King Restless |

- **Four heroes:** Warrior, Sorcerer, Rogue, and Woodsman, each with their own attacks and ability.
- **Drop in, drop out:** a new player presses jump or start at any time, on the title screen, in
  the hub, or in the middle of a chapter.
- **Lives and continues:** 3 lives each and 3 shared continues. A hero at zero health goes down;
  a teammate who stands close for two seconds revives them.
- **Score:** points per kill with a combo multiplier, a bonus for clearing a room without taking
  damage, gold pickups, a rank after each chapter, and a local high-score table with initials.
- **Upgrades:** spend gold in the hub on health, damage, speed, and extra lives.
- **Power-ups:** enemies sometimes drop fury, haste, shield, or heal orbs.
- **Difficulty** grows with the number of players and with every chapter already cleared.
- Every mini-boss attack (slam, charge, summon, fire ring, hook, volley) shows a warning on the
  floor before it lands.

## Players and controls

Up to four players share one screen. Each player brings their own device and presses **jump** (or
**start** on a gamepad) to take the next free slot: on the title screen, in the hub, or in the
middle of a chapter. Two players can share one keyboard; everyone else uses a gamepad.

| Slot | Color | Typical device |
| --- | --- | --- |
| P1 | Red | Keyboard, left side (WASD), or the first gamepad |
| P2 | Blue | Keyboard, right side (arrows), or a gamepad |
| P3 | Green | Gamepad |
| P4 | Yellow | Gamepad |

Each player picks a hero with left and right in the lobby. A player can drop out from the pause
menu, or by holding the ability button in the hub.

These are the defaults. **Controls (F1)** on the title screen and in the hub, **Controls** in the
pause menu, or a gamepad's Back button opens a screen that rebinds every key for both keyboard
layouts and every gamepad button. A key already in use swaps with the one it replaces, and the
choices are saved.

| Action | Keyboard, left (WASD) | Keyboard, right (arrows) | Any gamepad |
| --- | --- | --- | --- |
| Move | W A S D | Arrow keys | Left stick or d-pad |
| Jump, join | Space | Right Shift or Numpad 0 | A (south) |
| Attack | J | Comma or Numpad 1 | X (west) |
| Class ability | K | Period or Numpad 2 | B (east) |
| Use potion, buy in hub | L | Slash or Numpad 3 | Y (north) |
| Pause | Esc | Backspace | Start |

Enter (or start on any joined gamepad) begins the game from the lobby.

## Open and build

1. Install **Unity 6.3 LTS (6000.3.29f1)** with **Web Build Support** from Unity Hub.
2. Open this folder in Unity Hub. The first import takes several minutes.
3. Open `Assets/Scenes/Title.unity` and press Play.

Builds use the **Tools > Rise of the Restless** menu, or the command line:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.29f1\Editor\Unity.exe" -batchmode -quit -projectPath . -buildTarget WebGL -executeMethod ProjectTools.BuildWebGL
```

Every build first runs **Prepare Game Content**, which rebuilds the room catalog and creates any
missing chapter assets and scenes. The WebGL build goes to `Build/WebGL`; the live site is that
folder, published on the `gh-pages` branch.

### Store art (optional)

Some enemies wear models from the Asset Store, and the Rogue and the Woodsman wear Mixamo
characters. Their licenses forbid sharing the files, so they are not in this repo. Without them,
those enemies stay zombies, the two heroes keep their 2015 placeholder look, and everything else
works.

| Pack | Publisher | Used for |
| --- | --- | --- |
| Dungeon Skeletons Pack | Polygon Blacksmith | Skeleton enemies and the Gravewarden |
| Giant Spiders Animated | Starsupial | Cellars spiders and tarantulas |
| LowPoly Fantasy Monsters Pack (free demo) | TS WORK | Goblins, the hobgoblin Jailer, wolves, the troll Butcher |

Get each pack on the Asset Store, download it in **Window > Package Manager > My Assets**, and
import it. The next build moves it to `Assets/Store` (git-ignored), shrinks its textures for the
web build, and builds the skins. **Rebuild Enemy Skins** does the same on demand.

| Mixamo character | Used for |
| --- | --- |
| Heraklios By A. Dizon | The Rogue |
| Erika Archer With Bow/Arrow | The Woodsman |

Sign in at [mixamo.com](https://www.mixamo.com), pick each character, and download it with
**Format: FBX for Unity** and **Pose: T-pose**. Put both files, with their download names, in
`Assets/Store/Heroes`. The next build sizes them to the party, gives the Rogue the repo's
daggers, renders their HUD portraits, and builds the hero models. **Rebuild Hero Models** does the
same on demand, and **Render Hero Previews** draws the old and new heroes side by side in
`Build/hero_preview.png`.

**Automated check:** the Windows build (`Build/Windows/RiseOfTheRestless.exe`) run with `-smoketest <folder>` plays the whole game by itself
(every chapter, drop-in, revive, every boss, the ending, and a game over) and writes `report.txt`
and screenshots to the folder.

## How it fits together

- `Assets/Resources/Chapters/` holds one `ChapterDefinition` asset per chapter: dungeon length and
  room mix, starter and boss rooms, lighting, atmosphere, enemy variants, mini-boss, and music.
  Edit them in the Inspector.
- `MapManager` (the team's room-graph generator) builds each chapter from the `RoomCatalog`.
- `GameSession` keeps the players, lives, score, seals, and high scores between scenes.
- `ChapterDirector` runs a chapter: look, variants, the mini-boss, continues, game over, results.
- A variant or mini-boss can name a **skin**. `SkinSwap` hides the zombie and shows that model
  over it; the zombie's AI and attack timing stay in charge, and the skin copies its walk, run,
  attacks, flinches, and death. `SkinContent` (editor) builds the skins from store packs.
- A hero with an installed model gets it at spawn from `HeroSkin`. The hero's own Animator keeps
  its 2015 controller, clips, and attack events, and takes the model's humanoid avatar, so the
  same clips move the new body. `HeroContent` (editor) builds the models from the Mixamo files.
- Scenes: `Title` (lobby and attract mode), `Hub`, `MainScene` (every chapter), `Ending`.
- Music is original, written by `Tools/compose_music.py` (needs numpy and soundfile).

## What changed in 2026

- **Unity 4.6 to Unity 6.3.** Assets are text (YAML). Unity's API updater rewrote the removed
  Unity 4 calls.
- **Paid Asset Store packages removed** from the whole git history: Rewired (input) and Exploder
  (breakable props), plus a committed copy of Unity's `UnityEditor.dll`. Input uses Unity's Input
  System; props break with a small `Explodable` script that keeps the old script's GUID.
- **Finished as an arcade game:** lobby, hub, chapters, seals, mini-bosses, the throne room,
  lives, continues, revives, scoring, high scores, upgrades, power-ups, pause menu, and music.
- **Bugs fixed from 2015,** among them: enemies never dropped gold (rolled in a field initializer);
  the ranged zombie threw an exception every physics tick when no player was in range; King
  Restless's health bar was never assigned, so his setup threw; enemies crashed when no player was
  alive; the attack potion wiped out other damage bonuses; the camera logged on every physics
  tick; the dungeon generator listed room files on disk, which only works in the editor; the
  heroes' facing rings drew as solid gray squares, and a debug cube followed the party around.
- **Cleanup:** 319 components that pointed at deleted scripts, a corrupt scene (`Map_4_2.unity`,
  still in the history), empty prefabs, generated project files, and empty folders are gone.
- **WebGL:** textures use Crunch compression for WebGL only, keeping the download under 100 MB.
- **Ranged enemies attack.** In 2015 their shot timer was commented out and their bullet prefab
  was missing, so archers and spitters never fired.
- **New Rogue and Woodsman models** (optional, see Store art). Their 2015 textures were never
  committed, and their models were imported with Optimize Game Objects, which hides the skeleton,
  so a new model only moves after `HeroSkin` restores it.

## Known gaps from 2015

- The **Rogue** and **Woodsman** textures were never committed. Without the Mixamo characters,
  they use plain placeholder colors.
- The **potions** are untextured white models; their halo color shows the type.
- Without the store art, all enemies share the one zombie model; chapter variants differ by
  color, size, glow, stats, and behavior.

## License

The team's scripts, shaders, and the 2026 changes are [MIT licensed](LICENSE). Shader Forge (in
`Assets/Plugins/ShaderForge`) was later released as open source under the MIT license by its
author. Simple Particle Pack (in `Assets/Plugins/SimpleParticlePack`) and the Unity engine keep
their own licenses.
