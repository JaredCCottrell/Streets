# Development Log 002 - Core Design Decisions

**Date:** December 12, 2025

## Design Questions Resolved

All core gameplay questions have been answered. Here's the complete design document:

---

## Player Movement

- **First-person perspective**
- **Walk** - Default movement speed
- **Sprint** - Faster movement, consumes stamina
- **Stamina** - Depletes while sprinting, regenerates while walking/standing

---

## Threat System

Events are a **mix of harmless and harmful**:

- **Atmospheric Events** - Creepy but harmless; build tension and dread
- **Dangerous Encounters** - Can damage or kill the player

The unpredictability keeps players constantly on edge - they never know if something is a scare or a real threat.

---

## Survival Systems

Five core meters to manage:

| Meter | Description |
|-------|-------------|
| **Health** | Damaged by harmful encounters, death when depleted |
| **Stamina** | Drains when sprinting, regenerates over time |
| **Hunger** | Depletes over time, must find food to restore |
| **Thirst** | Depletes over time, must find drinks to restore |
| **Sanity** | Affected by creepy events (triggers TBD) |

### Inventory System
- Player has an inventory for carrying items
- Specific items to be designed later
- Likely categories: food, drinks, tools, light sources, defensive items

---

## Game Structure

### Goal
Reach the end of the road. (What awaits at the end is TBD)

### Game Modes

| Mode | Description |
|------|-------------|
| **Normal** | Checkpoints save progress; respawn at last checkpoint on death |
| **Hardcore** | No checkpoints; death resets all progress to the beginning |
| **Endless** | (Future) Survival mode tracking distance traveled |

---

## Implications for Level Design

Roadside locations become critical survival points:
- **Rest stops** - Scavenge for food, water, supplies
- **Gas stations** - Fuel? Items? Safe zones?
- **Abandoned vehicles** - Loot opportunities, environmental storytelling
- **Other structures** - Motels, diners, overpasses (TBD)

The procedural system needs to spawn these at reasonable intervals to prevent soft-locks from starvation/dehydration.

---

## Next Steps

1. Build first-person controller with sprint/stamina
2. Create survival meter system (health, hunger, thirst, sanity)
3. Design inventory system architecture
4. Prototype procedural road generation
5. Create first test events (one harmless, one harmful)

---

*Repository: https://github.com/JaredCCottrell/Streets*
