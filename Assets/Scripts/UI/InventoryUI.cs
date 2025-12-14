using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
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
        [SerializeField] private Button useButton;
        [SerializeField] private Button dropButton;
        [SerializeField] private Button[] hotbarAssignButtons;

        // State
        private InventorySlotUI[] slotUIs;
        private int selectedSlotIndex = -1;
        private int contextMenuSlotIndex = -1;
        private bool isOpen;

        public bool IsOpen => isOpen;

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
        }

        private void Start()
        {
            CreateSlotUIs();
            SetupContextMenu();
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

        private void SetupContextMenu()
        {
            if (useButton != null)
            {
                useButton.onClick.AddListener(OnUseClicked);
            }
            if (dropButton != null)
            {
                dropButton.onClick.AddListener(OnDropClicked);
            }

            for (int i = 0; i < hotbarAssignButtons.Length; i++)
            {
                int index = i;
                if (hotbarAssignButtons[i] != null)
                {
                    hotbarAssignButtons[i].onClick.AddListener(() => OnAssignToHotbar(index));
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

            if (contextMenu != null)
            {
                contextMenu.SetActive(true);
                contextMenu.transform.position = Mouse.current.position.ReadValue();
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
