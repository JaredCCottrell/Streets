using UnityEngine;
using UnityEngine.InputSystem;
using Streets.Inventory;

namespace Streets.House
{
    /// <summary>
    /// Base class for all interactable objects in the house.
    /// Provides range-based interaction with E key, prompts, and item requirements.
    /// </summary>
    public abstract class Interactable : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] protected float interactionRange = 2f;
        [SerializeField] protected Key interactKey = Key.E;
        [SerializeField] protected bool singleUse = false;

        [Header("Item Requirement")]
        [SerializeField] protected bool requiresItem = false;
        [SerializeField] protected string requiredKeyId;
        [SerializeField] protected bool consumeItemOnUse = false;

        [Header("UI Prompt")]
        [SerializeField] protected GameObject interactPrompt;
        [SerializeField] protected string promptText = "Press E to interact";
        [SerializeField] protected string lockedPromptText = "Locked";

        [Header("Audio")]
        [SerializeField] protected AudioClip interactSound;
        [SerializeField] protected AudioClip failSound;
        [SerializeField] protected AudioSource audioSource;

        // State
        protected Transform player;
        protected InventorySystem inventorySystem;
        protected bool playerInRange;
        protected bool hasBeenUsed;
        protected bool isEnabled = true;

        // Properties
        public bool IsInRange => playerInRange;
        public bool HasBeenUsed => hasBeenUsed;
        public bool IsEnabled => isEnabled;
        public string PromptText => GetDynamicPrompt();

        protected virtual void Start()
        {
            // Find player
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                inventorySystem = playerObj.GetComponentInChildren<InventorySystem>();
                if (inventorySystem == null)
                {
                    inventorySystem = FindObjectOfType<InventorySystem>();
                }
            }
            else
            {
                Debug.LogWarning($"[Interactable] No Player found for {gameObject.name}");
            }

            // Create audio source if needed
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null && (interactSound != null || failSound != null))
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 1f;
                }
            }

            // Hide prompt initially
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }
        }

        protected virtual void Update()
        {
            if (player == null || !isEnabled) return;
            if (singleUse && hasBeenUsed) return;

            // Check range
            float distance = Vector3.Distance(transform.position, player.position);
            bool wasInRange = playerInRange;
            playerInRange = distance <= interactionRange;

            // Range state changed
            if (playerInRange != wasInRange)
            {
                OnRangeStateChanged(playerInRange);
            }

            // Check for interaction input
            if (playerInRange && Keyboard.current != null && Keyboard.current[interactKey].wasPressedThisFrame)
            {
                TryInteract();
            }
        }

        protected virtual void OnRangeStateChanged(bool inRange)
        {
            if (inRange)
            {
                ShowPrompt();
            }
            else
            {
                HidePrompt();
            }
        }

        protected virtual void ShowPrompt()
        {
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(true);
                // Could update prompt text here if using TextMeshPro
            }
        }

        protected virtual void HidePrompt()
        {
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }
        }

        protected virtual string GetDynamicPrompt()
        {
            if (requiresItem && !HasRequiredItem())
            {
                return lockedPromptText;
            }
            return promptText;
        }

        protected void TryInteract()
        {
            if (!CanInteract())
            {
                OnInteractFailed();
                return;
            }

            // Consume item if required
            if (requiresItem && consumeItemOnUse && inventorySystem != null)
            {
                ConsumeRequiredItem();
            }

            // Play sound
            PlaySound(interactSound);

            // Mark as used if single use
            if (singleUse)
            {
                hasBeenUsed = true;
                HidePrompt();
            }

            // Execute interaction
            OnInteract();
        }

        /// <summary>
        /// Override to implement specific interaction behavior
        /// </summary>
        protected abstract void OnInteract();

        /// <summary>
        /// Override to add custom conditions for interaction
        /// </summary>
        protected virtual bool CanInteract()
        {
            if (!isEnabled) return false;
            if (singleUse && hasBeenUsed) return false;

            if (requiresItem && !HasRequiredItem())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Called when interaction fails (e.g., missing required item)
        /// </summary>
        protected virtual void OnInteractFailed()
        {
            PlaySound(failSound);
        }

        protected bool HasRequiredItem()
        {
            if (string.IsNullOrEmpty(requiredKeyId)) return true;
            if (inventorySystem == null) return false;

            return inventorySystem.HasKeyItem(requiredKeyId);
        }

        protected void ConsumeRequiredItem()
        {
            if (inventorySystem == null || string.IsNullOrEmpty(requiredKeyId)) return;

            // Find and remove the key item
            foreach (var slot in inventorySystem.Slots)
            {
                if (slot.item is KeyItemData keyItem && keyItem.keyId == requiredKeyId)
                {
                    inventorySystem.RemoveItem(slot.item, 1);
                    break;
                }
            }
        }

        protected void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// Enable or disable this interactable
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
            if (!enabled)
            {
                HidePrompt();
            }
        }

        /// <summary>
        /// Reset interaction state (for re-usable objects)
        /// </summary>
        public virtual void ResetInteraction()
        {
            hasBeenUsed = false;
        }

        protected virtual void OnDrawGizmosSelected()
        {
            // Draw interaction range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}
