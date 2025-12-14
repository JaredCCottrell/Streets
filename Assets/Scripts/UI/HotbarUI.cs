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

        [Header("Slot UI Elements")]
        [SerializeField] private HotbarSlotUI[] slotUIs;

        [Header("Selection")]
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
            RefreshAllSlots();
            UpdateSelection(hotbarSystem?.SelectedIndex ?? 0);
        }

        private void OnInventorySlotChanged(int slotIndex, InventorySlot slot)
        {
            // Check if this inventory slot is assigned to any hotbar slot
            if (hotbarSystem == null) return;

            for (int i = 0; i < slotUIs.Length; i++)
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
            if (hotbarIndex < 0 || hotbarIndex >= slotUIs.Length) return;
            if (slotUIs[hotbarIndex] == null) return;

            ItemData item = hotbarSystem?.GetHotbarItem(hotbarIndex);
            int quantity = hotbarSystem?.GetHotbarItemQuantity(hotbarIndex) ?? 0;

            slotUIs[hotbarIndex].SetItem(item, quantity);
        }

        private void UpdateSelection(int selectedIndex)
        {
            for (int i = 0; i < slotUIs.Length; i++)
            {
                if (slotUIs[i] != null)
                {
                    slotUIs[i].SetSelected(i == selectedIndex, selectedColor, normalColor);
                }
            }
        }

        private void RefreshAllSlots()
        {
            for (int i = 0; i < slotUIs.Length; i++)
            {
                UpdateSlotUI(i);
            }
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
