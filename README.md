# Senior-Project

My undergraduate senior capstone, "Senior Project 14": a multiplayer top-down dungeon crawler
built in Unity 4.6. Players fight through interconnected, typed rooms (starter, horde, puzzle,
boss, treasure) full of enemies and traps.

## Gameplay

- **Four classes**, each with distinct abilities:
  - **Warrior:** melee combos and a whirlwind
  - **Sorcerer:** ice spikes, blizzard, fireballs, meteor
  - **Rogue:** dashes and stealth
  - **Woodsman:** arrows, a hawk companion, and bombs
- **Rooms** are generated as a connected graph (`MapManager`) with typed encounters
- **Enemies** come in melee, ranged, horde, "intelligent," and boss variants, each with its
  own AI (`EnemyBase`, `EnemyManager`)
- **Hazards:** spike and flame traps, moving platforms, falling/death zones, projectile traps,
  and locked doors
- **Progression:** score, pickups (health/attack/haste potions), mana and abilities, respawns

Shared systems live in `CharacterBase` / `PlayerBase` (health, mana, damage), with
`PlayerManager` handling lifecycle.

## Open it

Open the project folder in Unity 4.6.0.

## License

My original scripts and shaders are [MIT licensed](LICENSE). Unity and the bundled
third-party asset-store packages (Rewired for input, Exploder for destructible meshes, Shader
Forge, and Simple Particle Pack) retain their own respective licenses and are not relicensed
here.
