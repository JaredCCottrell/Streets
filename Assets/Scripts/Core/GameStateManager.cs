using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Streets.Inventory;

namespace Streets.Core
{
    /// <summary>
    /// Manages game state persistence across scenes.
    /// Uses DontDestroyOnLoad to survive scene transitions.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;

        // Serializable state classes
        [Serializable]
        public class SerializedItem
        {
            public string itemName; // ItemData asset name for lookup
            public int quantity;
        }

        [Serializable]
        public class InventoryState
        {
            public List<SerializedItem> items = new List<SerializedItem>();
        }

        [Serializable]
        public class PlayerState
        {
            public float health = 100f;
            public float maxHealth = 100f;
            public float stamina = 100f;
            public float intoxication = 0f;
        }

        [Serializable]
        public class GameState
        {
            public InventoryState inventory = new InventoryState();
            public PlayerState player = new PlayerState();
            public bool completedHouseIntro = false;
            public string lastScene = "";
        }

        // Current state
        private GameState currentState = new GameState();

        // Item database for serialization/deserialization
        private Dictionary<string, ItemData> itemDatabase;

        // Events
        public event Action OnStateLoaded;
        public event Action OnStateSaved;

        // Properties
        public bool CompletedHouseIntro => currentState.completedHouseIntro;
        public GameState CurrentState => currentState;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            BuildItemDatabase();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void BuildItemDatabase()
        {
            itemDatabase = new Dictionary<string, ItemData>();

            // Load all ItemData assets from Resources
            ItemData[] allItems = Resources.LoadAll<ItemData>("Items");
            foreach (var item in allItems)
            {
                if (!itemDatabase.ContainsKey(item.name))
                {
                    itemDatabase[item.name] = item;
                }
            }

            if (debugMode)
            {
                Debug.Log($"[GameStateManager] Built item database with {itemDatabase.Count} items");
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (debugMode)
            {
                Debug.Log($"[GameStateManager] Scene loaded: {scene.name}");
            }

            currentState.lastScene = scene.name;
        }

        #region Inventory State

        public void SaveInventory(InventorySystem inventory)
        {
            if (inventory == null) return;

            currentState.inventory.items.Clear();

            foreach (var slot in inventory.Slots)
            {
                if (!slot.IsEmpty && slot.item != null)
                {
                    currentState.inventory.items.Add(new SerializedItem
                    {
                        itemName = slot.item.name,
                        quantity = slot.quantity
                    });
                }
            }

            if (debugMode)
            {
                Debug.Log($"[GameStateManager] Saved inventory: {currentState.inventory.items.Count} items");
            }
        }

        public void RestoreInventory(InventorySystem inventory)
        {
            if (inventory == null) return;

            inventory.Clear();

            foreach (var serializedItem in currentState.inventory.items)
            {
                if (itemDatabase.TryGetValue(serializedItem.itemName, out ItemData itemData))
                {
                    inventory.AddItem(itemData, serializedItem.quantity);
                }
                else
                {
                    Debug.LogWarning($"[GameStateManager] Could not find item: {serializedItem.itemName}");
                }
            }

            if (debugMode)
            {
                Debug.Log($"[GameStateManager] Restored inventory: {currentState.inventory.items.Count} items");
            }
        }

        #endregion

        #region Player Stats State

        public void SavePlayerStats(float health, float maxHealth, float intoxication = 0f)
        {
            currentState.player.health = health;
            currentState.player.maxHealth = maxHealth;
            currentState.player.intoxication = intoxication;

            if (debugMode)
            {
                Debug.Log($"[GameStateManager] Saved player stats - HP:{currentState.player.health}");
            }

            OnStateSaved?.Invoke();
        }

        public PlayerState GetPlayerState()
        {
            return currentState.player;
        }

        public void RestorePlayerStats(System.Action<PlayerState> applyState)
        {
            applyState?.Invoke(currentState.player);

            if (debugMode)
            {
                Debug.Log($"[GameStateManager] Restored player stats");
            }

            OnStateLoaded?.Invoke();
        }

        #endregion

        #region Game Progress

        public void MarkHouseIntroComplete()
        {
            currentState.completedHouseIntro = true;
            if (debugMode)
            {
                Debug.Log("[GameStateManager] House intro marked complete");
            }
        }

        public void ResetProgress()
        {
            currentState = new GameState();
            if (debugMode)
            {
                Debug.Log("[GameStateManager] Progress reset");
            }
        }

        #endregion

        #region Save/Load to Disk (Optional Future Enhancement)

        public void SaveToPlayerPrefs()
        {
            string json = JsonUtility.ToJson(currentState);
            PlayerPrefs.SetString("GameState", json);
            PlayerPrefs.Save();

            if (debugMode)
            {
                Debug.Log("[GameStateManager] Saved to PlayerPrefs");
            }
        }

        public void LoadFromPlayerPrefs()
        {
            if (PlayerPrefs.HasKey("GameState"))
            {
                string json = PlayerPrefs.GetString("GameState");
                currentState = JsonUtility.FromJson<GameState>(json);

                if (debugMode)
                {
                    Debug.Log("[GameStateManager] Loaded from PlayerPrefs");
                }
            }
        }

        public void ClearSavedData()
        {
            PlayerPrefs.DeleteKey("GameState");
            PlayerPrefs.Save();
            currentState = new GameState();

            if (debugMode)
            {
                Debug.Log("[GameStateManager] Cleared saved data");
            }
        }

        #endregion

        /// <summary>
        /// Register an item for serialization lookup
        /// </summary>
        public void RegisterItem(ItemData item)
        {
            if (item != null && !itemDatabase.ContainsKey(item.name))
            {
                itemDatabase[item.name] = item;
            }
        }
    }
}
