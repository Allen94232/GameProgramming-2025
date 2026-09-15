# Campus Dash

[![Play on itch.io](https://img.shields.io/badge/Play%20on-itch.io-FA5C5C?style=for-the-badge&logo=itch.io&logoColor=white)](https://allen94232.itch.io/campusdash)

A browser-based 2D bicycle racing game set on a busy campus. Reach class before time runs out while avoiding obstacles, interacting with the environment, and keeping the rider's mood high.

## Gameplay

The campus becomes a racetrack when the player realizes class is about to start. Each level combines navigation, time pressure, environmental interactions, and score optimization.

### Core Systems

- Time-limited bicycle racing
- Mood-driven movement speed
- Coins, pedestrians, traffic obstacles, pigeons, and environmental hazards
- Mouse-driven environmental interactions
- Tutorial and two main levels
- Online leaderboards through LootLocker
- Browser deployment through Unity WebGL

## Controls

| Input | Action |
| --- | --- |
| `W` / `S` | Accelerate / brake |
| `A` / `D` | Turn left / right |
| `Left Shift` | Additional movement input used by the bicycle controller |
| `Space` | Ring the bicycle bell |
| Mouse drag | Interact with supported environmental objects |

## Objects and Obstacles

| Object | Gameplay effect |
| --- | --- |
| Coins | Increase mood and contribute to the run |
| Traffic cones and trash cans | Block the route |
| Pedestrians | Reduce mood after a collision |
| Birds | Can be moved by ringing the bell |
| Bird droppings | Slow the player and reduce mood |
| Speed cameras | Penalize speeding |
| Fire hydrants | Create an interactive road hazard |
| Cars | Moving traffic hazards |
| Finish flag | Completes the level |

## Scoring

The score combines the player's remaining time, mood, and collected coins. Separate leaderboard entries are managed for the playable levels.

## Project Structure

```text
.
├── Assets/
│   ├── Scenes/
│   │   └── Levels/       # Tutorial, Level 1, and Level 2
│   └── Scripts/          # Gameplay, UI, audio, and leaderboard logic
├── Packages/             # Unity package definitions
├── ProjectSettings/      # Unity project settings
└── WebGL Builds/         # Browser build committed to the repository
```

## Key Code

| Component | Responsibility |
| --- | --- |
| [`PlayerController.cs`](Assets/Scripts/PlayerController.cs) | Bicycle movement and player input |
| [`GameManager.cs`](Assets/Scripts/GameManager.cs) | Level flow, timer, score, and game state |
| [`MoodController.cs`](Assets/Scripts/MoodController.cs) | Mood state and related gameplay changes |
| [`LeaderboardManager.cs`](Assets/Scripts/LeaderboardManager.cs) | LootLocker leaderboard integration |
| [`Pedestrian.cs`](Assets/Scripts/Pedestrian.cs) | Pedestrian movement and collision behavior |
| [`pigeon.cs`](Assets/Scripts/pigeon.cs) | Bird behavior |
| [`Draggable.cs`](Assets/Scripts/Draggable.cs) | Mouse-driven object interaction |
| [`VehicleSpawner.cs`](Assets/Scripts/VehicleSpawner.cs) | Traffic spawning |

## Development

- Unity `6000.2.2f1`
- C#
- Universal Render Pipeline `17.2.0`
- LootLocker SDK `6.4.0`
- WebGL

Verified third-party software and bundled asset acknowledgments are listed in [CREDITS.md](CREDITS.md).

### Run in Unity

1. Install Unity `6000.2.2f1`.
2. Clone the repository and add it through Unity Hub.
3. Open a scene under `Assets/Scenes/Levels`.
4. Enter Play Mode.

### Play the Published Build

[Play Campus Dash on itch.io](https://allen94232.itch.io/campusdash)

## Version History

| Version | Date | Change |
| --- | --- | --- |
| 1.0.0 | 2025-12-08 | Added Level 2 |
| 0.8.0 | 2025-12-07 | Added tutorial level and UI updates |
| 0.6.2 | 2025-12-01 | Updated coin art and Chinese-name display |
| 0.6.1 | 2025-11-25 | Added level selection and instructions |
| 0.6.0 | 2025-11-24 | Completed Level 1 design, settings, and player art |
| 0.5.0 | 2025-11-10 | Added Level 1 art, music, and sound effects |
| 0.1.1 | 2025-11-10 | Added leaderboard functionality |
