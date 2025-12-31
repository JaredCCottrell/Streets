using UnityEngine;

namespace Streets.Inventory
{
    /// <summary>
    /// A pickup-able item in the world. Has gravity and can be picked up by the player.
    /// When picked up, the item is held in front of the player until stored or used.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class ItemPickup : MonoBehaviour
    {
        [Header("Item Settings")]
        [SerializeField] private ItemData item;
        [SerializeField] private int quantity = 1;

        [Header("Physics")]
        [SerializeField] private float mass = 1f;
        [SerializeField] private float drag = 1f;
        [SerializeField] private float angularDrag = 2f;

        [Header("Interaction")]
        [SerializeField] private float interactionRange = 2f;

        private Rigidbody rb;
        private Collider col;
        private bool isBeingHeld = false;

        public ItemData Item => item;
        public int Quantity => quantity;
        public bool IsBeingHeld => isBeingHeld;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();

            SetupPhysics();
        }

        private void SetupPhysics()
        {
            if (rb != null)
            {
                rb.mass = mass;
                rb.linearDamping = drag;
                rb.angularDamping = angularDrag;
                rb.useGravity = true;
                rb.isKinematic = false;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }

        /// <summary>
        /// Initialize this pickup with item data (used when dropping items)
        /// </summary>
        public void Initialize(ItemData itemData, int itemQuantity)
        {
            item = itemData;
            quantity = itemQuantity;
            SetupPhysics();
        }

        /// <summary>
        /// Called when player picks up this item to hold it
        /// </summary>
        public void OnPickedUp()
        {
            isBeingHeld = true;

            // Disable physics while held
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            // Disable collider to prevent clipping issues
            if (col != null)
            {
                col.enabled = false;
            }
        }

        /// <summary>
        /// Called when player drops this item
        /// </summary>
        public void OnDropped(Vector3 force)
        {
            isBeingHeld = false;

            // Re-enable physics
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.AddForce(force, ForceMode.Impulse);
            }

            // Re-enable collider
            if (col != null)
            {
                col.enabled = true;
            }
        }

        /// <summary>
        /// Called when the item is stored in inventory
        /// </summary>
        public void OnStored()
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// Called when the item is used while held
        /// </summary>
        public bool TryUse()
        {
            // Only consumables can be used directly
            if (item is ConsumableData)
            {
                return true; // HeldItemSystem will handle the actual use
            }
            return false;
        }

        /// <summary>
        /// Check if this item can be used (consumed)
        /// </summary>
        public bool CanBeUsed()
        {
            return item is ConsumableData;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}
