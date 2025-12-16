using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Streets.Inventory;

namespace Streets.UI
{
    public class HotbarSlotUIComponent : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI Elements")]
        [SerializeField] private Image background;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private TextMeshProUGUI keyText;

        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        [SerializeField] private Color selectedColor = new Color(0.4f, 0.4f, 0.2f, 0.9f);
        [SerializeField] private Color dropTargetColor = new Color(0.2f, 0.4f, 0.2f, 0.9f);

        // State
        private int hotbarIndex;
        private InventoryUI inventoryUI;
        private bool isSelected;
        private bool isDropTarget;

        public int HotbarIndex => hotbarIndex;

        public void Initialize(int index, InventoryUI ui)
        {
            hotbarIndex = index;
            inventoryUI = ui;

            if (keyText != null)
            {
                keyText.text = (index + 1).ToString();
            }
        }

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

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (background == null) return;

            if (isDropTarget)
            {
                background.color = dropTargetColor;
            }
            else if (isSelected)
            {
                background.color = selectedColor;
            }
            else
            {
                background.color = normalColor;
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (inventoryUI != null && inventoryUI.IsDragging)
            {
                inventoryUI.OnDropOnHotbar(hotbarIndex);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (inventoryUI != null && inventoryUI.IsDragging)
            {
                isDropTarget = true;
                UpdateVisuals();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isDropTarget = false;
            UpdateVisuals();
        }
    }
}
