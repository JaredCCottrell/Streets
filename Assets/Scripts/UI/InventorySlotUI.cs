using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Streets.Inventory;

namespace Streets.UI
{
    public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI Elements")]
        [SerializeField] private Image background;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI quantityText;

        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        [SerializeField] private Color hoverColor = new Color(0.3f, 0.3f, 0.3f, 0.9f);
        [SerializeField] private Color selectedColor = new Color(0.4f, 0.4f, 0.2f, 0.9f);

        // State
        private int slotIndex;
        private InventoryUI inventoryUI;
        private InventorySlot currentSlot;
        private bool isHovered;
        private bool isSelected;

        public int SlotIndex => slotIndex;
        public InventorySlot CurrentSlot => currentSlot;

        public void Initialize(int index, InventoryUI ui)
        {
            slotIndex = index;
            inventoryUI = ui;
        }

        public void UpdateSlot(InventorySlot slot)
        {
            currentSlot = slot;

            if (slot == null || slot.IsEmpty)
            {
                if (icon != null)
                {
                    icon.sprite = null;
                    icon.enabled = false;
                }
                if (quantityText != null)
                {
                    quantityText.text = "";
                    quantityText.enabled = false;
                }
            }
            else
            {
                if (icon != null)
                {
                    icon.sprite = slot.item?.icon;
                    icon.enabled = slot.item?.icon != null;
                }
                if (quantityText != null)
                {
                    if (slot.item != null && slot.item.isStackable && slot.quantity > 1)
                    {
                        quantityText.text = slot.quantity.ToString();
                        quantityText.enabled = true;
                    }
                    else
                    {
                        quantityText.text = "";
                        quantityText.enabled = false;
                    }
                }
            }

            UpdateVisuals();
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (background == null) return;

            if (isSelected)
            {
                background.color = selectedColor;
            }
            else if (isHovered)
            {
                background.color = hoverColor;
            }
            else
            {
                background.color = normalColor;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (inventoryUI == null) return;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                inventoryUI.OnSlotLeftClick(slotIndex);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                inventoryUI.OnSlotRightClick(slotIndex);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            UpdateVisuals();
            inventoryUI?.OnSlotHover(slotIndex);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            UpdateVisuals();
            inventoryUI?.OnSlotHoverExit(slotIndex);
        }
    }
}
