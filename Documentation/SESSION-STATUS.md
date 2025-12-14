# Streets - Development Session Status

**Last Updated:** December 14, 2025

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

### Next Up
- [ ] Create procedural road generation system
- [ ] Build modular road segment prefabs
- [ ] Implement chunk loading/unloading for infinite road
- [ ] Create event/encounter system (harmless + harmful)
- [ ] Implement checkpoint system for Normal mode
- [ ] Set up eerie atmosphere (lighting, fog, skybox)

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
│   ├── UI/
│   │   ├── StaminaUI.cs
│   │   ├── HealthUI.cs
│   │   ├── HungerUI.cs
│   │   ├── ThirstUI.cs
│   │   ├── SanityUI.cs
│   │   ├── InventoryUI.cs
│   │   ├── InventorySlotUI.cs
│   │   └── HotbarUI.cs
│   └── Input/
│       └── InputSystem_Actions.cs (generated)
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

### Ground
- [ ] "Ground" layer created
- [ ] Floor objects set to Ground layer

### UI (Canvas)
- [ ] StaminaUI with fill bar
- [ ] HealthUI with fill bar
- [ ] HungerUI with fill bar
- [ ] ThirstUI with fill bar
- [ ] SanityUI with fill bar
- [ ] HotbarUI with 4 slot displays
- [ ] InventoryUI panel (starts disabled)

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
