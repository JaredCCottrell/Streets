using UnityEngine;
using UnityEngine.InputSystem;
using Streets.Effects;

namespace Streets.Inventory
{
    /// <summary>
    /// Manages picking up, holding, storing, and using items.
    /// E = Pick up item / Use held item (if usable)
    /// R = Store held item in inventory
    /// G = Drop held item
    /// </summary>
    public class HeldItemSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Transform holdPoint;
        [SerializeField] private InventorySystem inventorySystem;

        [Header("Pickup Settings")]
        [SerializeField] private float pickupRange = 2.5f;
        [SerializeField] private LayerMask pickupLayerMask = -1;

        [Header("Hold Settings")]
        [SerializeField] private float holdDistance = 0.5f;
        [SerializeField] private float holdHeight = -0.2f;
        [SerializeField] private float holdSmoothSpeed = 15f;
        [SerializeField] private Vector3 holdRotationOffset = new Vector3(0, 0, 0);

        [Header("Drop Settings")]
        [SerializeField] private float dropForce = 3f;

        [Header("UI Prompts")]
        [SerializeField] private bool showPrompts = true;

        // State
        private ItemPickup heldItem;
        private ItemPickup lookingAtItem;
        private bool isHoldingItem => heldItem != null;

        // Events
        public System.Action<ItemPickup> OnItemPickedUp;
        public System.Action<ItemPickup> OnItemDropped;
        public System.Action<ItemPickup> OnItemStored;
        public System.Action<ItemPickup> OnItemUsed;

        // Properties
        public bool IsHoldingItem => isHoldingItem;
        public ItemPickup HeldItem => heldItem;
        public ItemPickup LookingAtItem => lookingAtItem;

        private void Start()
        {
            // Find camera if not assigned
            if (cameraTransform == null)
            {
                cameraTransform = Camera.main?.transform;
            }

            // Create hold point if not assigned
            if (holdPoint == null && cameraTransform != null)
            {
                GameObject holdPointObj = new GameObject("HoldPoint");
                holdPoint = holdPointObj.transform;
                holdPoint.SetParent(cameraTransform);
                holdPoint.localPosition = new Vector3(0, holdHeight, holdDistance);
            }

            // Find inventory if not assigned
            if (inventorySystem == null)
            {
                inventorySystem = GetComponentInChildren<InventorySystem>();
                if (inventorySystem == null)
                {
                    inventorySystem = FindObjectOfType<InventorySystem>();
                }
            }
        }

        private void Update()
        {
            // Check what we're looking at
            UpdateLookTarget();

            // Handle input
            HandleInput();

            // Update held item position
            UpdateHeldItemPosition();
        }

        private void UpdateLookTarget()
        {
            lookingAtItem = null;

            if (cameraTransform == null) return;
            if (isHoldingItem) return; // Don't look for new items while holding one

            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickupLayerMask))
            {
                ItemPickup pickup = hit.collider.GetComponent<ItemPickup>();
                if (pickup == null)
                {
                    pickup = hit.collider.GetComponentInParent<ItemPickup>();
                }

                if (pickup != null && !pickup.IsBeingHeld)
                {
                    lookingAtItem = pickup;
                }
            }
        }

        private void HandleInput()
        {
            if (Keyboard.current == null) return;

            // E key - Pick up or Use
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                if (isHoldingItem)
                {
                    // Try to use held item
                    TryUseHeldItem();
                }
                else if (lookingAtItem != null)
                {
                    // Pick up the item we're looking at
                    PickUpItem(lookingAtItem);
                }
            }

            // R key - Store in inventory
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                if (isHoldingItem)
                {
                    StoreHeldItem();
                }
            }

            // G key - Drop held item
            if (Keyboard.current.gKey.wasPressedThisFrame)
            {
                if (isHoldingItem)
                {
                    DropHeldItem();
                }
            }
        }

        private void PickUpItem(ItemPickup pickup)
        {
            if (pickup == null) return;

            heldItem = pickup;
            heldItem.OnPickedUp();

            // Parent to hold point
            heldItem.transform.SetParent(holdPoint);

            OnItemPickedUp?.Invoke(heldItem);
            Debug.Log($"[HeldItemSystem] Picked up: {heldItem.Item?.itemName}");
        }

        private void UpdateHeldItemPosition()
        {
            if (!isHoldingItem || holdPoint == null) return;

            // Smoothly move to hold position
            heldItem.transform.localPosition = Vector3.Lerp(
                heldItem.transform.localPosition,
                Vector3.zero,
                Time.deltaTime * holdSmoothSpeed
            );

            // Apply rotation offset
            Quaternion targetRotation = Quaternion.Euler(holdRotationOffset);
            heldItem.transform.localRotation = Quaternion.Slerp(
                heldItem.transform.localRotation,
                targetRotation,
                Time.deltaTime * holdSmoothSpeed
            );
        }

        private void TryUseHeldItem()
        {
            if (!isHoldingItem) return;

            if (heldItem.CanBeUsed())
            {
                // Get the consumable data
                ConsumableData consumable = heldItem.Item as ConsumableData;
                if (consumable != null)
                {
                    // Apply effects
                    ApplyConsumableEffects(consumable);

                    OnItemUsed?.Invoke(heldItem);
                    Debug.Log($"[HeldItemSystem] Used: {heldItem.Item?.itemName}");

                    // Reduce quantity or destroy
                    // For now, just destroy (single use items)
                    Destroy(heldItem.gameObject);
                    heldItem = null;
                }
            }
            else
            {
                Debug.Log($"[HeldItemSystem] Cannot use: {heldItem.Item?.itemName}");
            }
        }

        private void ApplyConsumableEffects(ConsumableData consumable)
        {
            // Health effects
            var healthSystem = GetComponentInChildren<Survival.HealthSystem>();
            if (healthSystem == null) healthSystem = FindObjectOfType<Survival.HealthSystem>();

            if (healthSystem != null)
            {
                if (consumable.healthRestore > 0)
                {
                    healthSystem.Heal(consumable.healthRestore);
                }
                if (consumable.healthDamage > 0)
                {
                    healthSystem.TakeDamage(consumable.healthDamage);
                }
            }

            // Intoxication effect
            if (consumable.intoxicationAmount > 0)
            {
                var intoxSystem = FindObjectOfType<IntoxicationSystem>();
                if (intoxSystem != null)
                {
                    intoxSystem.AddIntoxication(consumable.intoxicationAmount);
                }
            }
        }

        private void StoreHeldItem()
        {
            if (!isHoldingItem) return;
            if (inventorySystem == null)
            {
                Debug.LogWarning("[HeldItemSystem] No inventory system found!");
                return;
            }

            bool added = inventorySystem.AddItem(heldItem.Item, heldItem.Quantity);
            if (added)
            {
                OnItemStored?.Invoke(heldItem);
                Debug.Log($"[HeldItemSystem] Stored: {heldItem.Item?.itemName}");

                heldItem.OnStored();
                heldItem = null;
            }
            else
            {
                Debug.Log("[HeldItemSystem] Inventory full!");
            }
        }

        private void DropHeldItem()
        {
            if (!isHoldingItem) return;

            // Unparent
            heldItem.transform.SetParent(null);

            // Calculate drop force
            Vector3 dropDirection = cameraTransform.forward + Vector3.up * 0.3f;
            Vector3 force = dropDirection.normalized * dropForce;

            heldItem.OnDropped(force);

            OnItemDropped?.Invoke(heldItem);
            Debug.Log($"[HeldItemSystem] Dropped: {heldItem.Item?.itemName}");

            heldItem = null;
        }

        /// <summary>
        /// Force drop the held item (called when opening inventory, etc.)
        /// </summary>
        public void ForceDropHeldItem()
        {
            if (isHoldingItem)
            {
                DropHeldItem();
            }
        }

        private void OnGUI()
        {
            if (!showPrompts) return;

            // Show interaction prompts
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 16;
            style.normal.textColor = Color.white;

            float centerX = Screen.width / 2f;
            float promptY = Screen.height * 0.6f;

            if (isHoldingItem)
            {
                string itemName = heldItem.Item?.itemName ?? "Item";
                string prompt = $"Holding: {itemName}\n";

                if (heldItem.CanBeUsed())
                {
                    prompt += "[E] Use  ";
                }
                prompt += "[R] Store  [G] Drop";

                GUI.Label(new Rect(centerX - 150, promptY, 300, 60), prompt, style);
            }
            else if (lookingAtItem != null)
            {
                string itemName = lookingAtItem.Item?.itemName ?? "Item";
                GUI.Label(new Rect(centerX - 100, promptY, 200, 30), $"[E] Pick up {itemName}", style);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (cameraTransform != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(cameraTransform.position, cameraTransform.position + cameraTransform.forward * pickupRange);
            }
        }
    }
}
