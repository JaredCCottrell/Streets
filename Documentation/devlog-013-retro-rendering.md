# Devlog 013: Retro Rendering Effects

## Overview

Added post-processing effects to give Streets a retro visual style reminiscent of PS1-era games and old CRT monitors. The system includes two URP Renderer Features that can be used independently or combined.

## Features Implemented

### 1. Pixelation Effect

Reduces the effective resolution to create chunky, retro-style pixels.

**Files:**
- `Assets/Shaders/Pixelation.shader` - The pixelation shader
- `Assets/Scripts/Rendering/PixelationRenderFeature.cs` - URP Renderer Feature
- `Assets/Scripts/Rendering/PixelationController.cs` - Runtime control component

**How It Works:**
- Samples the screen at reduced UV coordinates
- Uses point filtering (nearest neighbor) for sharp pixel edges
- Configurable pixel size (1-32)

**Settings:**
| Parameter | Range | Description |
|-----------|-------|-------------|
| Pixel Size | 1-32 | Size of each pixel block. Higher = more pixelated |
| Enabled | bool | Toggle effect on/off |

**Recommended Values:**
- Subtle: 2-3
- Light: 4-5
- Medium (PS1 style): 6-8
- Heavy: 10-12
- Extreme: 16+

### 2. CRT Effect

Simulates the look of old CRT monitors with scanlines, screen curvature, and color artifacts.

**Files:**
- `Assets/Shaders/CRT.shader` - The CRT shader
- `Assets/Scripts/Rendering/CRTRenderFeature.cs` - URP Renderer Feature

**Features:**
- **Scanlines** - Horizontal dark lines simulating CRT electron beam
- **Screen Curvature** - Barrel distortion for curved CRT look
- **Vignette** - Darkening at screen edges
- **Chromatic Aberration** - RGB color channel separation
- **Phosphor Simulation** - Subtle vertical color banding
- **Flicker** - Brightness variation over time

**Settings:**
| Parameter | Default | Range | Description |
|-----------|---------|-------|-------------|
| Scanline Intensity | 0.3 | 0-1 | Visibility of scanlines |
| Scanline Count | 400 | 100-1000 | Number of scanlines |
| Curvature | 0.1 | 0-0.5 | Screen bend amount |
| Vignette Intensity | 0.5 | 0-2 | Edge darkening strength |
| Chromatic Aberration | 1.0 | 0-5 | RGB separation amount |
| Brightness | 1.1 | 0.5-2 | Overall brightness multiplier |
| Flicker | 0.1 | 0-1 | Brightness wobble intensity |

## Setup Instructions

### Adding the Effects

1. Open `Assets/Settings/PC_Renderer` in the Inspector
2. Scroll to the bottom and click **Add Renderer Feature**
3. Select **Pixelation Render Feature** and/or **CRT Render Feature**
4. Configure settings as desired

### Recommended Order

For the full retro experience, add both features in this order:
1. **Pixelation Render Feature** (applied first)
2. **CRT Render Feature** (applied on top)

This ensures the CRT effects (scanlines, curvature) are applied to the already-pixelated image.

### Runtime Control (Pixelation)

Add the `PixelationController` component to any GameObject for runtime control:

```csharp
// Access via component
var controller = GetComponent<PixelationController>();
controller.PixelSize = 8;
controller.EffectEnabled = true;
controller.SetPreset(PixelationController.PixelPreset.Heavy);
```

**Debug Keys (Editor Only):**
- `[` - Decrease pixel size
- `]` - Increase pixel size
- `P` - Toggle pixelation on/off

## Technical Implementation

### RenderGraph API

Both effects implement the modern URP RenderGraph API via `RecordRenderGraph()` for compatibility with Unity 2023+. A legacy `Execute()` fallback is included for older URP versions.

```csharp
public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
{
    var resourceData = frameData.Get<UniversalResourceData>();
    var source = resourceData.activeColorTexture;

    // Create temp texture, blit with effect, copy back
    TextureHandle destination = renderGraph.CreateTexture(destinationDesc);
    // ... render passes
}
```

### Shader Structure

Both shaders use URP's Blit infrastructure:
- Include `Blit.hlsl` for vertex shader and texture sampling
- Use `_BlitTexture` as the source texture
- Output directly to render target

### Namespace

All rendering code is under `Streets.Rendering` namespace.

## Performance Notes

- Both effects are full-screen post-process passes
- Minimal performance impact (simple fragment shaders)
- Can be disabled at runtime for performance-sensitive situations
- Consider reducing effect intensity on lower-end hardware

## Visual Presets

### "PS1 Horror" (Recommended for Streets)
- Pixelation: 6
- Scanline Intensity: 0.2
- Curvature: 0.08
- Chromatic Aberration: 0.8
- Vignette: 0.6

### "Old TV"
- Pixelation: 4
- Scanline Intensity: 0.4
- Curvature: 0.15
- Chromatic Aberration: 1.5
- Flicker: 0.2

### "Subtle Retro"
- Pixelation: 3
- Scanline Intensity: 0.15
- Curvature: 0.05
- Chromatic Aberration: 0.5

## Future Enhancements

Potential additions:
- Color grading/posterization for limited color palettes
- Dithering patterns
- Vertex jitter (PS1-style wobble)
- Texture affine warping
- VHS tape effects (noise, tracking lines)
