# Devlog 006 - Inventory System

**Date:** December 13, 2025

---

## Overview

Implemented a complete inventory system with slot-based storage, item stacking, hotbar quick-access, and full UI support. The system integrates with existing survival systems to handle consumable effects.

---

## Design Decisions

### Slot-Based Inventory
- Chose fixed slot count (12 default) over weight-only system
- Provides clear visual representation and familiar UX
- Weight field included for potential future hybrid system

### Item Types via ScriptableObjects
Three item categories, each extending base `ItemData`:

1. **ConsumableData** - Food, water, medicine
   - Restores survival meters (health, hunger, thirst, sanity)
   - Can also have negative effects (poison, spoiled food)
   - Stackable (default max 5)

2. **EquipmentData** - Flashlight, weapons, tools
   - Has equipment slot (Hand, Head, Body)
   - Links to equip prefab for visual/functional representation
   - Non-stackable

3. **KeyItemData** - Quest/story items
   - Has unique `keyId` for trigger checks
   - Cannot be dropped by default
   - Non-stackable

### Hotbar System
- 4 quick-use slots mapped to number keys 1-4
- References inventory slots by index
- Auto-updates when inventory changes

---

## Architecture

### Core Components

```
InventorySystem (MonoBehaviour)
├── Manages InventorySlot[] array
├── Handles add/remove/use operations
├── Applies consumable effects to survival systems
└── Fires events for UI updates

HotbarSystem (MonoBehaviour)
├── Maintains hotbar-to-inventory slot mappings
├── Handles quick-use input
└── Syncs with inventory changes

InventorySlot (Serializable class)
├── Holds ItemData reference + quantity
├── Provides stack management methods
└── Tracks empty/full state
```

### Event Pattern
Follows existing survival system conventions:
```csharp
public event Action<int, InventorySlot> OnSlotChanged;
public event Action OnInventoryFull;
public event Action<ItemData> OnItemAdded;
public event Action<ItemData> OnItemRemoved;
public event Action<ItemData> OnItemUsed;
```

### UI Components

**HotbarUI** - Always visible, shows 4 quick slots
- Displays item icons and stack quantities
- Highlights selected slot
- Updates on inventory changes

**InventoryUI** - Toggle with Tab key
- Grid of draggable slots
- Item details panel on hover
- Context menu (Use, Drop, Assign to Hotbar)
- Pauses player movement when open

**InventorySlotUI** - Individual slot display
- Handles click/hover events
- Visual feedback for selection/hover states

---

## Integration Points

### FirstPersonController
Added inventory input handling:
```csharp
// Tab toggles inventory
// 1-4 use hotbar slots
// Movement disabled when inventory open
```

### Survival Systems
InventorySystem references all survival systems:
```csharp
[SerializeField] private HealthSystem healthSystem;
[SerializeField] private HungerSystem hungerSystem;
[SerializeField] private ThirstSystem thirstSystem;
[SerializeField] private SanitySystem sanitySystem;
```

When consuming items, effects are applied directly:
```csharp
if (consumable.hungerRestore > 0)
    hungerSystem.Eat(consumable.hungerRestore);
```

---

## File Structure

```
Assets/Scripts/Inventory/
├── Items/
│   ├── ItemData.cs           # Base ScriptableObject
│   ├── ConsumableData.cs     # Food/water/medicine
│   ├── EquipmentData.cs      # Tools/weapons
│   └── KeyItemData.cs        # Quest items
├── InventorySlot.cs          # Runtime slot data
├── InventorySystem.cs        # Main manager
└── HotbarSystem.cs           # Quick-use slots

Assets/Scripts/UI/
├── HotbarUI.cs               # Hotbar display
├── InventoryUI.cs            # Full inventory panel
└── InventorySlotUI.cs        # Slot component
```

---

## Controls

| Input | Action |
|-------|--------|
| Tab | Toggle inventory panel |
| 1-4 | Use hotbar slot |
| Left-click slot | Select/swap items |
| Right-click slot | Open context menu |

---

## Unity Setup Required

1. Add `InventorySystem` and `HotbarSystem` to Player
2. Create Canvas with Hotbar and InventoryPanel
3. Set up slot prefabs with UI elements
4. Wire references in Inspector
5. Create item ScriptableObjects (Create → Streets → Items)

---

## Future Considerations

- Item pickup system for world objects
- Equipment visual representation (flashlight beam, held items)
- Inventory persistence (save/load)
- Item crafting/combining
- Loot tables for procedural item spawning
