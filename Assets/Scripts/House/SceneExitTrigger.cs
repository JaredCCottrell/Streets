using UnityEngine;
using UnityEngine.Events;
using Streets.Core;
using Streets.Inventory;

namespace Streets.House
{
    /// <summary>
    /// Trigger zone that transitions to another scene.
    /// Placed at the end of the cul-de-sac to start the road section.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SceneExitTrigger : MonoBehaviour
    {
        [Header("Scene Settings")]
        [SerializeField] private string targetScene = "RoadScene";
        [SerializeField] private bool useSceneIndex = false;
        [SerializeField] private int targetSceneIndex = 1;

        [Header("Requirements")]
        [SerializeField] private bool requireAllPuzzlesSolved = false;
        [SerializeField] private HouseProgressTracker progressTracker;
        [SerializeField] private string[] requiredKeyIds;

        [Header("Messages")]
        [SerializeField] private string blockedMessage = "I should explore the house first.";
        [SerializeField] private string exitPrompt = "Leave the neighborhood?";
        [SerializeField] private bool showConfirmation = true;

        [Header("State Saving")]
        [SerializeField] private bool savePlayerState = true;
        [SerializeField] private bool markHouseComplete = true;

        [Header("Events")]
        [SerializeField] private UnityEvent OnExitBlocked;
        [SerializeField] private UnityEvent OnExitTriggered;

        // References
        private InventorySystem inventorySystem;
        private float playerHealth = 100f;
        private float playerMaxHealth = 100f;

        private bool playerInTrigger = false;

        private void Start()
        {
            // Ensure collider is trigger
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            // Find progress tracker if not assigned
            if (progressTracker == null)
            {
                progressTracker = FindObjectOfType<HouseProgressTracker>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            playerInTrigger = true;

            // Cache player references
            CachePlayerReferences(other.gameObject);

            if (CanExit())
            {
                if (showConfirmation)
                {
                    ShowConfirmation();
                }
                else
                {
                    TriggerExit();
                }
            }
            else
            {
                ShowBlockedMessage();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            playerInTrigger = false;
        }

        private void CachePlayerReferences(GameObject player)
        {
            inventorySystem = player.GetComponentInChildren<InventorySystem>();
            if (inventorySystem == null)
            {
                inventorySystem = FindObjectOfType<InventorySystem>();
            }

            // Get current health value if available
            var healthSystem = player.GetComponentInChildren<Streets.Survival.HealthSystem>();
            if (healthSystem != null)
            {
                playerHealth = healthSystem.CurrentHealth;
                playerMaxHealth = healthSystem.MaxHealth;
            }
        }

        private bool CanExit()
        {
            // Check progress tracker
            if (requireAllPuzzlesSolved && progressTracker != null)
            {
                if (!progressTracker.AreAllPuzzlesComplete())
                {
                    return false;
                }
            }

            // Check required keys
            if (requiredKeyIds != null && requiredKeyIds.Length > 0)
            {
                if (inventorySystem == null) return false;

                foreach (var keyId in requiredKeyIds)
                {
                    if (!string.IsNullOrEmpty(keyId) && !inventorySystem.HasKeyItem(keyId))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void ShowBlockedMessage()
        {
            Debug.Log($"[SceneExitTrigger] {blockedMessage}");
            OnExitBlocked?.Invoke();

            // Could show UI message here
            // DialogueManager.Instance?.ShowMessage(blockedMessage);
        }

        private void ShowConfirmation()
        {
            // For now, just trigger exit
            // In a full implementation, this would show a confirmation dialog
            Debug.Log($"[SceneExitTrigger] {exitPrompt}");
            TriggerExit();
        }

        private void TriggerExit()
        {
            Debug.Log("[SceneExitTrigger] Triggering scene transition...");

            OnExitTriggered?.Invoke();

            // Save player state
            if (savePlayerState && GameStateManager.Instance != null)
            {
                GameStateManager.Instance.SaveInventory(inventorySystem);
                GameStateManager.Instance.SavePlayerStats(playerHealth, playerMaxHealth);

                if (markHouseComplete)
                {
                    GameStateManager.Instance.MarkHouseIntroComplete();
                }
            }

            // Transition to target scene
            if (SceneTransitionManager.Instance != null)
            {
                if (useSceneIndex)
                {
                    SceneTransitionManager.Instance.LoadScene(targetSceneIndex);
                }
                else
                {
                    SceneTransitionManager.Instance.LoadScene(targetScene);
                }
            }
            else
            {
                // Fallback to direct scene load
                Debug.LogWarning("[SceneExitTrigger] No SceneTransitionManager found, using direct load");
                UnityEngine.SceneManagement.SceneManager.LoadScene(useSceneIndex ? targetSceneIndex : GetSceneIndex(targetScene));
            }
        }

        private int GetSceneIndex(string sceneName)
        {
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (name == sceneName) return i;
            }
            return 0;
        }

        /// <summary>
        /// Force trigger the exit (for debugging)
        /// </summary>
        public void ForceExit()
        {
            TriggerExit();
        }

        private void OnDrawGizmos()
        {
            // Draw trigger area
            Gizmos.color = new Color(0, 1, 0, 0.3f);

            var collider = GetComponent<Collider>();
            if (collider is BoxCollider box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (collider is SphereCollider sphere)
            {
                Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius);
                Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius);
            }

            // Draw exit label
            Gizmos.color = Color.green;
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2, $"EXIT → {targetScene}");
#endif
        }
    }
}
