# Sci-Fi FPS Project

A fast-paced Sci-Fi First-Person Shooter (FPS) built with **Unity 6**. Navigate hostile futuristic installations, engage autonomous robotic sentries, master an arsenal of energy and explosive weapons, and complete tactical objectives across multi-stage campaigns.

---

## 🎮 Game Overview

In this game, players infiltrate modular sci-fi outposts and fortified facilities filled with hostile security bots and automated defense systems. Combining tight gunplay, vertical jetpack mobility, and strategic objective completion, players must fight their way to extraction zones while managing health, ammo, and weapon heat.

---

## 🌟 Key Features

- **Fast-Paced Gunplay**: Tight, responsive first-person movement featuring sprinting, crouching, aiming down sights (ADS), and vertical jetpack thrusting.
- **Dynamic Arsenal**: Carry and swap between multiple weapon types including rapid-fire plasma blasters, spread shotguns, and explosive projectile launchers.
- **Intelligent Enemy AI**: Patrolling hover drones and automated turrets equipped with line-of-sight detection modules, target tracking, and dynamic NavMesh pathfinding.
- **Interactive Objective System**: Live in-game compass, waypoint markers, objective toasts, and mission trackers (kill targets, collect items, reach extraction points).
- **Multi-Level Progression**: Includes the custom **Odex Campaign** levels as well as classic arena challenge maps.
- **Full HUD Suite**: Dynamic crosshairs, damage feedback indicators, shield/health bars, ammo counters, and weapon overheat gauges.

---

## 🔫 Weapons & Equipment

| Weapon / Gear | Description | Fire Mode |
| :--- | :--- | :--- |
| **Blaster** | Standard-issue energy rifle firing high-velocity plasma bolts. Reliable at medium range with steady rate of fire. | Full-Auto / Semi-Auto |
| **Shotgun** | Close-quarters scatter cannon firing a spread of energy pellets. High burst damage at point-blank range. | Pump / Burst Spread |
| **Launcher** | Heavy ordnance weapon launching explosive charges with area-of-effect blast damage. | Heavy Projectile |
| **Jetpack** | Thruster pack providing vertical lift and aerial maneuvering to scale ramps, rooftops, and avoid hazards. | Thruster Boost |
| **Pickups** | Health packs, ammo crates, and fuel canisters scattered across the battlefield to replenish vitality and ammo. | Instant / Interactive |

---

## 🤖 Enemies & Threats

- **HoverBot**: Agile flying drones that patrol designated routes. Upon detecting the player, they close distance, hover into attack position, and fire tracking energy salvos.
- **Turret**: Heavy stationary sentry emplacements guarding choke points. Automatically pivots toward targets in its cone of vision and opens rapid suppressive fire.
- **Environmental Hazards**: Lava pits, laser grids, and falling hazard zones requiring precise platforming and jetpack navigation.

---

## 🗺️ Campaign & Level Structure

The game includes two distinct gameplay tracks:

### 1. Odex Campaign
- **Menu** (`Assets/Odex/Scenes/Menu.unity`): Main title screen with level selection and settings.
- **Level 1** (`Assets/Odex/Scenes/Leavel1.unity`): Introductory sector focusing on movement mechanics, basic gunplay, and initial encounters.
- **Level 2** (`Assets/Odex/Scenes/Leavel2.unity`): Expanded tactical corridors with elevated platforms and patrolling drone guards.
- **Level 3** (`Assets/Odex/Scenes/Leavel3.unity`): High-intensity encounter area with multiple defensive turrets and tight corners.
- **Level 4** (`Assets/Odex/Scenes/Leavel4.unity`): Climax facility test featuring combined arms combat and environmental challenges.

### 2. Tactical Arena
- **Intro Menu** (`Assets/FPS/Scenes/IntroMenu.unity`): Mission briefing and menu.
- **Secondary Arena** (`Assets/FPS/Scenes/SecondaryScene.unity`): Comprehensive dungeon and arena map featuring full objective flow (eliminate hostiles, gather key items, reach extraction).
- **Victory & Defeat** (`WinScene.unity` / `LoseScene.unity`): Post-mission outcome screens with replay and return options.

---

## ⌨️ Controls & Keybindings

| Action | Keyboard / Mouse | Gamepad |
| :--- | :--- | :--- |
| **Move** | `W`, `A`, `S`, `D` | Left Stick |
| **Look / Aim** | Mouse Movement | Right Stick |
| **Fire Weapon** | Left Mouse Button | Right Trigger |
| **Aim Down Sights (ADS)** | Right Mouse Button | Left Trigger |
| **Jump** | `Space` | South Button (`A` / `Cross`) |
| **Jetpack Thrust** | Hold `Space` (in air) | Hold South Button |
| **Sprint** | Left Shift | Left Stick Click (`L3`) |
| **Crouch** | `C` or Left Control | Right Stick Click (`R3`) |
| **Switch Weapon** | Mouse Scroll Wheel / `1`, `2`, `3` | D-Pad / Shoulder Bumpers |
| **Pause / Menu** | `Escape` | `Start` / `Options` |

---

## 🛠️ Technical Specifications

- **Engine Version**: Unity 6 (`6000.6.9f1`)
- **Render Pipeline**: Universal Render Pipeline (URP)
- **AI & Pathfinding**: Unity AI Navigation (`com.unity.ai.navigation` 2.0.16)
- **UI Framework**: Unity UGUI + TextMesh Pro
- **Platform**: PC (Linux / Windows / macOS)

---

## 🚀 How to Play

1. Open the project in **Unity Hub** using **Unity 6000.6.9f1** or newer.
2. In the Project window, navigate to [Assets/Odex/Scenes/Menu.unity](file:///home/omarhanykasban/GitHub/unity-project/Assets/Odex/Scenes/Menu.unity) or [Assets/FPS/Scenes/IntroMenu.unity](file:///home/omarhanykasban/GitHub/unity-project/Assets/FPS/Scenes/IntroMenu.unity).
3. Press the **Play** button at the top of the Unity Editor.
4. Select a mission, eliminate hostile machines, and complete your objectives!
