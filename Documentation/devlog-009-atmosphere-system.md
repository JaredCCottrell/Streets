# Devlog 009 - Eerie Atmosphere System

**Date:** December 16, 2025

---

## Overview

Implemented an atmosphere system to create the eerie, unsettling feel of a desolate highway at night. The system manages fog, lighting, post-processing effects, and subtle horror elements like light flickering.

---

## Features Implemented

### Atmosphere Configuration

**AtmosphereConfig.cs** - ScriptableObject for all atmosphere settings
- Fog settings (mode, color, density, distance)
- Lighting (ambient color/intensity, directional light)
- Skybox tint and exposure
- Post-processing overrides (contrast, saturation, vignette, film grain, bloom)
- Eerie effects (light flicker timing and intensity)
- Distance-based fog variation

### Runtime Controller

**AtmosphereController.cs** - Applies and manages atmosphere at runtime
- Initializes fog, lighting, and skybox on start
- Configures URP post-processing volume
- Light flicker system with random intervals
- Fog density increases with distance traveled
- Public methods for runtime atmosphere changes:
  - `SetFogDensity(float)` - Adjust fog
  - `TriggerFlicker()` - Force a light flicker
  - `PulseDarkness(duration, intensity)` - Momentary darkness for scares

### Editor Setup Tool

**AtmosphereSetupTool.cs** - One-click atmosphere setup
- Creates pre-configured eerie atmosphere config
- Sets up Global Volume with horror post-processing
- Adds AtmosphereController to scene
- Auto-assigns references

---

## Technical Details

### Default Eerie Settings

| Setting | Value | Effect |
|---------|-------|--------|
| Fog Density | 0.025 | Thick but not blinding |
| Fog Color | Dark blue-gray | Cold, isolated feel |
| Ambient Intensity | 0.25 | Very dim environment |
| Light Intensity | 0.15 | Weak moonlight |
| Saturation | -30 | Desaturated, lifeless |
| Vignette | 0.4 | Darker edges |
| Film Grain | 0.25 | Subtle noise/grit |

### Light Flicker System
```
1. Schedule random flicker (8-45 seconds apart)
2. Roll against flicker chance (3%)
3. If triggered: rapidly vary light intensity for 0.1-0.5s
4. Also affects ambient lighting slightly
5. Schedule next flicker
```

### Distance Fog Progression
- Base fog density: 0.025
- Increase per km: 0.008
- Max density: 0.06
- Creates sense of descent into darkness

### Post-Processing Stack
- **Color Adjustments**: +25 contrast, -30 saturation, -0.3 exposure
- **Vignette**: 0.4 intensity, black color
- **Bloom**: 0.3 intensity, 0.9 threshold
- **Film Grain**: 0.25 intensity
- **Lift/Gamma/Gain**: Blue tint in shadows

---

## Setup Instructions

### Quick Setup
1. Open **Streets → Setup Atmosphere**
2. Click **"Do All Steps"**
3. Optionally assign RoadGenerator reference for distance fog

### Manual Setup
1. Create AtmosphereConfig asset (Right-click → Create → Streets → Atmosphere Config)
2. Add Global Volume to scene with URP Volume Profile
3. Create AtmosphereController GameObject
4. Assign config and volume references
5. Right-click controller → "Apply Settings Now"

### Runtime Usage
```csharp
// Get controller reference
AtmosphereController atmosphere = FindObjectOfType<AtmosphereController>();

// Trigger effects for scares
atmosphere.TriggerFlicker();
atmosphere.PulseDarkness(2f, 0.05f); // 2 second darkness pulse

// Adjust fog dynamically
atmosphere.SetFogDensity(0.05f);
```

---

## Related Files

```
Assets/Scripts/Atmosphere/
├── AtmosphereConfig.cs      # Settings ScriptableObject
└── AtmosphereController.cs  # Runtime controller

Assets/Editor/
└── AtmosphereSetupTool.cs   # Setup wizard

Assets/Settings/
├── EerieAtmosphereConfig.asset  # Default horror config
└── EerieVolumeProfile.asset     # Post-processing profile
```

---

## Future Improvements

- [ ] Multiple atmosphere presets (clear night, stormy, foggy)
- [ ] Weather transitions
- [ ] Dynamic cloud/sky system
- [ ] Ambient audio integration
- [ ] Time-of-day cycle
