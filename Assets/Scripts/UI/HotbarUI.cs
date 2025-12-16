using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Streets.Inventory;

namespace Streets.UI
{
    public class HotbarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HotbarSystem hotbarSystem;
        [SerializeField] private InventorySystem inventorySystem;
        [SerializeField] private InventoryUI inventoryUI;

        [Header("Slot UI Elements")]
        [SerializeField] private HotbarSlotUIComponent[] slotComponents;

        [Header("Legacy Slot UI (for backwards compatibility)")]
        [SerializeField] private HotbarSlotUI[] slotUIs;

        [Header("Selection Colors")]
        [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        [SerializeField] private Color selectedColor = new Color(0.4f, 0.4f, 0.2f, 0.9f);

        private void OnEnable()
        {
            if (hotbarSystem != null)
            {
                hotbarSystem.OnHotbarSlotChanged += UpdateSlot;
                hotbarSystem.OnSelectionChanged += UpdateSelection;
            }

            if (inventorySystem != null)
            {
                inventorySystem.OnSlotChanged += OnInventorySlotChanged;
            }
        }

        private void OnDisable()
        {
            if (hotbarSystem != null)
            {
                hotbarSystem.OnHotbarSlotChanged -= UpdateSlot;
                hotbarSystem.OnSelectionChanged -= UpdateSelection;
            }

            if (inventorySystem != null)
            {
                inventorySystem.OnSlotChanged -= OnInventorySlotChanged;
            }
        }

        private void Start()
        {
            InitializeSlotComponents();
            RefreshAllSlots();
            UpdateSelection(hotbarSystem?.SelectedIndex ?? 0);
        }

        private void InitializeSlotComponents()
        {
            if (slotComponents == null || inventoryUI == null) return;

            for (int i = 0; i < slotComponents.Length; i++)
            {
                if (slotComponents[i] != null)
                {
                    slotComponents[i].Initialize(i, inventoryUI);
                }
            }
        }

        private void OnInventorySlotChanged(int slotIndex, InventorySlot slot)
        {
            // Check if this inventory slot is assigned to any hotbar slot
            if (hotbarSystem == null) return;

            for (int i = 0; i < GetSlotCount(); i++)
            {
                if (hotbarSystem.GetInventorySlotIndex(i) == slotIndex)
                {
                    UpdateSlotUI(i);
                }
            }
        }

        private void UpdateSlot(int hotbarIndex, int inventorySlotIndex)
        {
            UpdateSlotUI(hotbarIndex);
        }

        private void UpdateSlotUI(int hotbarIndex)
        {
            ItemData item = hotbarSystem?.GetHotbarItem(hotbarIndex);
            int quantity = hotbarSystem?.GetHotbarItemQuantity(hotbarIndex) ?? 0;

            // Try new component system first
            if (slotComponents != null && hotbarIndex >= 0 && hotbarIndex < slotComponents.Length && slotComponents[hotbarIndex] != null)
            {
                slotComponents[hotbarIndex].SetItem(item, quantity);
            }
            // Fall back to legacy system
            else if (slotUIs != null && hotbarIndex >= 0 && hotbarIndex < slotUIs.Length && slotUIs[hotbarIndex] != null)
            {
                slotUIs[hotbarIndex].SetItem(item, quantity);
            }
        }

        private void UpdateSelection(int selectedIndex)
        {
            // Try new component system first
            if (slotComponents != null && slotComponents.Length > 0)
            {
                for (int i = 0; i < slotComponents.Length; i++)
                {
                    if (slotComponents[i] != null)
                    {
                        slotComponents[i].SetSelected(i == selectedIndex);
                    }
                }
            }
            // Fall back to legacy system
            else if (slotUIs != null)
            {
                for (int i = 0; i < slotUIs.Length; i++)
                {
                    if (slotUIs[i] != null)
                    {
                        slotUIs[i].SetSelected(i == selectedIndex, selectedColor, normalColor);
                    }
                }
            }
        }

        private void RefreshAllSlots()
        {
            for (int i = 0; i < GetSlotCount(); i++)
            {
                UpdateSlotUI(i);
            }
        }

        private int GetSlotCount()
        {
            if (slotComponents != null && slotComponents.Length > 0)
            {
                return slotComponents.Length;
            }
            if (slotUIs != null)
            {
                return slotUIs.Length;
            }
            return 0;
        }
    }

    [System.Serializable]
    public class HotbarSlotUI
    {
        public Image background;
        public Image icon;
        public TextMeshProUGUI quantityText;
        public TextMeshProUGUI keyText;

        public void SetItem(ItemData item, int quantity)
        {
            if (icon != null)
            {
                if (item != null && item.icon != null)
                {
                    icon.sprite = item.icon;
                    icon.enabled = true;
                }
                else
                {
                    icon.sprite = null;
                    icon.enabled = false;
                }
            }

            if (quantityText != null)
            {
                if (item != null && item.isStackable && quantity > 1)
                {
                    quantityText.text = quantity.ToString();
                    quantityText.enabled = true;
                }
                else
                {
                    quantityText.text = "";
                    quantityText.enabled = false;
                }
            }
        }

        public void SetSelected(bool selected, Color selectedColor, Color normalColor)
        {
            if (background != null)
            {
                background.color = selected ? selectedColor : normalColor;
            }
        }
    }
}
