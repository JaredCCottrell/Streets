# Devlog 008 - Inventory Improvements

**Date:** December 15, 2025

---

## Overview

Enhanced the inventory system with drag-and-drop functionality and improved the context menu UI for better usability.

---

## Features Implemented

### Drag and Drop

**InventorySlotUI.cs** - Added drag interfaces
- `IBeginDragHandler` - Start dragging an item
- `IDragHandler` - Update drag position
- `IEndDragHandler` - Complete or cancel drag
- `IDropHandler` - Receive dropped items
- Visual feedback: source slot dims during drag

**InventoryUI.cs** - Drag management
- Creates floating drag icon that follows cursor
- Tracks dragged slot index
- Handles drop on inventory slots (swap)
- Handles drop on hotbar slots (assign)
- Auto-finds Canvas for proper positioning

**HotbarSlotUIComponent.cs** - New MonoBehaviour for hotbar slots
- Receives drops from inventory
- Highlights when valid drop target
- Replaces serializable HotbarSlotUI for drop support

### Context Menu Improvements

**ContextMenuBuilder.cs** - Programmatic menu construction
- Creates compact, properly-sized buttons
- Vertical layout with consistent spacing
- Auto-generates Use, Drop, and Hotbar buttons
- Configurable dimensions and colors
- Editor tool: Right-click → "Build Context Menu"

**Positioning Fix**
- Menu now appears at slot's top-right corner
- Uses RectTransform world corners for accurate placement
- Pivot adjusted for proper expansion direction

---

## Technical Details

### Drag Icon System
```
1. OnBeginDrag: Create/show floating icon with item sprite
2. OnDrag: Update icon position to mouse
3. OnEndDrag: Hide icon, restore source slot alpha
4. OnDrop: Swap slots or assign to hotbar
```

### Context Menu Builder Settings
| Setting | Default | Description |
|---------|---------|-------------|
| Menu Width | 120px | Total menu width |
| Button Height | 30px | Individual button height |
| Button Spacing | 4px | Gap between buttons |
| Padding | 6px | Menu edge padding |
| Font Size | 14 | Button text size |

### Bug Fixes
- **Item pickup key**: Dropped items now correctly use E key instead of inheriting prefab's serialized value (F8)
- Set `pickupKey = Key.E` in `ItemPickup.Initialize()`

---

## Setup Instructions

### Enabling Drag-and-Drop
Drag-and-drop works automatically with the updated scripts. No additional setup required for inventory slots.

### Hotbar Drop Support
1. Add `HotbarSlotUIComponent` to each hotbar slot GameObject
2. Assign UI elements (background, icon, text)
3. In `HotbarUI`, assign slots to **Slot Components** array
4. Assign **Inventory UI** reference

### Using Context Menu Builder
1. Select context menu GameObject
2. Add `ContextMenuBuilder` component
3. Right-click → "Build Context Menu"
4. In `InventoryUI`, assign the **Context Menu Builder** reference

---

## Related Files

```
Assets/Scripts/UI/
├── InventoryUI.cs           # Drag management
├── InventorySlotUI.cs       # Drag interfaces
├── HotbarUI.cs              # Updated for components
├── HotbarSlotUIComponent.cs # New - drop target
└── ContextMenuBuilder.cs    # New - menu builder

Assets/Scripts/Inventory/
└── ItemPickup.cs            # Pickup key fix
```
