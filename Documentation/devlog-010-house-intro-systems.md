# Devlog 010: House Intro & Item Systems

## Overview
Major update implementing the house intro sequence, item interaction overhaul, intoxication system, and NPC interaction for the alien-themed game.

## Systems Removed
- **Hunger System** - Removed from all scripts, UI components remain but unused
- **Thirst System** - Removed from all scripts, UI components remain but unused
- **Sanity System** - Removed from all scripts, UI components remain but unused

## New Systems

### Intoxication System (`Assets/Scripts/Effects/`)
Beer and alcohol now cause intoxication effects instead of restoring sanity.

**IntoxicationSystem.cs**
- Tracks intoxication level (0-100)
- Stacking: Multiple drinks compound the effect
- Decay: Level decreases at 5/second after 3 seconds without drinking
- Thresholds: Sober (0-19), Mild (20-49), Moderate (50-79), Severe (80-100)

**IntoxicationEffect.cs**
- Visual post-processing effects that scale with intoxication:
  - Chromatic aberration (color fringing)
  - Lens distortion with wobble
  - Vignette (darkened edges)
  - Color saturation boost
  - Camera wobble/sway
  - Motion blur

### Held Item System (`Assets/Scripts/Inventory/HeldItemSystem.cs`)
Complete overhaul of item pickup mechanics.

**Controls:**
- **E** on world item = Pick up and hold in front of player
- **E** while holding = Use item (if consumable)
- **R** while holding = Store in inventory
- **G** while holding = Drop item

**Features:**
- Items have physics (gravity, no floating)
- Held items follow camera smoothly
- On-screen prompts for available actions
- Raycast detection for items in view

### Item Pickup Changes (`Assets/Scripts/Inventory/ItemPickup.cs`)
- Removed floating/bobbing animation
- Physics-based with gravity
- Supports being held by HeldItemSystem
- RequireComponent for Rigidbody and Collider

### House Interaction Systems (`Assets/Scripts/House/`)

**Core Scripts:**
- `Interactable.cs` - Base class for all interactable objects
- `Door.cs` - Doors with lock/unlock, key requirements
- `CodeLock.cs` - Combination locks with keypad UI
- `Examinable.cs` - Notes, photos, clues
- `Moveable.cs` - Pushable furniture
- `PuzzleSwitch.cs` - Levers and buttons
- `PuzzleController.cs` - Multi-switch puzzle logic
- `SceneExitTrigger.cs` - Transition to road scene
- `HouseProgressTracker.cs` - Track puzzle completion

**UI Components:**
- `CodeLockUI.cs` - Keypad interface
- `ExamineUI.cs` - Document viewing panel

### Grandpa NPC (`Assets/Scripts/House/NPCs/GrandpaNPC.cs`)
First objective NPC who asks for beer.

**Features:**
- Three dialogue states: wants beer, receiving beer, satisfied
- Detects when player holds beer
- Takes beer from player's hand
- Can give reward item after receiving beer
- Faces player during interaction

**Dialogue Assets (via editor tool):**
- `Grandpa_WantsBeer` - Initial request dialogue
- `Grandpa_ReceivingBeer` - Thanks player for beer
- `Grandpa_AlreadyHasBeer` - Post-beer dialogue

### Scene Management (`Assets/Scripts/Core/`)

**GameStateManager.cs**
- Persists inventory across scenes
- Saves/restores health and intoxication
- DontDestroyOnLoad singleton

**SceneTransitionManager.cs**
- Fade transitions between scenes
- Handles scene loading

## Editor Tools (`Assets/Scripts/Editor/HouseItemCreator.cs`)

**Menu Items:**
- `Streets > Create House Items` - Creates beer and key items
- `Streets > Create Grandpa NPC Assets` - Creates grandpa dialogue
- `Streets > Create House Scene` - Creates scene with hierarchy
- `Streets > Validate House Setup` - Checks setup completeness

**Items Created:**
- Beer (consumable, 25 intoxication)
- Key_GarageFridge (key_fridge)
- Key_BedroomDoor (key_bedroom)
- Key_BasementDoor (key_basement)
- Key_FrontDoor (key_front)

## ConsumableData Changes
Simplified fields:
- `healthRestore` - Health restored
- `healthDamage` - Health damage
- `intoxicationAmount` - Intoxication added (0-50 range)

Removed:
- hungerRestore, hungerDrain
- thirstRestore, thirstDrain
- sanityRestore, sanityDrain

## DialogueManager Updates
- Added callback overload: `StartDialogue(dialogue, onComplete)`
- Removed sanity system integration
- Cleaner completion handling

## Game Flow (House Intro)

```
1. Player starts in bedroom
2. Talk to Grandpa → "Get me a beer"
3. Find fridge key (bedroom nightstand)
4. Unlock garage fridge
5. Pick up beer (E to hold)
6. Bring to Grandpa (E to give)
7. Grandpa drinks, gives hint/key
8. Solve remaining puzzles
9. Exit through front door
10. Walk to end of cul-de-sac
11. Transition to infinite road
```

## Setup Checklist

### Player Setup
- [ ] Add `HeldItemSystem` component
- [ ] Add `IntoxicationSystem` component
- [ ] Add `IntoxicationEffect` component
- [ ] Assign camera references

### House Scene Setup
- [ ] Run `Streets > Create House Items`
- [ ] Run `Streets > Create Grandpa NPC Assets`
- [ ] Create Grandpa GameObject with `GrandpaNPC`
- [ ] Place fridge with `Door` component (requiredKeyId: "key_fridge")
- [ ] Place beer pickup inside fridge
- [ ] Place fridge key pickup in bedroom

### Item Pickups
- [ ] Ensure Rigidbody component (useGravity: true)
- [ ] Ensure Collider component
- [ ] Assign ItemData reference
