# Devlog 012: UI Scaling and Custom Stamina Bar

## Overview
This session focused on fixing UI scaling issues across different resolutions and implementing custom sprite-based UI bars.

## UI Scaling Fixes

### Problem
The inventory and player bar UIs were using "Constant Pixel Size" mode, causing them to:
- Change size and position at different resolutions
- Not stay centered/anchored properly

### Solution
Updated Canvas Scaler settings for both canvases:

**Canvas (Inventory) & Canvas (Player Bars):**
- UI Scale Mode: Constant Pixel Size → Scale With Screen Size
- Reference Resolution: 1920 x 1080
- Screen Match Mode: Match Width Or Height
- Match: 0.5 (balanced scaling)

### Inventory Panel Fixes
- Fixed InventoryPanel anchors for center positioning
- Fixed SlotsContainer positioning (was 736px offset, now centered)
- Moved DetailsPanel to be a sibling of InventoryPanel (not a child)
- DetailsPanel now appears to the right of the inventory
- Fixed ItemIcon, ItemName, ItemDescription layout:
  - Stacked vertically (icon at top, name below, description at bottom)
  - Reduced font sizes (Name: 18, Description: 14)

## Custom Stamina Bar

### Implementation
Replaced the default rectangular bar with custom lung sprites:
- **Background**: `lungsfull.png` - Shows the full/empty state
- **Fill**: `lungs.png` - Animates horizontally based on stamina level

### Settings
- Image Type: Filled (horizontal, left origin)
- Preserve Aspect: Enabled
- Fixed RectTransform scale (was 5x0.25, now 1x1)
- Anchored to bottom-left corner

## Files Changed
- `Assets/Scenes/SampleScene.unity` - UI hierarchy and component changes
- `Assets/Textures/UI/lungs.png` - Stamina fill sprite
- `Assets/Textures/UI/lungsfull.png` - Stamina background sprite

## Next Steps
- Create custom sprites for other bars (health, hunger, thirst, sanity)
- Apply same sprite-based bar system to all survival meters
