using UnityEngine;

namespace Streets.Inventory
{
    public class ItemPickup : MonoBehaviour
    {
        [Header("Item Settings")]
        [SerializeField] private ItemData item;
        [SerializeField] private int quantity = 1;

        [Header("Pickup Settings")]
        [SerializeField] private float pickupRange = 2f;
        [SerializeField] private KeyCode pickupKey = KeyCode.E;
        [SerializeField] private bool autoPickup = false;

        [Header("Visual Feedback")]
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobHeight = 0.2f;
        [SerializeField] private float rotateSpeed = 50f;

        private Vector3 startPosition;
        private InventorySystem playerInventory;
        private bool playerInRange;

        private void Start()
        {
            startPosition = transform.position;
        }

        private void Update()
        {
            AnimatePickup();

            if (playerInRange && !autoPickup && UnityEngine.Input.GetKeyDown(pickupKey))
            {
                TryPickup();
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
            Debug.Log($"ItemPickup: Something entered trigger: {other.name}, Tag: {other.tag}");

            if (!other.CompareTag("Player"))
            {
                Debug.Log("ItemPickup: Not tagged as Player, ignoring");
                return;
            }

            playerInventory = other.GetComponent<InventorySystem>();
            if (playerInventory == null)
            {
                playerInventory = other.GetComponentInParent<InventorySystem>();
            }

            if (playerInventory != null)
            {
                Debug.Log("ItemPickup: Player in range, inventory found!");
                playerInRange = true;

                if (autoPickup)
                {
                    TryPickup();
                }
            }
            else
            {
                Debug.LogWarning("ItemPickup: Player found but no InventorySystem component!");
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
            if (playerInventory == null || item == null) return;

            if (playerInventory.AddItem(item, quantity))
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
