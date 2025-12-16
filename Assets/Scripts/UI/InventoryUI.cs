using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;
using Streets.Inventory;

namespace Streets.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventorySystem inventorySystem;
        [SerializeField] private HotbarSystem hotbarSystem;

        [Header("Panel")]
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private InventorySlotUI slotPrefab;

        [Header("Item Details")]
        [SerializeField] private GameObject detailsPanel;
        [SerializeField] private Image detailsIcon;
        [SerializeField] private TextMeshProUGUI detailsName;
        [SerializeField] private TextMeshProUGUI detailsDescription;

        [Header("Context Menu")]
        [SerializeField] private GameObject contextMenu;
        [SerializeField] private ContextMenuBuilder contextMenuBuilder;
        [SerializeField] private Button useButton;
        [SerializeField] private Button dropButton;
        [SerializeField] private Button[] hotbarAssignButtons;

        [Header("Drag and Drop")]
        [SerializeField] private Canvas parentCanvas;
        [SerializeField] private GameObject dragIconPrefab;

        // State
        private InventorySlotUI[] slotUIs;
        private int selectedSlotIndex = -1;
        private int contextMenuSlotIndex = -1;
        private bool isOpen;

        // Drag state
        private int draggedSlotIndex = -1;
        private GameObject dragIconInstance;
        private RectTransform dragIconRect;
        private Image dragIconImage;

        public bool IsOpen => isOpen;
        public bool IsDragging => draggedSlotIndex >= 0;
        public int DraggedSlotIndex => draggedSlotIndex;

        private void Awake()
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
            }
            if (contextMenu != null)
            {
                contextMenu.SetActive(false);
            }
            if (detailsPanel != null)
            {
                detailsPanel.SetActive(false);
            }

            // Find canvas if not assigned
            if (parentCanvas == null)
            {
                parentCanvas = GetComponentInParent<Canvas>();
            }
        }

        private void Start()
        {
            CreateSlotUIs();
            SetupContextMenu();
            CreateDragIcon();
        }

        private void OnEnable()
        {
            if (inventorySystem != null)
            {
                inventorySystem.OnSlotChanged += UpdateSlot;
            }
        }

        private void OnDisable()
        {
            if (inventorySystem != null)
            {
                inventorySystem.OnSlotChanged -= UpdateSlot;
            }
        }

        private void CreateSlotUIs()
        {
            if (inventorySystem == null || slotsContainer == null || slotPrefab == null) return;

            slotUIs = new InventorySlotUI[inventorySystem.MaxSlots];

            for (int i = 0; i < inventorySystem.MaxSlots; i++)
            {
                InventorySlotUI slotUI = Instantiate(slotPrefab, slotsContainer);
                slotUI.Initialize(i, this);
                slotUI.UpdateSlot(inventorySystem.GetSlot(i));
                slotUIs[i] = slotUI;
            }
        }

        private void CreateDragIcon()
        {
            if (dragIconPrefab != null)
            {
                dragIconInstance = Instantiate(dragIconPrefab, parentCanvas.transform);
            }
            else
            {
                // Create a simple drag icon dynamically
                dragIconInstance = new GameObject("DragIcon");
                dragIconInstance.transform.SetParent(parentCanvas.transform, false);

                dragIconImage = dragIconInstance.AddComponent<Image>();
                dragIconImage.raycastTarget = false;

                dragIconRect = dragIconInstance.GetComponent<RectTransform>();
                dragIconRect.sizeDelta = new Vector2(50, 50);
            }

            if (dragIconInstance != null)
            {
                dragIconRect = dragIconInstance.GetComponent<RectTransform>();
                dragIconImage = dragIconInstance.GetComponent<Image>();
                if (dragIconImage != null)
                {
                    dragIconImage.raycastTarget = false;
                }
                dragIconInstance.SetActive(false);
            }
        }

        private void SetupContextMenu()
        {
            // Get buttons from builder if assigned
            if (contextMenuBuilder != null)
            {
                useButton = contextMenuBuilder.useButton;
                dropButton = contextMenuBuilder.dropButton;
                hotbarAssignButtons = contextMenuBuilder.hotbarButtons;
            }

            if (useButton != null)
            {
                useButton.onClick.AddListener(OnUseClicked);
            }
            if (dropButton != null)
            {
                dropButton.onClick.AddListener(OnDropClicked);
            }

            if (hotbarAssignButtons != null)
            {
                for (int i = 0; i < hotbarAssignButtons.Length; i++)
                {
                    int index = i;
                    if (hotbarAssignButtons[i] != null)
                    {
                        hotbarAssignButtons[i].onClick.AddListener(() => OnAssignToHotbar(index));
                    }
                }
            }
        }

        private void SetButtonText(Button button, string text)
        {
            TextMeshProUGUI tmp = button.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = text;
            }
            else
            {
                // Fallback to legacy Text component
                Text legacyText = button.GetComponentInChildren<Text>();
                if (legacyText != null)
                {
                    legacyText.text = text;
                }
            }
        }

        private void UpdateSlot(int slotIndex, InventorySlot slot)
        {
            if (slotUIs != null && slotIndex >= 0 && slotIndex < slotUIs.Length)
            {
                slotUIs[slotIndex]?.UpdateSlot(slot);
            }
        }

        public void Toggle()
        {
            if (isOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        public void Open()
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(true);
            }
            isOpen = true;
            RefreshAllSlots();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Close()
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
            }
            if (contextMenu != null)
            {
                contextMenu.SetActive(false);
            }
            if (detailsPanel != null)
            {
                detailsPanel.SetActive(false);
            }

            // Cancel any drag
            CancelDrag();

            isOpen = false;
            selectedSlotIndex = -1;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void RefreshAllSlots()
        {
            if (inventorySystem == null || slotUIs == null) return;

            for (int i = 0; i < slotUIs.Length; i++)
            {
                slotUIs[i]?.UpdateSlot(inventorySystem.GetSlot(i));
            }
        }

        public void OnSlotLeftClick(int slotIndex)
        {
            // Don't process click if we just finished a drag
            if (IsDragging) return;

            HideContextMenu();

            if (selectedSlotIndex >= 0 && selectedSlotIndex != slotIndex)
            {
                // Swap slots
                inventorySystem?.SwapSlots(selectedSlotIndex, slotIndex);
                ClearSelection();
            }
            else if (selectedSlotIndex == slotIndex)
            {
                ClearSelection();
            }
            else
            {
                SelectSlot(slotIndex);
            }
        }

        public void OnSlotRightClick(int slotIndex)
        {
            if (IsDragging) return;

            ClearSelection();

            InventorySlot slot = inventorySystem?.GetSlot(slotIndex);
            if (slot == null || slot.IsEmpty) return;

            ShowContextMenu(slotIndex);
        }

        public void OnSlotHover(int slotIndex)
        {
            InventorySlot slot = inventorySystem?.GetSlot(slotIndex);
            if (slot == null || slot.IsEmpty)
            {
                HideDetails();
                return;
            }

            ShowDetails(slot.item);
        }

        public void OnSlotHoverExit(int slotIndex)
        {
            HideDetails();
        }

        #region Drag and Drop

        public void OnBeginDragSlot(int slotIndex, PointerEventData eventData)
        {
            InventorySlot slot = inventorySystem?.GetSlot(slotIndex);
            if (slot == null || slot.IsEmpty) return;

            HideContextMenu();
            ClearSelection();

            draggedSlotIndex = slotIndex;

            // Setup drag icon
            if (dragIconInstance != null && dragIconImage != null)
            {
                dragIconImage.sprite = slot.item?.icon;
                dragIconImage.enabled = slot.item?.icon != null;
                dragIconInstance.SetActive(true);
                UpdateDragIconPosition(eventData);
            }

            // Dim the original slot
            if (slotUIs[slotIndex]?.Icon != null)
            {
                var iconColor = slotUIs[slotIndex].Icon.color;
                iconColor.a = 0.5f;
                slotUIs[slotIndex].Icon.color = iconColor;
            }
        }

        public void OnDragSlot(PointerEventData eventData)
        {
            if (!IsDragging) return;
            UpdateDragIconPosition(eventData);
        }

        public void OnEndDragSlot(PointerEventData eventData)
        {
            if (!IsDragging) return;

            // Restore original slot appearance
            if (draggedSlotIndex >= 0 && draggedSlotIndex < slotUIs.Length && slotUIs[draggedSlotIndex]?.Icon != null)
            {
                var iconColor = slotUIs[draggedSlotIndex].Icon.color;
                iconColor.a = 1f;
                slotUIs[draggedSlotIndex].Icon.color = iconColor;
            }

            // Hide drag icon
            if (dragIconInstance != null)
            {
                dragIconInstance.SetActive(false);
            }

            draggedSlotIndex = -1;
        }

        public void OnDropOnSlot(int targetSlotIndex, PointerEventData eventData)
        {
            if (!IsDragging) return;
            if (draggedSlotIndex == targetSlotIndex) return;

            // Swap the slots
            inventorySystem?.SwapSlots(draggedSlotIndex, targetSlotIndex);
        }

        public void OnDropOnHotbar(int hotbarIndex)
        {
            if (!IsDragging) return;

            // Assign the dragged inventory slot to the hotbar
            hotbarSystem?.AssignSlot(hotbarIndex, draggedSlotIndex);
        }

        private void UpdateDragIconPosition(PointerEventData eventData)
        {
            if (dragIconRect == null || parentCanvas == null) return;

            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                eventData.position,
                parentCanvas.worldCamera,
                out position);

            dragIconRect.anchoredPosition = position;
        }

        private void CancelDrag()
        {
            if (draggedSlotIndex >= 0 && draggedSlotIndex < slotUIs.Length && slotUIs[draggedSlotIndex]?.Icon != null)
            {
                var iconColor = slotUIs[draggedSlotIndex].Icon.color;
                iconColor.a = 1f;
                slotUIs[draggedSlotIndex].Icon.color = iconColor;
            }

            if (dragIconInstance != null)
            {
                dragIconInstance.SetActive(false);
            }

            draggedSlotIndex = -1;
        }

        #endregion

        private void SelectSlot(int slotIndex)
        {
            if (selectedSlotIndex >= 0 && slotUIs != null && selectedSlotIndex < slotUIs.Length)
            {
                slotUIs[selectedSlotIndex]?.SetSelected(false);
            }

            selectedSlotIndex = slotIndex;

            if (slotUIs != null && slotIndex >= 0 && slotIndex < slotUIs.Length)
            {
                slotUIs[slotIndex]?.SetSelected(true);
            }
        }

        private void ClearSelection()
        {
            if (selectedSlotIndex >= 0 && slotUIs != null && selectedSlotIndex < slotUIs.Length)
            {
                slotUIs[selectedSlotIndex]?.SetSelected(false);
            }
            selectedSlotIndex = -1;
        }

        private void ShowContextMenu(int slotIndex)
        {
            contextMenuSlotIndex = slotIndex;

            if (contextMenu != null && slotUIs != null && slotIndex >= 0 && slotIndex < slotUIs.Length)
            {
                contextMenu.SetActive(true);

                // Position next to the slot
                RectTransform slotRect = slotUIs[slotIndex].GetComponent<RectTransform>();
                RectTransform menuRect = contextMenu.GetComponent<RectTransform>();

                if (slotRect != null && menuRect != null)
                {
                    // Get slot's world corners
                    Vector3[] slotCorners = new Vector3[4];
                    slotRect.GetWorldCorners(slotCorners);

                    // Position menu to the right of the slot
                    Vector3 menuPosition = slotCorners[2]; // Top-right corner
                    menuRect.position = menuPosition;

                    // Adjust pivot so menu expands down and right from this point
                    menuRect.pivot = new Vector2(0, 1);
                }
            }

            // Configure buttons based on item type
            InventorySlot slot = inventorySystem?.GetSlot(slotIndex);
            if (slot?.item != null)
            {
                if (useButton != null)
                {
                    useButton.interactable = slot.item is ConsumableData;
                }
                if (dropButton != null)
                {
                    bool canDrop = !(slot.item is KeyItemData keyItem) || keyItem.canDrop;
                    dropButton.interactable = canDrop;
                }
            }
        }

        private void HideContextMenu()
        {
            if (contextMenu != null)
            {
                contextMenu.SetActive(false);
            }
            contextMenuSlotIndex = -1;
        }

        private void ShowDetails(ItemData item)
        {
            if (detailsPanel == null || item == null) return;

            detailsPanel.SetActive(true);

            if (detailsIcon != null)
            {
                detailsIcon.sprite = item.icon;
                detailsIcon.enabled = item.icon != null;
            }
            if (detailsName != null)
            {
                detailsName.text = item.itemName;
            }
            if (detailsDescription != null)
            {
                detailsDescription.text = item.description;
            }
        }

        private void HideDetails()
        {
            if (detailsPanel != null)
            {
                detailsPanel.SetActive(false);
            }
        }

        private void OnUseClicked()
        {
            if (contextMenuSlotIndex >= 0)
            {
                inventorySystem?.UseItem(contextMenuSlotIndex);
            }
            HideContextMenu();
        }

        private void OnDropClicked()
        {
            if (contextMenuSlotIndex >= 0)
            {
                inventorySystem?.DropItem(contextMenuSlotIndex);
            }
            HideContextMenu();
        }

        private void OnAssignToHotbar(int hotbarIndex)
        {
            if (contextMenuSlotIndex >= 0 && hotbarSystem != null)
            {
                hotbarSystem.AssignSlot(hotbarIndex, contextMenuSlotIndex);
            }
            HideContextMenu();
        }
    }
}
