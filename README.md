# Souciss-FPS

<img alt="Unity Version" src="https://img.shields.io/badge/Unity-2022.3.27f1-blue.svg"> <img alt="Downloads" src="https://img.shields.io/github/downloads/Souciss12/Souciss-FPS/total"> <img alt="Last Commit" src="https://img.shields.io/github/last-commit/Souciss12/Souciss-FPS">

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
- Multiplayer system with Photon PUN integration
- Player health system with visual health bar
- Weapon switching via number keys or mouse wheel
- Advanced weapon aiming system with adjustable positions
- Multiple sound effects for weapon shoot, reload, empty clips, and impacts
- Impact effects that vary based on target type (enemy vs. surface)
- Complete lobby system for creating and joining multiplayer games
- Network synchronized player movements and actions
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
3. Open the `MainMenu` and launch the game to explore the demo features.

## 🎮 Controls
- **W, A, S, D**: Movement
- **Mouse**: Look around
- **Space**: Jump
- **Left Shift**: Run
- **Left Click**: Shoot
- **Right Click**: Aim
- **R**: Reload weapon
- **Mouse Wheel**: Change weapon
- **1-9**: Quick weapon selection (if available)

## 📋 Planned Features
- [ ] Customizable weapon recoil with vertical and horizontal components
- [ ] Footstep sounds based on surface material
- [ ] Interactive environment objects and destructible props
- [ ] Dynamic crosshair that reflects weapon accuracy
- [ ] Grenade and throwable items system
- [ ] Kill feed and scoreboard for multiplayer
- [x] Fire impacts sounds based on target type
- [x] Basic health system
- [x] Multiplayer capabilities with Photon networking
- [x] Player synchronization across network
- [x] Lobby system for multiplayer games

## 📦 Releases
### [v0.3.0 - Souciss-FPS](https://github.com/Souciss12/Souciss-FPS/releases/tag/v0.3.0)
- Multiplayer capabilities with Photon networking
- Added weapon reloading with animations and sounds
- Improved weapon aiming mechanics
- Enhanced weapon switching system
- Added empty clip sound effect
- Added additional weapon customization options

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

## 📸 Screenshots
- None currently

## 🛠 Customization
The FPS Controller and Weapon components can be easily customized through the Unity Inspector:

### FPS Controller Parameters:
- Walk Speed: Base movement speed when walking
- Run Speed: Increased movement speed when running
- Jump Power: Height of player jumps
- Gravity: Strength of gravity applied to player
- Mouse Sensitivity: Sensitivity of mouse look controls
- Look X Limit: Vertical rotation limit (prevents over-rotation)
- Max Health: Maximum player health value

### Gun Controller Parameters:
- Is Automatic: Toggle between automatic and semi-automatic firing
- Fire Rate: Time between shots (in seconds)
- Clip Size: Number of rounds per magazine
- Ammo Capacity: Total ammunition in reserve
- Muzzle Flash Images: Customizable muzzle flash sprites
- Weapon Position: Position of weapon when not aiming
- Weapon Position with Scope: Position of weapon when aiming
- Weapon Rotation: Rotation of weapon when not aiming
- Weapon Rotation with Scope: Rotation of weapon when aiming
- Aim Smoothing: Smoothness of transition when aiming
- Max Fire Distance: Maximum range of weapon
- Fire Spread + Fire Spread with Scope: Accuracy when firing from hip vs. aiming
- Max Fire Spread Increase: Maximum accuracy decrease during rapid fire
- Fire Spread Increase per Shot: Progressive accuracy decrease per shot
- Fire Spread Recovery Time: Time to recover base accuracy after firing
- Fire Spread Recovery Delay: Delay before accuracy starts recovering
- Enemy Impact Effect: Visual effect when hitting enemies
- Surface Impact Effect: Visual effect when hitting surfaces
- Impact Force: Physical force applied to hit objects
- Impact Lifetime: Duration of impact effects
- Impact Sound Volume: Volume of impact sounds
- Audio Clips: Sounds for firing, impacts, reloading, and empty clips

### Weapon Sway Parameters:
- Sway Amount: Base intensity of movement-based weapon sway
- Max Sway Amount: Maximum possible weapon sway
- Sway Smoothness: Smoothness of sway animation
- Rotation Amount: Intensity of rotational sway
- Max Rotation Amount: Maximum rotational sway
- Rotation Smoothness: Smoothness of rotation animation

## 🤝 Contributing
Contributions are welcome! Feel free to fork the project and submit pull requests.

## 📞 Contact
If you have any questions or suggestions, please open an issue or join my [discord](https://discord.com/invite/fe2RfUPkBu).

⭐ Don't forget to star this repository if you find it useful! ⭐