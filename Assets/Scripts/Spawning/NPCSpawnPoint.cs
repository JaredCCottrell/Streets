using UnityEngine;

namespace Streets.Spawning
{
    /// <summary>
    /// Add to any GameObject to give it a chance to spawn an NPC at its location.
    /// </summary>
    public class NPCSpawnPoint : MonoBehaviour
    {
        [Header("NPC Pool")]
        [Tooltip("Possible NPCs to spawn (picks one randomly)")]
        [SerializeField] private GameObject[] npcPrefabs;

        [Header("Spawn Settings")]
        [Tooltip("Chance to spawn (0-1). 0.1 = 10% chance")]
        [Range(0f, 1f)]
        [SerializeField] private float spawnChance = 0.1f;

        [Tooltip("When to attempt spawn")]
        [SerializeField] private SpawnTrigger spawnTrigger = SpawnTrigger.OnStart;

        [Tooltip("Offset from this object's position")]
        [SerializeField] private Vector3 spawnOffset = Vector3.zero;

        [Tooltip("Randomize Y rotation")]
        [SerializeField] private bool randomRotation = true;

        [Header("Limits")]
        [Tooltip("Only spawn once ever")]
        [SerializeField] private bool spawnOnce = true;

        [Tooltip("Destroy this spawn point after spawning")]
        [SerializeField] private bool destroyAfterSpawn = false;

        // State
        private bool hasSpawned = false;
        private GameObject spawnedNPC;

        public bool HasSpawned => hasSpawned;
        public GameObject SpawnedNPC => spawnedNPC;

        private void Start()
        {
            if (spawnTrigger == SpawnTrigger.OnStart)
            {
                TrySpawn();
            }
        }

        private void OnEnable()
        {
            if (spawnTrigger == SpawnTrigger.OnEnable && (!spawnOnce || !hasSpawned))
            {
                TrySpawn();
            }
        }

        /// <summary>
        /// Attempt to spawn an NPC based on spawn chance
        /// </summary>
        public bool TrySpawn()
        {
            if (spawnOnce && hasSpawned) return false;
            if (npcPrefabs == null || npcPrefabs.Length == 0) return false;

            float roll = Random.value;
            if (roll > spawnChance) return false;

            return ForceSpawn();
        }

        /// <summary>
        /// Force spawn an NPC (ignores spawn chance)
        /// </summary>
        public bool ForceSpawn()
        {
            if (spawnOnce && hasSpawned) return false;
            if (npcPrefabs == null || npcPrefabs.Length == 0) return false;

            // Pick random prefab
            GameObject prefab = npcPrefabs[Random.Range(0, npcPrefabs.Length)];
            if (prefab == null) return false;

            // Calculate spawn position and rotation
            Vector3 spawnPos = transform.position + transform.TransformDirection(spawnOffset);
            Quaternion spawnRot = transform.rotation;

            if (randomRotation)
            {
                spawnRot = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            }

            // Spawn
            spawnedNPC = Instantiate(prefab, spawnPos, spawnRot);
            hasSpawned = true;

            Debug.Log($"[NPCSpawnPoint] Spawned {prefab.name} at {spawnPos}");

            if (destroyAfterSpawn)
            {
                Destroy(gameObject);
            }

            return true;
        }

        /// <summary>
        /// Despawn the NPC if one was spawned
        /// </summary>
        public void Despawn()
        {
            if (spawnedNPC != null)
            {
                Destroy(spawnedNPC);
                spawnedNPC = null;
            }
        }

        /// <summary>
        /// Reset so it can spawn again
        /// </summary>
        public void Reset()
        {
            Despawn();
            hasSpawned = false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Vector3 pos = transform.position + transform.TransformDirection(spawnOffset);
            Gizmos.DrawWireSphere(pos, 0.3f);
            Gizmos.DrawLine(transform.position, pos);
        }
    }

    public enum SpawnTrigger
    {
        OnStart,
        OnEnable,
        Manual
    }
}
