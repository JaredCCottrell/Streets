# Development Log 005 - Survival Systems

**Date:** December 12, 2025

## Overview

Implemented the complete survival system: hunger, thirst, and sanity. Combined with the previously created health system, the player now has four survival meters to manage.

## Files Created

| File | Purpose |
|------|---------|
| `Assets/Scripts/Survival/HungerSystem.cs` | Hunger management with starvation |
| `Assets/Scripts/Survival/ThirstSystem.cs` | Thirst management with dehydration |
| `Assets/Scripts/Survival/SanitySystem.cs` | Sanity management (placeholder for events) |
| `Assets/Scripts/UI/HungerUI.cs` | Hunger bar UI |
| `Assets/Scripts/UI/ThirstUI.cs` | Thirst bar UI |
| `Assets/Scripts/UI/SanityUI.cs` | Sanity bar UI |

## Files Modified

| File | Changes |
|------|---------|
| `FirstPersonController.cs` | Added survival system references, notifies hunger/thirst of sprint state |

---

## Survival Systems Summary

| System | Depletes | Critical State | Damage |
|--------|----------|----------------|--------|
| Health | From damage | Death at 0 | N/A |
| Hunger | Over time (1/sec) | Starvation at 0 | 5 dmg / 3 sec |
| Thirst | Over time (1.5/sec) | Dehydration at 0 | 7 dmg / 2.5 sec |
| Sanity | From events (TBD) | Insanity at 0 | 3 dmg / 4 sec |

---

## HungerSystem.cs

### Features
- Depletes constantly over time
- Depletes faster while sprinting (2x)
- Starvation deals damage when empty

### Configuration (Inspector)

| Setting | Default | Description |
|---------|---------|-------------|
| Max Hunger | 100 | Total hunger pool |
| Hunger Depletion Rate | 1/sec | Base drain rate |
| Deplete While Idle | true | Drain even when standing |
| Sprint Depletion Multiplier | 2x | Extra drain while sprinting |
| Starvation Threshold | 0 | Below this, starvation begins |
| Starvation Damage | 5 | Damage dealt per tick |
| Starvation Damage Interval | 3s | Time between damage |

### Events
```csharp
event Action<float, float> OnHungerChanged    // (current, max)
event Action OnStarvationStarted
event Action OnStarvationEnded
event Action<float> OnFoodConsumed            // (amount restored)
```

### Public API
```csharp
void Eat(float amount)         // Restore hunger
void SetHunger(float amount)   // Set to specific value
void FillHunger()              // Restore to max
void SetSprinting(bool)        // Called by FirstPersonController
```

---

## ThirstSystem.cs

### Features
- Depletes constantly over time (faster than hunger)
- Depletes faster while sprinting (2.5x)
- Dehydration deals more damage than starvation

### Configuration (Inspector)

| Setting | Default | Description |
|---------|---------|-------------|
| Max Thirst | 100 | Total thirst pool |
| Thirst Depletion Rate | 1.5/sec | Base drain rate |
| Deplete While Idle | true | Drain even when standing |
| Sprint Depletion Multiplier | 2.5x | Extra drain while sprinting |
| Dehydration Threshold | 0 | Below this, dehydration begins |
| Dehydration Damage | 7 | Damage dealt per tick |
| Dehydration Damage Interval | 2.5s | Time between damage |

### Events
```csharp
event Action<float, float> OnThirstChanged    // (current, max)
event Action OnDehydrationStarted
event Action OnDehydrationEnded
event Action<float> OnDrinkConsumed           // (amount restored)
```

### Public API
```csharp
void Drink(float amount)       // Restore thirst
void SetThirst(float amount)   // Set to specific value
void FillThirst()              // Restore to max
void SetSprinting(bool)        // Called by FirstPersonController
```

---

## SanitySystem.cs

### Features
- Does NOT deplete over time (event-driven only)
- Placeholder for future creepy events
- Insanity deals damage when empty

### Configuration (Inspector)

| Setting | Default | Description |
|---------|---------|-------------|
| Max Sanity | 100 | Total sanity pool |
| Insanity Threshold | 0 | Below this, insanity begins |
| Insanity Damage | 3 | Damage dealt per tick |
| Insanity Damage Interval | 4s | Time between damage |

### Events
```csharp
event Action<float, float> OnSanityChanged    // (current, max)
event Action OnInsanityStarted
event Action OnInsanityEnded
event Action<float> OnSanityRestored          // (amount restored)
event Action<float> OnSanityLost              // (amount lost)
```

### Public API
```csharp
void LoseSanity(float amount)      // Called by creepy events
void RestoreSanity(float amount)   // Rest, items, safe zones
void SetSanity(float amount)       // Set to specific value
void FillSanity()                  // Restore to max
```

### Future Integration
When the event system is created, events will call:
```csharp
// Example: player witnesses something disturbing
sanitySystem.LoseSanity(15f);

// Example: player rests at a safe location
sanitySystem.RestoreSanity(10f);
```

---

## UI Components

All UI components follow the same pattern:
- Reference to their respective system
- Fill Image with gradient colors
- Pulse effect when in critical state

### Color Schemes

| UI | Full Color | Empty Color | Critical Color |
|----|------------|-------------|----------------|
| Health | Green | Yellow → Red | Red (pulsing) |
| Hunger | Orange | Dark Brown | Red (pulsing) |
| Thirst | Light Blue | Dark Blue | Red (pulsing) |
| Sanity | Purple | Dark Purple | Red (pulsing) |

---

## Unity Setup Instructions

### Player Setup

Add all systems to the Player object:

```
Player (GameObject)
├── CharacterController
├── FirstPersonController
├── HealthSystem
├── HungerSystem
├── ThirstSystem
└── SanitySystem
```

### Component References

**FirstPersonController:**
- Hunger System → Player's HungerSystem
- Thirst System → Player's ThirstSystem

**HungerSystem:**
- Health System → Player's HealthSystem

**ThirstSystem:**
- Health System → Player's HealthSystem

**SanitySystem:**
- Health System → Player's HealthSystem

### UI Setup

All survival bars under the same Canvas:

```
Canvas
├── StaminaBackground
│   └── StaminaFill
├── HealthBackground
│   ├── DamageFill (optional)
│   └── HealthFill
├── HungerBackground (Image + HungerUI)
│   └── HungerFill
├── ThirstBackground (Image + ThirstUI)
│   └── ThirstFill
└── SanityBackground (Image + SanityUI)
    └── SanityFill
```

### UI Component Assignment

Each UI script needs:
1. Reference to system on Player
2. Reference to fill Image

Example for HungerUI:
- Hunger System → Player's HungerSystem component
- Hunger Fill → HungerFill Image

---

## Design Notes

### Survival Pressure Balance

- **Thirst** is most urgent (depletes 1.5x faster than hunger, deals more damage)
- **Hunger** is steady pressure (constant drain, moderate damage)
- **Sanity** is event-driven (doesn't drain passively, controlled by encounters)
- **Sprinting** accelerates hunger (2x) and thirst (2.5x), creating risk/reward decisions

### Gameplay Implications

1. Players must balance sprinting (escape threats) vs. conserving resources
2. Roadside locations become critical for finding food/water
3. Sanity creates tension from creepy events without passive drain
4. All systems eventually damage health, creating unified death condition

---

*Repository: https://github.com/JaredCCottrell/Streets*
