# Souciss-FPS

<img alt="Unity Version" src="https://img.shields.io/badge/Unity-2022.3.27f1-blue.svg">
<img alt="Downloads" src="https://img.shields.io/github/downloads/Souciss12/Souciss-FPS/total">
<img alt="Last Commit" src="https://img.shields.io/github/last-commit/Souciss12/Souciss-FPS">

## 📝 Description
Souciss-FPS is a simple customizable multiplayer first-person shooter built with Unity.

## ✨ Features
- Smooth first-person character controller
- Configurable walking and running speeds
- Jumping mechanics with adjustable jump height
- Camera control with customizable sensitivity and rotation limits
- Realistic customizable weapon sway effect for immersive gameplay
- Easy customizable weapon spread system and muzzle flash
- Easy customizable weapon target hit effect
- Simple project structure for easy expansion
- Fully customizable through the Unity Inspector

## 🔧 Requirements
- Unity 2022.3.27f1 or newer
- Basic knowledge of Unity and C#

## 🚀 Installation
1. Clone this repository:
   ```bash
   git clone https://github.com/Souciss12/Souciss-FPS.git
   ```
2. Open the project in Unity Hub using Unity 2022.3.27f1 or later.
3. Before doing anything you need to change the Photon AppId in the assets/Photon/ with your Photon AppId wich can be found be creating one on photonengine.com
4. Open the `MainMenu` and launch the game to explore the demo features.

## 🎮 Controls
- **W, A, S, D**: Movement
- **Mouse**: Look around
- **Space**: Jump
- **Left Shift**: Run
- **R**: Reload weapon

## 🛠 Customization
The FPS Controller and Weapon components can be easily customized through the Unity Inspector:

### FPS Controller Parameters:
- Walk Speed
- Run Speed
- Jump Power
- Gravity
- Mouse Sensitivity
- Look X Limit (vertical rotation limit)

### Gun Controller Parameters:
- Fire Rate
- Clip Size
- Ammo Capacity (Capacity in reserve)
- Muzzle Flash Images
- Weapon Position + Weapon Position with Scope
- Weapon Rotation + Weapon Rotation with Scope
- Fire Spread + Fire Spread with Scope
- Max Fire Spread Increase
- Fire Spread Increase per Shot
- Fire Spread Recovery Time
- Enemy Impact Effect
- Surface Impact Effect
- Fire Impact Force

### Weapon Sway Parameters:
- Sway Amount
- Max Sway Amount
- Sway Smoothness
- Rotation Amount
- Max Rotation Amount
- Rotation Smoothness

## 📋 Planned Features
- [ ] Customizable weapon recoil
- [ ] Footstep sounds based on surface material
- [x] Fire impacts sounds based on target type
- [x] Basic health system
- [x] Multiplayer capabilities

## 📦 Releases
### [v0.2.0 - Souciss-FPS](https://github.com/Souciss12/Souciss-FPS/releases/tag/v0.2.0)
- Implemented multiplayer functionality using Photon PUN
- Added basic health system
- Added player synchronization across network
- Fire impacts sounds based on target type
- Implemented networked weapon shooting and damage
- Added lobby system for creating and joining games

### [v0.1.0 - Souciss-FPS](https://github.com/Souciss12/Souciss-FPS/releases/tag/v0.1.0)
- Basic FPS controller implementation
- Camera controls with mouse
- Walking and running mechanics
- Jumping functionality
- Weapon system with shooting mechanics
- Simple weapon sway effect
- Basic attackable enemy
- Demo scene with basic environment

### Upcoming in v0.3.0
- Customizable weapon recoil
- Footstep sounds based on surface material
- Advanced health system with regeneration
- More multiplayer game modes
- Player customization options
- Enhanced UI for multiplayer lobbies

## 📸 Screenshots
- None currently

## 🤝 Contributing
Contributions are welcome! Feel free to fork the project and submit pull requests.

## 📞 Contact
If you have any questions or suggestions, please open an issue or join my [discord](https://discord.com/invite/fe2RfUPkBu).

⭐ Don't forget to star this repository if you find it useful! ⭐
````
