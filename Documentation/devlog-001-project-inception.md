# Development Log 001 - Project Inception

**Date:** December 12, 2025

## Project Overview

**Streets** is a first-person survival horror game set on an endless, eerie interstate highway.

## Core Concept

- **Genre:** Survival horror / walking simulator hybrid
- **Perspective:** First-person
- **Setting:** Abandoned interstate highway with a liminal, unsettling atmosphere
- **Visual Style:** Stylized with an eerie, abandoned aesthetic
- **Platform:** PC
- **Team:** Solo project

## Gameplay Vision

The player walks down a seemingly endless procedurally generated interstate. As they progress, random creepy events occur - creating tension and dread. The further the player travels, the stranger things become.

## Technical Architecture (Planned)

### 1. Player Controller
- First-person movement and camera
- Interaction system for objects
- Potential stamina system

### 2. Procedural Road System
- Modular road segments that snap together
- Chunk-based loading (spawn ahead, despawn behind player)
- Variety: straight roads, curves, overpasses, rest stops, roadside elements

### 3. Event/Encounter System
- Random triggers based on distance or time
- Categories: visual scares, audio events, interactive threats, environmental hazards
- Escalating intensity with progression

### 4. Atmosphere
- Perpetual dusk/night lighting
- Fog and muted color palette
- Ambient soundscape (wind, distant sounds, unsettling noises)
- Environmental storytelling through abandoned objects

## Open Questions (To Be Decided)

1. Can the player run? Is there stamina?
2. Are creepy events purely atmospheric, or can they damage/kill the player?
3. Does the player have inventory, health, or survival meters?
4. Is there an end goal, or is it endless survival for distance high score?

## Next Steps

- Set up first-person player controller
- Create procedural road generation system
- Build modular road segment prefabs
- Implement chunk loading/unloading for infinite road
- Create basic event/encounter system framework
- Set up eerie atmosphere (lighting, fog, skybox)

---

*Repository: https://github.com/JaredCCottrell/Streets*
