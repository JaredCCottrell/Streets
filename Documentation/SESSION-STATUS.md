# Streets - Development Session Status

**Last Updated:** December 12, 2025

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

### Next Up
- [ ] Design inventory system architecture
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
│   ├── UI/
│   │   ├── StaminaUI.cs
│   │   ├── HealthUI.cs
│   │   ├── HungerUI.cs
│   │   ├── ThirstUI.cs
│   │   └── SanityUI.cs
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
- [ ] Child camera at eye height

### Component References
- [ ] FirstPersonController → Camera Transform assigned
- [ ] FirstPersonController → Ground Mask set to "Ground" layer
- [ ] FirstPersonController → HungerSystem & ThirstSystem assigned
- [ ] HungerSystem → HealthSystem assigned
- [ ] ThirstSystem → HealthSystem assigned
- [ ] SanitySystem → HealthSystem assigned

### Ground
- [ ] "Ground" layer created
- [ ] Floor objects set to Ground layer

### UI (Canvas)
- [ ] StaminaUI with fill bar
- [ ] HealthUI with fill bar
- [ ] HungerUI with fill bar
- [ ] ThirstUI with fill bar
- [ ] SanityUI with fill bar

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
