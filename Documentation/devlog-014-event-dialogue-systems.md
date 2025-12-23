# Devlog 014: Event & Dialogue Systems

**Date:** December 22, 2024

---

## Overview

This session focused on building two major systems: a sanity-based event system and a full dialogue system with interactive NPCs.

---

## Event System

### Architecture

The event system triggers random encounters as the player travels along the road. Events are weighted by the player's sanity level - lower sanity means harder, more terrifying events.

**Core Components:**

| Script | Purpose |
|--------|---------|
| `EventCategory.cs` | Enums for event categories and difficulty tiers |
| `EventData.cs` | ScriptableObject defining individual events |
| `EventPool.cs` | Collection of events with spawn chance logic |
| `EventManager.cs` | Listens to road segments, spawns events |

### Event Categories

Events are categorized to allow overlapping (e.g., atmospheric + creature):

- **Atmospheric** - Fog, sounds, flickering lights (overlaps with everything)
- **Creature** - Hostile entities, stalkers
- **Obstacle** - Roadblocks, car pile-ups
- **Apparition** - Figures, shadows, hallucinations

### Difficulty Tiers

| Tier | Description |
|------|-------------|
| Harmless | Pure atmosphere, no threat |
| Unsettling | Creepy but minor |
| Dangerous | Moderate threat |
| Terrifying | Serious threat |
| Nightmare | Extreme danger |

### Sanity-Based Scaling

The player's sanity determines:
1. **Trigger chance**: 40% base + (missing sanity × 50%)
2. **Max difficulty**: High sanity caps at Unsettling, low sanity unlocks Nightmare
3. **Event weighting**: Lower sanity weights toward harder events

---

## Dialogue System

### Architecture

A full dialogue system supporting branching conversations with consequences.

**Core Components:**

| Script | Purpose |
|--------|---------|
| `DialogueData.cs` | ScriptableObject for dialogue sequences |
| `DialogueLine` | Individual line with optional choices |
| `DialogueChoice` | Player response with sanity consequences |
| `DialogueManager.cs` | Singleton managing dialogue flow, pauses time |
| `DialogueUI.cs` | UI for subtitles and choice buttons |
| `DialogueEntity.cs` | Component for interactable NPCs |

### Features

- **Time pause**: Game pauses during dialogue
- **Branching choices**: Up to 4 choices per line
- **Consequences**: Each choice can modify sanity (+/-)
- **Jump/End**: Choices can skip to specific lines or end dialogue
- **Response text**: NPC can respond to player's choice
- **Item drops**: Dialogue completion can spawn items
- **Typewriter effect**: Text appears character by character

### Dialogue Flow

1. Player approaches NPC (DialogueEntity)
2. Press E to interact
3. DialogueManager starts dialogue, pauses time
4. Lines display with optional choices
5. Player clicks/presses Space to advance, or clicks choice buttons
6. Sanity modified based on choices
7. Dialogue ends, time resumes

---

## Shadow Figure NPC

A placeholder horror NPC with:
- Solid black capsule body
- Glowing white eyes (pulsing emission)
- Hovering animation
- Faces player during dialogue
- Destroys itself after conversation

### Editor Tools

- `Streets > Create Shadow Figure NPC` - Creates configured NPC
- `Streets > Create Test Dialogue Data` - Creates sample dialogue
- `Streets > Build Dialogue UI` - Builds the dialogue canvas

---

## NPC Spawn Point

A utility component for random NPC spawning:

```csharp
[SerializeField] private GameObject[] npcPrefabs;
[SerializeField] private float spawnChance = 0.1f; // 10%
[SerializeField] private SpawnTrigger spawnTrigger;
```

Add to any GameObject (road segments, props) to give it a chance to spawn NPCs.

---

## Road Segment Updates

- Added `canTriggerEvent` flag to RoadSegment
- Added `OnPlayerEntered` / `OnPlayerExited` events
- RoadGenerator now fires `OnPlayerEnteredSegment` when player crosses into new segment
- Segments reset event state when recycled

---

## Files Created

```
Assets/Scripts/Events/
├── EventCategory.cs
├── EventData.cs
├── EventPool.cs
├── EventManager.cs
└── TestEvent.cs

Assets/Scripts/Dialogue/
├── DialogueData.cs
├── DialogueManager.cs
├── DialogueUI.cs
├── DialogueEntity.cs
├── ShadowFigure.cs
└── BillboardPrompt.cs

Assets/Scripts/Spawning/
└── NPCSpawnPoint.cs

Assets/Scripts/Editor/
├── DialogueUIBuilder.cs
└── ShadowFigureBuilder.cs

Assets/Settings/Events/
└── (EventPool and EventData assets)

Assets/Settings/Dialogue/
└── Test_ShadowConversation.asset

Assets/Prefabs/
├── ShadowFigure_NPC.prefab
└── TestEvent_Sphere.prefab
```

---

## Next Steps

1. Design main quest narrative structure
2. Create opening scene (grandfather's house)
3. Build static intro road section
4. Implement transition from normal to endless road
5. Create main quest NPCs with story dialogues
