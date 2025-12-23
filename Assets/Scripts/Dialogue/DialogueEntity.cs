using UnityEngine;
using UnityEngine.InputSystem;

namespace Streets.Dialogue
{
    /// <summary>
    /// An entity that can be interacted with to start dialogue.
    /// Attach to NPCs, objects, or event-spawned creatures.
    /// </summary>
    public class DialogueEntity : MonoBehaviour
    {
        [Header("Dialogue")]
        [SerializeField] private DialogueData dialogueData;

        [Header("Interaction")]
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private Key interactKey = Key.E;
        [SerializeField] private bool autoStartOnRange = false;

        [Header("Behavior")]
        [Tooltip("Look at player during dialogue")]
        [SerializeField] private bool facePlayerDuringDialogue = true;

        [Tooltip("Destroy entity after dialogue completes")]
        [SerializeField] private bool destroyAfterDialogue = false;

        [Tooltip("Delay before destroying after dialogue")]
        [SerializeField] private float destroyDelay = 0f;

        [Header("UI Prompt")]
        [SerializeField] private GameObject interactPrompt;
        [SerializeField] private string promptText = "Press E to interact";

        // State
        private Transform player;
        private bool playerInRange;
        private bool hasInteracted;
        private bool dialogueComplete;

        // Properties
        public bool IsInRange => playerInRange;
        public bool HasDialogue => dialogueData != null;

        private void Start()
        {
            // Find player
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning($"[DialogueEntity] No GameObject with 'Player' tag found! Make sure your player has the Player tag.");
            }

            // Check for DialogueManager
            if (DialogueManager.Instance == null)
            {
                Debug.LogWarning("[DialogueEntity] No DialogueManager found in scene!");
            }

            // Check for dialogue data
            if (dialogueData == null)
            {
                Debug.LogWarning($"[DialogueEntity] No DialogueData assigned to {gameObject.name}!");
            }

            // Hide prompt initially
            if (interactPrompt != null)
                interactPrompt.SetActive(false);
        }

        private void Update()
        {
            if (player == null || dialogueComplete) return;

            // Check range
            float distance = Vector3.Distance(transform.position, player.position);
            bool wasInRange = playerInRange;
            playerInRange = distance <= interactionRange;

            // Range state changed
            if (playerInRange != wasInRange)
            {
                OnRangeStateChanged(playerInRange);
            }

            // Face player during dialogue
            if (facePlayerDuringDialogue && DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
            {
                FacePlayer();
            }

            // Check for interaction input
            if (Keyboard.current != null && Keyboard.current[interactKey].wasPressedThisFrame)
            {
                Debug.Log($"[DialogueEntity] E pressed. InRange={playerInRange}, HasInteracted={hasInteracted}, Distance={Vector3.Distance(transform.position, player.position):F1}");
            }

            if (playerInRange && !hasInteracted)
            {
                if (autoStartOnRange)
                {
                    StartInteraction();
                }
                else if (Keyboard.current != null && Keyboard.current[interactKey].wasPressedThisFrame)
                {
                    StartInteraction();
                }
            }
        }

        private void OnRangeStateChanged(bool inRange)
        {
            if (inRange && !hasInteracted)
            {
                ShowPrompt();
            }
            else
            {
                HidePrompt();
            }
        }

        private void ShowPrompt()
        {
            if (interactPrompt != null)
                interactPrompt.SetActive(true);
        }

        private void HidePrompt()
        {
            if (interactPrompt != null)
                interactPrompt.SetActive(false);
        }

        private void StartInteraction()
        {
            if (dialogueData == null)
            {
                Debug.LogWarning($"[DialogueEntity] No dialogue data assigned to {gameObject.name}");
                return;
            }

            if (DialogueManager.Instance == null)
            {
                Debug.LogError("[DialogueEntity] DialogueManager not found in scene!");
                return;
            }

            hasInteracted = true;
            HidePrompt();

            // Face player before starting
            if (facePlayerDuringDialogue)
            {
                FacePlayer();
            }

            DialogueManager.Instance.StartDialogue(dialogueData, this);
        }

        private void FacePlayer()
        {
            if (player == null) return;

            Vector3 lookDir = player.position - transform.position;
            lookDir.y = 0; // Keep upright
            if (lookDir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDir);
            }
        }

        /// <summary>
        /// Called by DialogueManager when dialogue ends
        /// </summary>
        public void OnDialogueComplete(bool skipped)
        {
            dialogueComplete = true;

            if (destroyAfterDialogue)
            {
                if (destroyDelay > 0)
                {
                    Destroy(gameObject, destroyDelay);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }

        /// <summary>
        /// Set dialogue at runtime (for event-spawned entities)
        /// </summary>
        public void SetDialogue(DialogueData dialogue)
        {
            dialogueData = dialogue;
        }

        /// <summary>
        /// Force start dialogue (bypasses range check)
        /// </summary>
        public void ForceStartDialogue()
        {
            if (!hasInteracted && dialogueData != null)
            {
                StartInteraction();
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw interaction range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}
