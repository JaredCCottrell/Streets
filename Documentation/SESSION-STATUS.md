# Streets - Development Session Status

**Last Updated:** December 22, 2024

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
- [x] **Road textures (Asphalt_material_14 for road surface)**
- [x] **Road line material (RoadLineWhite.mat - URP white)**
- [x] **Volumetric Fog system (Kronnect Volumetric Fog & Mist 2)**
- [x] **Fog setup tools (VolumetricFog2Setup.cs editor window)**
- [x] **Horror fog profile (HorrorFogProfile.asset)**
- [x] **Skybox setup tools (SkyboxSetup.cs editor window)**
- [x] **AllSky Free integration (Cold Night skybox)**
- [x] **UI scaling fixes (Scale With Screen Size for all canvases)**
- [x] **Inventory panel resolution-independent positioning**
- [x] **Details panel layout fix (now sibling of inventory panel)**
- [x] **Custom stamina bar with lung sprites**
- [x] **Custom sanity bar with brain sprites (alpha pulse when insane)**
- [x] **Debug sanity controls (G/H keys in editor)**
- [x] **Event system architecture (EventData, EventPool, EventManager)**
- [x] **Sanity-based event difficulty scaling**
- [x] **Event category system (Atmospheric, Creature, Obstacle, Apparition)**
- [x] **Overlapping events support (Atmospheric + other categories)**
- [x] **Chance-based event triggering with sanity influence**
- [x] **Dialogue system (DialogueData, DialogueManager, DialogueUI)**
- [x] **Dialogue choices with sanity consequences**
- [x] **Time pause during dialogue**
- [x] **DialogueEntity for NPC interactions**
- [x] **ShadowFigure NPC (black body, glowing white eyes)**
- [x] **Editor tools (DialogueUIBuilder, ShadowFigureBuilder)**
- [x] **NPCSpawnPoint for random NPC spawning**
- [x] **Test dialogue with branching choices**

### Next Up
- [ ] Design main quest structure
- [ ] Create opening scene (grandfather's house)
- [ ] Create static intro road section
- [ ] Implement transition to endless road
- [ ] Create main quest NPCs and dialogues
- [ ] Create custom sprites for health bar (heart sprites)
- [ ] Create custom sprites for hunger bar (stomach sprites)
- [ ] Create custom sprites for thirst bar (water drop sprites)
- [ ] Create prop prefabs (guardrails, signs, abandoned cars, streetlights)
- [ ] Add curve road segments (SlightLeft, SlightRight prefabs)

---

## Asset Store Packages Installed (Local Only - Not in Repo)

These need to be imported if setting up fresh:
- **AllSky Free** - Skybox pack (using Cold Night)
- **Volumetric Fog & Mist 2** - Kronnect fog system (paid)
- **Asphalt Materials** - Road textures (using Asphalt_material_14)
- **Yughues Free Pavement Materials** - Alternative textures
- **Yughues Free Ground Materials** - Ground textures
- **TextMesh Pro** - UI text

**Important:** After importing asset store packages, run:
`Edit > Rendering > Materials > Convert Built-in Materials to URP`

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
│   ├── Events/
│   │   ├── EventCategory.cs        # Enums for event categories & difficulty
│   │   ├── EventData.cs            # ScriptableObject for event definitions
│   │   ├── EventPool.cs            # ScriptableObject for event collections
│   │   └── EventManager.cs         # Spawns/manages events based on sanity
│   └── UI/
│       ├── StaminaUI.cs
│       ├── HealthUI.cs
│       ├── HungerUI.cs
│       ├── ThirstUI.cs
│       ├── SanityUI.cs
│       ├── InventoryUI.cs
│       ├── InventorySlotUI.cs
│       ├── HotbarUI.cs
│       ├── HotbarSlotUIComponent.cs
│       └── ContextMenuBuilder.cs
├── Editor/
│   ├── SkyboxSetup.cs              # Skybox selection tool
│   └── VolumetricFog2Setup.cs      # Fog setup tool
├── Materials/
│   └── RoadLineWhite.mat           # White URP material for road lines
├── Settings/
│   ├── RoadConfig.asset            # Road configuration
│   ├── HorrorFogProfile.asset      # Fog settings for horror atmosphere
│   ├── PC_Renderer.asset           # URP renderer (with SSAO)
│   └── PC_RPAsset.asset            # URP pipeline asset
├── Prefabs/
│   └── RoadSegment_Straight.prefab # Road segment with asphalt + white lines
└── Scenes/
    └── SampleScene.unity

Documentation/
├── devlog-001-project-inception.md
├── devlog-002-design-decisions.md
├── devlog-003-player-controller.md
├── devlog-004-health-system.md
├── devlog-005-survival-systems.md
├── devlog-006-inventory-system.md
├── devlog-010-road-textures-fog.md
└── session-status.md (this file)
```

---

## Road Generation System

### How It Works
1. **RoadGenerator** maintains a pool of road segments
2. Segments spawn ahead of player, despawn behind
3. Each segment has entry/exit connection points for seamless joining
4. **RoadPropSpawner** listens for new segments and places props

### Current Road Prefab
`RoadSegment_Straight.prefab`:
- **RoadSurface** - Asphalt_material_14 (realistic highway texture)
- **CenterLine** - RoadLineWhite.mat (yellow center line)
- **LaneLine_R1/L1** - RoadLineWhite.mat (white lane markers)
- **EdgeLine_L/R** - RoadLineWhite.mat (white edge lines)
- Entry/Exit points for segment chaining
- Prop spawn points (4 per segment)
- Event spawn point (1 per segment)
- Item spawn points (2 per segment)

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

## Fog System

### Setup
Use menu: `Streets > Quick Setup Horror Fog` or `Streets > Setup Volumetric Fog 2`

### HorrorFogProfile Settings
- White fog color (albedo: 1,1,1,1)
- Density: 0.6
- Height: 15m (low-lying)
- Vertical offset: -2m
- Max distance: 300m
- Covers 600m of road

---

## Skybox System

### Setup
Use menu: `Streets > Quick Apply Cold Night Skybox` or `Streets > Setup Skybox`

Currently using AllSky Free "Cold Night" skybox.

---

## Event System

### How It Works
1. **RoadSegment** has a `canTriggerEvent` flag (default true)
2. When player enters a segment, **EventManager** rolls for trigger chance
3. Trigger chance increases as sanity drops (40% base + 50% of missing sanity)
4. If triggered, **EventPool** selects events weighted by sanity:
   - High sanity = Harmless/Unsettling events
   - Low sanity = Terrifying/Nightmare events
5. Events can overlap by category (Atmospheric + Creature, etc.)

### Event Categories
| Category | Description | Overlaps With |
|----------|-------------|---------------|
| Atmospheric | Fog, sounds, flickering lights | Everything |
| Creature | Hostile entities, stalkers | Atmospheric |
| Obstacle | Roadblocks, car pile-ups | Atmospheric |
| Apparition | Figures, shadows, hallucinations | Atmospheric, Creature |

### Difficulty Scaling (by Sanity)
| Sanity | Max Event Difficulty |
|--------|---------------------|
| 100-75% | Unsettling |
| 75-50% | Dangerous |
| 50-25% | Terrifying |
| 25-0% | Nightmare |

### Setup
1. Create `EventData` assets: `Assets > Create > Streets > Events > Event Data`
2. Create `EventPool` asset: `Assets > Create > Streets > Events > Event Pool`
3. Add events to the pool, set base trigger chance
4. Add `EventManager` to scene, assign RoadGenerator + SanitySystem + EventPool

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

### Fog System
- [ ] Import Volumetric Fog & Mist 2 (including URP package inside bundle)
- [ ] Run `Streets > Quick Setup Horror Fog`
- [ ] VolumetricFogManager in scene
- [ ] VolumetricFog volume following player

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
- [ ] RoadGenerator → Player assigned, Segment Prefabs array populated
- [ ] RoadPropSpawner → RoadGenerator assigned

### Ground
- [ ] "Ground" layer created
- [ ] Road segments set to Ground layer (Layer 6)

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

```bash
# Check git status
git status

# See recent commits
git log --oneline -10

# Pull latest changes
git pull
```

---

## Known Issues / Notes

- **Curve segments disabled**: RoadGenerator weights set to 100% straight (curveWeight=0, specialWeight=0) until curve prefabs are created
- **Asset store materials**: May show pink until converted to URP via Edit > Rendering > Materials > Convert Built-in Materials to URP
- **Fog follows scene origin**: May need to parent fog volume to player for it to follow

---

*Repository: https://github.com/JaredCCottrell/Streets*
