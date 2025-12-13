# Development Log 004 - Health System

**Date:** December 12, 2025

## Overview

Implemented the health system as the first component of the survival meter system.

## Files Created

| File | Purpose |
|------|---------|
| `Assets/Scripts/Survival/HealthSystem.cs` | Core health management |
| `Assets/Scripts/UI/HealthUI.cs` | Health bar UI with damage effects |
| `Assets/Scripts/Survival/DamageZone.cs` | Test trigger for damaging player |

## HealthSystem.cs

### Features

- **Take Damage** - Reduces health with invincibility frames
- **Heal** - Restores health up to max
- **Invincibility** - Brief immunity after taking damage
- **Regeneration** - Optional auto-heal after delay (disabled by default)
- **Events** - For UI and other systems to respond

### Configuration (Inspector)

#### Health Settings
| Setting | Default | Description |
|---------|---------|-------------|
| Max Health | 100 | Total health pool |
| Current Health | (auto) | Starts at max |

#### Damage Settings
| Setting | Default | Description |
|---------|---------|-------------|
| Invincibility Duration | 0.5s | Immunity window after taking damage |

#### Regeneration Settings
| Setting | Default | Description |
|---------|---------|-------------|
| Can Regenerate | false | Enable auto-healing |
| Regen Rate | 5/sec | Health restored per second |
| Regen Delay | 5s | Wait time after damage before regen starts |

### Events

```csharp
event Action<float, float> OnHealthChanged  // (current, max)
event Action<float> OnDamageTaken           // (damage amount)
event Action<float> OnHealed                // (heal amount)
event Action OnDeath                        // player died
```

### Public API

```csharp
// Properties (read-only)
float CurrentHealth     // Current health value
float MaxHealth         // Maximum health
float HealthPercent     // Current health as 0-1 percentage
bool IsAlive            // Health > 0
bool IsFullHealth       // Health >= max

// Methods
void TakeDamage(float damage)                    // Deal damage
void Heal(float amount)                          // Restore health
void SetHealth(float amount)                     // Set to specific value
void SetMaxHealth(float newMax, bool healToFull) // Change max health
void FullHeal()                                  // Restore to max
void Revive(float healthPercent = 1f)            // Revive after death
```

## HealthUI.cs

### Features

- Health bar with color gradient based on health percentage
- Damage fill bar that drains behind main health bar
- Screen flash overlay when taking damage

### Configuration (Inspector)

#### References
| Setting | Description |
|---------|-------------|
| Health System | Reference to player's HealthSystem |
| Health Fill | Main health bar Image (Filled type) |
| Damage Fill | Secondary bar that trails behind (optional) |
| Damage Flash Overlay | Fullscreen Image for damage flash (optional) |

#### Color Settings
| Setting | Default | Description |
|---------|---------|-------------|
| Healthy Color | Green | Color at high health |
| Damaged Color | Yellow | Color at medium health |
| Critical Color | Red | Color at low health |
| Damaged Threshold | 0.5 (50%) | Below this shows damaged color |
| Critical Threshold | 0.25 (25%) | Below this shows critical color |

#### Damage Flash
| Setting | Default | Description |
|---------|---------|-------------|
| Flash Duration | 0.2s | How long the red flash lasts |
| Flash Color | Red (30% alpha) | Flash overlay color |

### Color Behavior

- **100% - 50%**: Green → Yellow gradient
- **50% - 25%**: Yellow → Red gradient
- **Below 25%**: Solid red (critical)

## DamageZone.cs

Test component for debugging health system.

### Configuration
| Setting | Default | Description |
|---------|---------|-------------|
| Damage Amount | 10 | Damage dealt per tick |
| Damage Interval | 1s | Time between damage ticks |
| Instant Kill | false | Kill immediately on contact |

### Usage
1. Create empty GameObject
2. Add Collider component, enable "Is Trigger"
3. Add DamageZone script
4. Player takes damage when entering/staying in trigger

## Unity Setup Instructions

### Adding Health to Player

1. Select Player object
2. Add `HealthSystem` component
3. Configure settings as desired

### Health UI Setup

Uses same Canvas as Stamina UI:

```
Canvas
├── StaminaBackground
│   └── StaminaFill
└── HealthBackground (Image + HealthUI)
    ├── DamageFill (Image - Filled, red/dark, render behind)
    └── HealthFill (Image - Filled, green)
```

**Steps:**
1. Create Image under Canvas → rename "HealthBackground"
2. Add `HealthUI` script to HealthBackground
3. Create child Image → rename "HealthFill"
   - Set Image Type: Filled
   - Set Fill Method: Horizontal
   - Set color to green
4. (Optional) Create another child Image → rename "DamageFill"
   - Same Filled settings
   - Set color to dark red
   - Move above HealthFill in hierarchy (renders behind)
5. (Optional) Create fullscreen Image for damage flash
   - Anchor to stretch full screen
   - Set color to transparent
6. Assign all references in HealthUI component

### Optional: Damage Flash Overlay

For the red screen flash when taking damage:

1. Create Image under Canvas
2. Set anchors to stretch (hold Alt, click bottom-right anchor preset)
3. Set all edge values to 0
4. Set color to transparent (alpha = 0)
5. Assign to "Damage Flash Overlay" in HealthUI

---

*Repository: https://github.com/JaredCCottrell/Streets*
