using System;
using UnityEngine;

namespace Streets.Inventory
{
    public class HotbarSystem : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private int hotbarSize = 4;

        [Header("References")]
        [SerializeField] private InventorySystem inventory;

        // State - stores inventory slot indices (-1 = empty)
        private int[] hotbarSlots;
        private int selectedIndex = 0;

        // Events
        public event Action<int, int> OnHotbarSlotChanged; // hotbarIndex, inventorySlotIndex
        public event Action<int> OnSelectionChanged; // selectedIndex

        // Properties
        public int HotbarSize => hotbarSize;
        public int SelectedIndex => selectedIndex;
        public int[] HotbarSlots => hotbarSlots;

        private void Awake()
        {
            hotbarSlots = new int[hotbarSize];
            for (int i = 0; i < hotbarSize; i++)
            {
                hotbarSlots[i] = -1;
            }
        }

        private void OnEnable()
        {
            if (inventory != null)
            {
                inventory.OnSlotChanged += HandleInventorySlotChanged;
            }
        }

        private void OnDisable()
        {
            if (inventory != null)
            {
                inventory.OnSlotChanged -= HandleInventorySlotChanged;
            }
        }

        private void HandleInventorySlotChanged(int slotIndex, InventorySlot slot)
        {
            // If an inventory slot that's assigned to hotbar becomes empty, update hotbar
            for (int i = 0; i < hotbarSlots.Length; i++)
            {
                if (hotbarSlots[i] == slotIndex)
                {
                    OnHotbarSlotChanged?.Invoke(i, slotIndex);
                }
            }
        }

        public void AssignSlot(int hotbarIndex, int inventorySlotIndex)
        {
            if (hotbarIndex < 0 || hotbarIndex >= hotbarSlots.Length) return;

            // Remove this inventory slot from any other hotbar slot
            for (int i = 0; i < hotbarSlots.Length; i++)
            {
                if (hotbarSlots[i] == inventorySlotIndex && i != hotbarIndex)
                {
                    hotbarSlots[i] = -1;
                    OnHotbarSlotChanged?.Invoke(i, -1);
                }
            }

            hotbarSlots[hotbarIndex] = inventorySlotIndex;
            OnHotbarSlotChanged?.Invoke(hotbarIndex, inventorySlotIndex);
        }

        public void ClearSlot(int hotbarIndex)
        {
            if (hotbarIndex < 0 || hotbarIndex >= hotbarSlots.Length) return;

            hotbarSlots[hotbarIndex] = -1;
            OnHotbarSlotChanged?.Invoke(hotbarIndex, -1);
        }

        public void UseHotbarSlot(int hotbarIndex)
        {
            if (hotbarIndex < 0 || hotbarIndex >= hotbarSlots.Length) return;

            int inventoryIndex = hotbarSlots[hotbarIndex];
            if (inventoryIndex < 0) return;

            inventory?.UseItem(inventoryIndex);
        }

        public void SelectSlot(int hotbarIndex)
        {
            if (hotbarIndex < 0 || hotbarIndex >= hotbarSlots.Length) return;

            selectedIndex = hotbarIndex;
            OnSelectionChanged?.Invoke(selectedIndex);
        }

        public void UseSelectedSlot()
        {
            UseHotbarSlot(selectedIndex);
        }

        public ItemData GetHotbarItem(int hotbarIndex)
        {
            if (hotbarIndex < 0 || hotbarIndex >= hotbarSlots.Length) return null;

            int inventoryIndex = hotbarSlots[hotbarIndex];
            if (inventoryIndex < 0 || inventory == null) return null;

            InventorySlot slot = inventory.GetSlot(inventoryIndex);
            return slot?.item;
        }

        public int GetHotbarItemQuantity(int hotbarIndex)
        {
            if (hotbarIndex < 0 || hotbarIndex >= hotbarSlots.Length) return 0;

            int inventoryIndex = hotbarSlots[hotbarIndex];
            if (inventoryIndex < 0 || inventory == null) return 0;

            InventorySlot slot = inventory.GetSlot(inventoryIndex);
            return slot?.quantity ?? 0;
        }

        public bool IsSlotEmpty(int hotbarIndex)
        {
            if (hotbarIndex < 0 || hotbarIndex >= hotbarSlots.Length) return true;
            return hotbarSlots[hotbarIndex] < 0 || GetHotbarItem(hotbarIndex) == null;
        }

        public int GetInventorySlotIndex(int hotbarIndex)
        {
            if (hotbarIndex < 0 || hotbarIndex >= hotbarSlots.Length) return -1;
            return hotbarSlots[hotbarIndex];
        }
    }
}
