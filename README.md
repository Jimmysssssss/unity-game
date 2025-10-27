# Crystal Quest 3D

Crystal Quest is a compact 3D Unity adventure built entirely with runtime-generated content. Explore a stylized valley, collect the scattered energy crystals, and escape through the portal that appears once the mission is complete.

## Gameplay Overview

- **Movement**: Use `WASD` or the arrow keys to explore the arena. Press `Space` to jump. The controls work with both the legacy Input Manager and the newer Input System package (WASD/arrow keys, Space, or a gamepad's left stick and South button). If the legacy input axes have been removed from your project, the keyboard fallback still kicks in automatically.
- **Objective**: Gather all glowing crystals that hover above the valley floor. A shimmering exit portal materializes when the last crystal is collected—step into it to win.
- **Camera**: The third-person camera smoothly follows the player, keeping the action in view.

## Project Structure

```
Assets/
├── Scenes/
│   └── MainScene.unity        # Entry point scene wired to the GameManager
└── Scripts/                   # Runtime scripts that assemble and run the experience
    ├── CameraFollow.cs
    ├── Collectible.cs
    ├── GameManager.cs
    ├── GoalArea.cs
    └── PlayerController.cs
Packages/
└── manifest.json               # Unity package dependencies
ProjectSettings/
├── EditorBuildSettings.asset   # Ensures the main scene is included in builds
└── ProjectVersion.txt          # Target Unity editor version
```

All of the level geometry, lighting accents, UI, and gameplay hooks are spawned procedurally when the scene loads—open `Assets/Scenes/MainScene.unity` and press **Play** to start.

## Getting Started

1. Open the project in Unity `2021.3.29f1` or newer.
2. Load the `MainScene` located under **Assets/Scenes**.
3. Enter Play Mode to explore, collect crystals, and find the portal.
   - Using the built-in legacy Input Manager? The controller script now falls back to raw key presses if the standard `Horizontal`, `Vertical`, or `Jump` axes are missing, so you can move immediately even in brand-new projects.
   - If your project is configured to use the **Input System (New)** backend, no additional setup is needed—the scripts automatically read from keyboard or gamepad devices.

Enjoy the hunt!
