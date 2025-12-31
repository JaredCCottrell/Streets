using UnityEngine;
using UnityEngine.Events;
using Streets.Dialogue;
using Streets.Inventory;

namespace Streets.House.NPCs
{
    /// <summary>
    /// Grandpa NPC who wants a beer from the garage fridge.
    /// The player must find the fridge key, get the beer, and bring it to grandpa.
    /// </summary>
    public class GrandpaNPC : MonoBehaviour
    {
        [Header("Dialogue")]
        [SerializeField] private DialogueData wantsBeerDialogue;
        [SerializeField] private DialogueData receivingBeerDialogue;
        [SerializeField] private DialogueData alreadyHasBeerDialogue;

        [Header("Interaction")]
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private bool facePlayer = true;

        [Header("Beer Detection")]
        [SerializeField] private string beerItemName = "Cold Beer";

        [Header("Rewards")]
        [SerializeField] private ItemData rewardItem;
        [SerializeField] private Transform rewardDropPoint;

        [Header("Events")]
        [SerializeField] private UnityEvent OnBeerReceived;
        [SerializeField] private UnityEvent OnFirstInteraction;

        [Header("Animations (Optional)")]
        [SerializeField] private Animator animator;
        [SerializeField] private string drinkingAnimTrigger = "Drink";
        [SerializeField] private string happyAnimTrigger = "Happy";

        // State
        private bool hasReceivedBeer = false;
        private bool hasBeenTalkedTo = false;
        private Transform player;
        private HeldItemSystem playerHeldItemSystem;
        private bool playerInRange = false;
        private bool isInDialogue = false;

        // Properties
        public bool HasReceivedBeer => hasReceivedBeer;

        private void Start()
        {
            // Find player
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerHeldItemSystem = playerObj.GetComponent<HeldItemSystem>();
                if (playerHeldItemSystem == null)
                {
                    playerHeldItemSystem = playerObj.GetComponentInChildren<HeldItemSystem>();
                }
            }

            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
        }

        private void Update()
        {
            if (player == null) return;

            // Check range
            float distance = Vector3.Distance(transform.position, player.position);
            bool wasInRange = playerInRange;
            playerInRange = distance <= interactionRange;

            // Face player when in range
            if (playerInRange && facePlayer && !isInDialogue)
            {
                FacePlayer();
            }

            // Handle interaction
            if (playerInRange && !isInDialogue)
            {
                if (UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
                {
                    HandleInteraction();
                }
            }
        }

        private void HandleInteraction()
        {
            // Check if player is holding beer
            if (playerHeldItemSystem != null && playerHeldItemSystem.IsHoldingItem)
            {
                ItemPickup heldItem = playerHeldItemSystem.HeldItem;
                if (heldItem != null && heldItem.Item != null)
                {
                    // Check if it's beer
                    if (heldItem.Item.itemName == beerItemName ||
                        heldItem.Item.name.Contains("Beer"))
                    {
                        ReceiveBeer(heldItem);
                        return;
                    }
                }
            }

            // Not holding beer - just talk
            StartDialogue();
        }

        private void StartDialogue()
        {
            if (DialogueManager.Instance == null)
            {
                // Fallback if no dialogue manager
                Debug.Log(GetFallbackDialogue());
                return;
            }

            DialogueData dialogue = GetCurrentDialogue();
            if (dialogue != null)
            {
                isInDialogue = true;

                if (!hasBeenTalkedTo)
                {
                    hasBeenTalkedTo = true;
                    OnFirstInteraction?.Invoke();
                }

                DialogueManager.Instance.StartDialogue(dialogue, OnDialogueComplete);
            }
        }

        private DialogueData GetCurrentDialogue()
        {
            if (hasReceivedBeer)
            {
                return alreadyHasBeerDialogue;
            }
            return wantsBeerDialogue;
        }

        private string GetFallbackDialogue()
        {
            if (hasReceivedBeer)
            {
                return "Grandpa: *sips beer* Ahh, that hits the spot. Thanks, kiddo.";
            }
            return "Grandpa: Hey there! Could you grab me a beer from the fridge in the garage? I think I left the key somewhere in the bedroom...";
        }

        private void ReceiveBeer(ItemPickup beerItem)
        {
            if (hasReceivedBeer) return;

            hasReceivedBeer = true;

            // Take the beer from player
            Destroy(beerItem.gameObject);

            // Play receiving dialogue
            if (DialogueManager.Instance != null && receivingBeerDialogue != null)
            {
                isInDialogue = true;
                DialogueManager.Instance.StartDialogue(receivingBeerDialogue, OnBeerDialogueComplete);
            }
            else
            {
                Debug.Log("Grandpa: Oh! A cold one! You're a lifesaver, kiddo!");
                OnBeerDialogueComplete(false);
            }

            // Play animation
            if (animator != null)
            {
                animator.SetTrigger(happyAnimTrigger);
            }

            OnBeerReceived?.Invoke();
        }

        private void OnDialogueComplete(bool skipped)
        {
            isInDialogue = false;
        }

        private void OnBeerDialogueComplete(bool skipped)
        {
            isInDialogue = false;

            // Play drinking animation
            if (animator != null)
            {
                animator.SetTrigger(drinkingAnimTrigger);
            }

            // Give reward if any
            if (rewardItem != null)
            {
                GiveReward();
            }
        }

        private void GiveReward()
        {
            if (rewardItem == null) return;

            // Try to find item pickup prefab
            var pickupPrefab = Resources.Load<ItemPickup>("Prefabs/ItemPickup");

            Vector3 dropPos = rewardDropPoint != null ?
                rewardDropPoint.position :
                transform.position + transform.forward * 0.5f + Vector3.up * 0.5f;

            if (pickupPrefab != null)
            {
                ItemPickup pickup = Instantiate(pickupPrefab, dropPos, Quaternion.identity);
                pickup.Initialize(rewardItem, 1);
            }
            else
            {
                // Create a simple pickup object
                GameObject rewardObj = new GameObject($"Reward_{rewardItem.itemName}");
                rewardObj.transform.position = dropPos;

                var rb = rewardObj.AddComponent<Rigidbody>();
                var col = rewardObj.AddComponent<BoxCollider>();
                col.size = Vector3.one * 0.3f;

                var pickup = rewardObj.AddComponent<ItemPickup>();
                pickup.Initialize(rewardItem, 1);
            }

            Debug.Log($"[GrandpaNPC] Gave reward: {rewardItem.itemName}");
        }

        private void FacePlayer()
        {
            if (player == null) return;

            Vector3 lookDir = player.position - transform.position;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
            }
        }

        private void OnGUI()
        {
            if (!playerInRange || isInDialogue) return;

            // Show interaction prompt
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 16;
            style.normal.textColor = Color.white;

            float centerX = Screen.width / 2f;
            float promptY = Screen.height * 0.55f;

            string prompt;
            if (playerHeldItemSystem != null && playerHeldItemSystem.IsHoldingItem)
            {
                var heldItem = playerHeldItemSystem.HeldItem;
                if (heldItem?.Item?.itemName == beerItemName ||
                    (heldItem?.Item?.name.Contains("Beer") ?? false))
                {
                    prompt = "[E] Give beer to Grandpa";
                }
                else
                {
                    prompt = "[E] Talk to Grandpa";
                }
            }
            else
            {
                prompt = "[E] Talk to Grandpa";
            }

            GUI.Label(new Rect(centerX - 100, promptY, 200, 30), prompt, style);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}
