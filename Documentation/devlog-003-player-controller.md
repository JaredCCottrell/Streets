# Development Log 003 - First-Person Player Controller

**Date:** December 12, 2025

## Overview

Implemented the first-person player controller with movement, mouse look, and stamina system.

## Files Created

| File | Purpose |
|------|---------|
| `Assets/Scripts/Player/FirstPersonController.cs` | Main player controller |
| `Assets/Scripts/UI/StaminaUI.cs` | Stamina bar UI component |

## FirstPersonController.cs

### Features

- **First-person movement** using CharacterController
- **Walk/Sprint** with stamina management
- **Mouse look** with vertical angle clamping
- **Gravity** and ground detection
- **Unity New Input System** integration

### Configuration (Inspector)

#### Movement Settings
| Setting | Default | Description |
|---------|---------|-------------|
| Walk Speed | 3 | Movement speed while walking |
| Sprint Speed | 6 | Movement speed while sprinting |
| Gravity | -9.81 | Gravity force |
| Ground Check Distance | 0.4 | Radius for ground detection sphere |
| Ground Mask | (none) | Layer mask for ground detection |

#### Look Settings
| Setting | Default | Description |
|---------|---------|-------------|
| Mouse Sensitivity | 100 | Look sensitivity |
| Max Look Angle | 90 | Vertical look clamp (degrees) |
| Camera Transform | (none) | Reference to player camera |

#### Stamina Settings
| Setting | Default | Description |
|---------|---------|-------------|
| Max Stamina | 100 | Total stamina pool |
| Stamina Drain Rate | 20/sec | Drain rate while sprinting |
| Stamina Regen Rate | 10/sec | Regeneration rate |
| Stamina Regen Delay | 1 sec | Delay before regen starts |
| Min Stamina Percent To Sprint | 0.25 (25%) | Must regenerate to this % after depletion before sprinting again |

### Stamina Behavior

1. **Sprinting** (Shift + Move) drains stamina at 20/sec
2. **Stopping sprint** starts a 1-second delay timer
3. **After delay**, stamina regenerates at 10/sec
4. **If depleted to 0**, player cannot sprint until stamina reaches 25%
5. This prevents "stutter sprinting" exploits

### Public API

```csharp
// Properties (read-only)
float CurrentStamina    // Current stamina value
float MaxStamina        // Maximum stamina
float StaminaPercent    // Current stamina as 0-1 percentage
bool IsSprinting        // Is player currently sprinting
bool IsGrounded         // Is player on ground
bool IsMoving           // Is player inputting movement

// Methods
void SetStamina(float amount)      // Set stamina to specific value
void ModifyStamina(float delta)    // Add/subtract stamina
```

## StaminaUI.cs

Simple UI component that displays stamina as a fill bar.

### Features
- Displays stamina as horizontal fill bar
- Auto-fades out when stamina is full
- Fades in when stamina changes or is below max

### Setup Instructions

1. Create Canvas (GameObject > UI > Canvas)
2. Create Image for background (dark color)
3. Create child Image for fill (bright color)
   - Set Image Type: Filled
   - Set Fill Method: Horizontal
4. Add CanvasGroup to background Image
5. Add StaminaUI script to background
6. Assign references:
   - Player Controller → Player object
   - Stamina Fill → Fill Image
   - Canvas Group → CanvasGroup component

## Unity Setup Instructions

### Player Setup

1. Create empty GameObject named "Player"
2. Add CharacterController component
3. Add FirstPersonController script
4. Create child GameObject for camera
   - Add Camera component
   - Position at eye height (Y ~1.6)
5. Assign camera to `cameraTransform` field
6. Create "Ground" layer (Edit > Project Settings > Tags and Layers)
7. Assign Ground layer to floor objects
8. Set `groundMask` to include Ground layer

### Input System Setup

The project uses Unity's New Input System. The input actions asset needs to generate a C# wrapper:

1. Select `Assets/InputSystem_Actions.inputactions`
2. In Inspector, enable "Generate C# Class"
3. Set path to `Assets/Scripts/Input/InputSystem_Actions.cs`
4. Set class name to `InputSystem_Actions`
5. Set namespace to `Streets.Input`
6. Click Apply

### Controls

| Action | Keyboard | Gamepad |
|--------|----------|---------|
| Move | WASD / Arrows | Left Stick |
| Look | Mouse | Right Stick |
| Sprint | Left Shift | Left Stick Press |
| Jump | Space | South Button (A/X) |
| Interact | E (hold) | North Button (Y/Triangle) |

---

*Repository: https://github.com/JaredCCottrell/Streets*
