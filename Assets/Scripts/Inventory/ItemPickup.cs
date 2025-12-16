using UnityEngine;
using UnityEngine.InputSystem;

namespace Streets.Inventory
{
    public class ItemPickup : MonoBehaviour
    {
        [Header("Item Settings")]
        [SerializeField] private ItemData item;
        [SerializeField] private int quantity = 1;

        [Header("Pickup Settings")]
        [SerializeField] private float pickupRange = 2f;
        [SerializeField] private Key pickupKey = Key.E;
        [SerializeField] private bool autoPickup = false;
        [SerializeField] private bool canPickupWhileMoving = true;

        [Header("Physics (for dropped items)")]
        [SerializeField] private float drag = 2f;
        [SerializeField] private float angularDrag = 2f;

        [Header("Visual Feedback")]
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobHeight = 0.2f;
        [SerializeField] private float rotateSpeed = 50f;

        private Vector3 startPosition;
        private InventorySystem playerInventory;
        private bool playerInRange;
        private Rigidbody rb;
        private bool hasSettled;
        private float settleCheckDelay = 0.5f;
        private float settleTimer;

        public ItemData Item => item;
        public int Quantity => quantity;

        private void Start()
        {
            startPosition = transform.position;
            rb = GetComponent<Rigidbody>();

            // If no rigidbody, we're already settled (placed in editor)
            if (rb == null)
            {
                hasSettled = true;
            }
        }

        /// <summary>
        /// Initialize this pickup with item data (used when dropping items)
        /// </summary>
        public void Initialize(ItemData itemData, int itemQuantity)
        {
            item = itemData;
            quantity = itemQuantity;
            startPosition = transform.position;
            pickupKey = Key.E;

            // Apply drag settings to help item settle faster
            if (rb == null) rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearDamping = drag;
                rb.angularDamping = angularDrag;
            }
        }

        private void Update()
        {
            // Debug: check if E is pressed at all
            if (Keyboard.current[pickupKey].wasPressedThisFrame)
            {
                Debug.Log($"[ItemPickup] E pressed on {gameObject.name}. playerInRange={playerInRange}, item={item?.itemName ?? "NULL"}");
            }

            // Check if dropped item has settled
            if (!hasSettled)
            {
                CheckIfSettled();
            }
            else
            {
                AnimatePickup();
            }

            bool canPickup = hasSettled || canPickupWhileMoving;
            if (playerInRange && canPickup && !autoPickup && Keyboard.current[pickupKey].wasPressedThisFrame)
            {
                Debug.Log($"[ItemPickup] Attempting pickup!");
                TryPickup();
            }
        }

        private void CheckIfSettled()
        {
            if (rb == null)
            {
                hasSettled = true;
                return;
            }

            settleTimer += Time.deltaTime;

            // Wait a bit before checking velocity
            if (settleTimer < settleCheckDelay) return;

            // Check if nearly stopped
            if (rb.linearVelocity.magnitude < 0.1f && rb.angularVelocity.magnitude < 0.1f)
            {
                // Item has settled - disable physics and start animating
                hasSettled = true;
                startPosition = transform.position;

                // Make kinematic so animation works
                rb.isKinematic = true;
            }
        }

        private void AnimatePickup()
        {
            // Bob up and down
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);

            // Rotate
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[ItemPickup] Trigger entered by: {other.name}, Tag: {other.tag}");

            if (!other.CompareTag("Player"))
            {
                return;
            }

            playerInventory = other.GetComponent<InventorySystem>();
            if (playerInventory == null)
            {
                playerInventory = other.GetComponentInParent<InventorySystem>();
            }

            Debug.Log($"[ItemPickup] Player detected. InventorySystem found: {playerInventory != null}");

            if (playerInventory != null)
            {
                playerInRange = true;

                if (autoPickup && (hasSettled || canPickupWhileMoving))
                {
                    TryPickup();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
                playerInventory = null;
            }
        }

        private void TryPickup()
        {
            Debug.Log($"[ItemPickup] TryPickup called. playerInventory={playerInventory != null}, item={item?.itemName ?? "null"}, quantity={quantity}");

            if (playerInventory == null || item == null) return;

            bool added = playerInventory.AddItem(item, quantity);
            Debug.Log($"[ItemPickup] AddItem result: {added}");

            if (added)
            {
                Destroy(gameObject);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, pickupRange);
        }
    }
}
