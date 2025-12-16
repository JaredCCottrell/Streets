# Devlog 007 - Road Generation System

**Date:** December 15, 2025

---

## Overview

Implemented the core infinite highway system - the foundation of Streets' gameplay. The road generates procedurally ahead of the player and cleans up behind them, creating the illusion of an endless interstate.

---

## Features Implemented

### Road Segment System

**RoadSegment.cs** - Component for modular road pieces
- Configurable segment length and width
- Entry/exit connection points for seamless joining
- Spawn points for props, events, and items
- Segment types: Straight, curves, bridges, tunnels, rest stops
- Gizmo visualization in editor

**RoadSegmentBuilder.cs** - Editor tool for creating road geometry
- Procedurally generates road surface mesh
- Creates lane lines (yellow center, white lanes, edge lines)
- Auto-creates connection points
- Generates prop spawn points along roadsides
- URP-compatible materials

### Infinite Road Generation

**RoadGenerator.cs** - Manages the endless road
- Spawns segments ahead of player (configurable count)
- Despawns segments behind player
- Object pooling for performance
- Weighted random segment selection
- Events for segment spawn/despawn (used by prop system)

**RoadConfig.cs** - ScriptableObject for road settings
- Lane dimensions (width, count per direction)
- Shoulder and median widths
- Material references
- Environment settings (fog, ambient color)

### Prop Spawning System

**RoadPropData.cs** - Defines individual prop types
- Prefab reference
- Spawn chance and weight
- Allowed sides (left/right/both)
- Random rotation and scale options
- Clustering support

**RoadPropPool.cs** - Collections of props
- Weighted random selection
- Spawn point usage chance
- Min/max props per segment

**RoadPropSpawner.cs** - Places props on road segments
- Listens to RoadGenerator events
- Randomized spawn point selection
- Object pooling for recycling props
- Side-aware placement (left vs right)

---

## Technical Details

### Segment Connection
Segments use entry/exit Transform points. When spawning a new segment:
1. Get previous segment's exit position/rotation
2. Align new segment's entry point to match
3. Offset segment position to account for entry point location

### Object Pooling
Both road segments and props are pooled:
- Segments pooled by type (straight, curve, etc.)
- Props pooled by RoadPropData reference
- Despawned objects returned to pool instead of destroyed

### Spawn Point Randomization
Each segment has multiple prop spawn points. On spawn:
1. Shuffle spawn point indices
2. Roll against spawn point usage chance
3. Select random prop from pool (weighted)
4. Roll against individual prop spawn chance
5. Apply position offset, rotation, scale variation

---

## Setup Instructions

### Creating a Road Segment Prefab
1. Create empty GameObject
2. Add `RoadSegment` component
3. Add `RoadSegmentBuilder` component
4. Assign a `RoadConfig` asset
5. Right-click RoadSegmentBuilder → "Build Road Segment"
6. Save as prefab

### Setting Up Road Generation
1. Create `RoadGenerator` object in scene
2. Assign player reference
3. Add segment prefabs to array
4. Configure segments ahead/behind counts

### Setting Up Props
1. Create `RoadPropData` assets for each prop type
2. Create `RoadPropPool` asset, add prop data references
3. Create `RoadPropSpawner` object in scene
4. Assign RoadGenerator and prop pools

---

## Future Improvements

- [ ] Curve segments (slight left/right bends)
- [ ] Special segments (bridges, overpasses, rest stops)
- [ ] Biome/environment variations
- [ ] Distance-based difficulty scaling
- [ ] Landmark/unique segment injection

---

## Related Files

```
Assets/Scripts/Road/
├── RoadSegment.cs
├── RoadGenerator.cs
├── RoadConfig.cs
├── RoadSegmentBuilder.cs
├── RoadPropData.cs
├── RoadPropPool.cs
└── RoadPropSpawner.cs

Assets/Settings/
└── RoadConfig.asset

Assets/Prefabs/
└── RoadSegment_Straight.prefab
```
