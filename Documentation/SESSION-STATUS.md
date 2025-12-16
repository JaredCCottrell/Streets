# Streets - Development Session Status

**Last Updated:** December 15, 2025

---

## Current Progress

### Completed
- [x] GitHub repository setup
- [x] First-person player controller (walk, sprint, mouse look)
- [x] Stamina system (25% threshold to resume sprinting)
- [x] Health system (damage, healing, invincibility frames)
- [x] Hunger system (depletes over time, starvation damage)
- [x] Thirst system (depletes over time, dehydration damage)
- [x] Sanity system (placeholder - event-driven, insanity damage)
- [x] UI components for all survival meters
- [x] Inventory system (slot-based, stacking, consumables/equipment/key items)
- [x] Hotbar system (4 quick-use slots, keys 1-4)
- [x] Inventory UI (Tab to toggle, context menu, item details)
- [x] Item pickup system (E key or auto-pickup, with bobbing animation)
- [x] Inventory drag-and-drop (drag items between slots and to hotbar)
- [x] Context menu builder (compact, auto-positioned menu)
- [x] Procedural road generation system
- [x] Modular road segment prefabs with connection points
- [x] Chunk loading/unloading for infinite road
- [x] Road prop spawning system (randomized placement)

### Next Up
- [ ] Create prop prefabs (guardrails, signs, abandoned cars, streetlights)
- [ ] Create event/encounter system (harmless + harmful)
- [ ] Implement checkpoint system for Normal mode
- [ ] Set up eerie atmosphere (lighting, fog, skybox)
- [ ] Add curve road segments for variety

---

## Project Structure

```
Assets/
├── Scripts/
│   ├── Player/
│   │   └── FirstPersonController.cs
│   ├── Survival/
│   │   ├── HealthSystem.cs
│   │   ├── HungerSystem.cs
│   │   ├── ThirstSystem.cs
│   │   ├── SanitySystem.cs
│   │   └── DamageZone.cs
│   ├── Inventory/
│   │   ├── Items/
│   │   │   ├── ItemData.cs
│   │   │   ├── ConsumableData.cs
│   │   │   ├── EquipmentData.cs
│   │   │   └── KeyItemData.cs
│   │   ├── InventorySlot.cs
│   │   ├── InventorySystem.cs
│   │   ├── HotbarSystem.cs
│   │   └── ItemPickup.cs
│   ├── Road/
│   │   ├── RoadSegment.cs          # Component for road prefabs
│   │   ├── RoadGenerator.cs        # Spawns/despawns road chunks
│   │   ├── RoadConfig.cs           # ScriptableObject for road settings
│   │   ├── RoadSegmentBuilder.cs   # Editor tool to build road geometry
│   │   ├── RoadPropData.cs         # ScriptableObject for prop definitions
│   │   ├── RoadPropPool.cs         # ScriptableObject for prop collections
│   │   └── RoadPropSpawner.cs      # Spawns props on road segments
│   ├── UI/
│   │   ├── StaminaUI.cs
│   │   ├── HealthUI.cs
│   │   ├── HungerUI.cs
│   │   ├── ThirstUI.cs
│   │   ├── SanityUI.cs
│   │   ├── InventoryUI.cs          # Updated with drag-and-drop
│   │   ├── InventorySlotUI.cs      # Updated with drag interfaces
│   │   ├── HotbarUI.cs
│   │   ├── HotbarSlotUIComponent.cs # New - MonoBehaviour for hotbar drops
│   │   └── ContextMenuBuilder.cs   # New - Builds compact context menu
│   └── Input/
│       └── InputSystem_Actions.cs (generated)
├── Settings/
│   └── RoadConfig.asset           # Road configuration
└── Scenes/
    └── SampleScene.unity

Documentation/
├── devlog-001-project-inception.md
├── devlog-002-design-decisions.md
├── devlog-003-player-controller.md
├── devlog-004-health-system.md
├── devlog-005-survival-systems.md
├── devlog-006-inventory-system.md
└── SESSION-STATUS.md (this file)
```

---

## Road Generation System

### How It Works
1. **RoadGenerator** maintains a pool of road segments
2. Segments spawn ahead of player, despawn behind
3. Each segment has entry/exit connection points for seamless joining
4. **RoadPropSpawner** listens for new segments and places props

### Road Segment Setup
1. Create empty GameObject
2. Add `RoadSegment` + `RoadSegmentBuilder` components
3. Assign a `RoadConfig` asset
4. Right-click RoadSegmentBuilder → "Build Road Segment"
5. Save as prefab

### Prop System Setup
1. Create `RoadPropData` assets for each prop type
2. Create `RoadPropPool` asset with prop collection
3. Add `RoadPropSpawner` to scene
4. Assign RoadGenerator and Prop Pools

---

## Game Design Summary

**Genre:** First-person survival horror

**Setting:** Endless eerie interstate highway

**Goal:** Reach the end of the road

**Game Modes:**
- Normal (checkpoints)
- Hardcore (permadeath)
- Endless (future - distance survival)

**Survival Meters:**
| Meter | Behavior |
|-------|----------|
| Health | Damaged by threats and other meters at 0 |
| Stamina | Drains while sprinting, regens after delay |
| Hunger | Depletes over time (2x while sprinting) |
| Thirst | Depletes over time (2.5x while sprinting) |
| Sanity | Event-driven (creepy encounters) |

**Inventory:**
| Feature | Details |
|---------|---------|
| Slots | 12 (configurable) |
| Stacking | Consumables stack (max 5), equipment/keys don't |
| Hotbar | 4 slots, keys 1-4 for quick use |
| Item Types | Consumables, Equipment, Key Items |
| Drag & Drop | Drag items between inventory slots and to hotbar |

**Threats:** Mix of harmless atmospheric scares and dangerous encounters

---

## Unity Setup Checklist

If starting fresh or verifying setup:

### Player Object
- [ ] CharacterController component
- [ ] FirstPersonController script
- [ ] HealthSystem script
- [ ] HungerSystem script
- [ ] ThirstSystem script
- [ ] SanitySystem script
- [ ] InventorySystem script
- [ ] HotbarSystem script
- [ ] Child camera at eye height

### Road System
- [ ] RoadGenerator object with RoadGenerator component
- [ ] RoadPropSpawner object with RoadPropSpawner component
- [ ] Road segment prefabs assigned to RoadGenerator
- [ ] Prop pools assigned to RoadPropSpawner

### Component References
- [ ] FirstPersonController → Camera Transform assigned
- [ ] FirstPersonController → Ground Mask set to "Ground" layer
- [ ] FirstPersonController → HungerSystem & ThirstSystem assigned
- [ ] FirstPersonController → InventorySystem, HotbarSystem, InventoryUI assigned
- [ ] HungerSystem → HealthSystem assigned
- [ ] ThirstSystem → HealthSystem assigned
- [ ] SanitySystem → HealthSystem assigned
- [ ] InventorySystem → All survival systems assigned
- [ ] HotbarSystem → InventorySystem assigned
- [ ] RoadGenerator → Player assigned
- [ ] RoadPropSpawner → RoadGenerator assigned

### Ground
- [ ] "Ground" layer created
- [ ] Road segments set to Ground layer

### UI (Canvas)
- [ ] StaminaUI with fill bar
- [ ] HealthUI with fill bar
- [ ] HungerUI with fill bar
- [ ] ThirstUI with fill bar
- [ ] SanityUI with fill bar
- [ ] HotbarUI with 4 slot displays (HotbarSlotUIComponent)
- [ ] InventoryUI panel with ContextMenuBuilder

---

## Quick Resume Commands

```
# Check git status
git status

# See recent commits
git log --oneline -10

# Pull latest changes
git pull
```

---

*Repository: https://github.com/JaredCCottrell/Streets*
