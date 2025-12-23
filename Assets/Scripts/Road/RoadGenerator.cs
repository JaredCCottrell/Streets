using System.Collections.Generic;
using UnityEngine;

namespace Streets.Road
{
    public class RoadGenerator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private RoadSegment[] segmentPrefabs;

        [Header("Generation Settings")]
        [SerializeField] private int segmentsAhead = 5;
        [SerializeField] private int segmentsBehind = 2;
        [SerializeField] private float checkInterval = 0.5f;

        [Header("Segment Weights")]
        [SerializeField] private float straightWeight = 100f;
        [SerializeField] private float curveWeight = 0f;
        [SerializeField] private float specialWeight = 0f;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;

        // Active segments
        private LinkedList<RoadSegment> activeSegments = new LinkedList<RoadSegment>();
        private int nextSegmentIndex = 0;
        private float lastCheckTime;
        private RoadSegment lastKnownSegment;

        // Segment pools by type
        private Dictionary<SegmentType, List<RoadSegment>> segmentPools = new Dictionary<SegmentType, List<RoadSegment>>();
        private Dictionary<SegmentType, RoadSegment> prefabsByType = new Dictionary<SegmentType, RoadSegment>();

        // Events
        public System.Action<RoadSegment> OnSegmentSpawned;
        public System.Action<RoadSegment> OnSegmentDespawned;
        public System.Action<RoadSegment> OnPlayerEnteredSegment;

        // Properties
        public int CurrentSegmentIndex => GetCurrentSegmentIndex();
        public RoadSegment CurrentSegment => GetCurrentSegment();
        public float TotalDistanceTraveled => nextSegmentIndex > 0 ? (nextSegmentIndex - segmentsAhead) * GetAverageSegmentLength() : 0;

        private void Start()
        {
            InitializePools();
            GenerateInitialRoad();
        }

        private void Update()
        {
            if (Time.time - lastCheckTime >= checkInterval)
            {
                lastCheckTime = Time.time;
                UpdateRoad();
            }
        }

        private void InitializePools()
        {
            // Organize prefabs by type
            foreach (var prefab in segmentPrefabs)
            {
                if (prefab != null)
                {
                    prefabsByType[prefab.Type] = prefab;
                    segmentPools[prefab.Type] = new List<RoadSegment>();
                }
            }
        }

        private void GenerateInitialRoad()
        {
            // Generate initial segments
            Vector3 spawnPosition = transform.position;
            Quaternion spawnRotation = transform.rotation;

            for (int i = 0; i < segmentsAhead + segmentsBehind; i++)
            {
                RoadSegment segment = SpawnSegment(spawnPosition, spawnRotation);
                if (segment != null)
                {
                    spawnPosition = segment.GetNextSegmentPosition();
                    spawnRotation = segment.GetNextSegmentRotation();
                }
            }

            // Position player at start if needed
            if (player != null && activeSegments.Count > 0)
            {
                RoadSegment firstSegment = activeSegments.First.Value;
                if (firstSegment.EntryPoint != null)
                {
                    // Player starts slightly after entry
                    player.position = firstSegment.EntryPoint.position + firstSegment.EntryPoint.forward * 5f;
                    player.position = new Vector3(player.position.x, player.position.y + 1f, player.position.z);
                }
            }
        }

        private void UpdateRoad()
        {
            if (player == null || activeSegments.Count == 0) return;

            float playerZ = GetPlayerProgressAlongRoad();
            RoadSegment currentSegment = GetCurrentSegment();

            // Check if we need to spawn more segments ahead
            if (currentSegment != null)
            {
                // Check if player entered a new segment
                if (currentSegment != lastKnownSegment)
                {
                    // Notify the old segment that player exited
                    if (lastKnownSegment != null)
                    {
                        lastKnownSegment.NotifyPlayerExited();
                    }

                    // Notify the new segment that player entered
                    currentSegment.NotifyPlayerEntered();
                    OnPlayerEnteredSegment?.Invoke(currentSegment);
                    lastKnownSegment = currentSegment;
                }

                int segmentsAheadOfPlayer = CountSegmentsAhead(currentSegment);

                while (segmentsAheadOfPlayer < segmentsAhead)
                {
                    SpawnNextSegment();
                    segmentsAheadOfPlayer++;
                }

                // Check if we need to despawn segments behind
                int segmentsBehindPlayer = CountSegmentsBehind(currentSegment);

                while (segmentsBehindPlayer > segmentsBehind)
                {
                    DespawnOldestSegment();
                    segmentsBehindPlayer--;
                }
            }
        }

        private RoadSegment SpawnSegment(Vector3 position, Quaternion rotation)
        {
            SegmentType type = ChooseSegmentType();
            RoadSegment segment = GetOrCreateSegment(type);

            if (segment != null)
            {
                segment.gameObject.SetActive(true);
                segment.AlignToEntry(position, rotation);
                segment.SegmentIndex = nextSegmentIndex++;
                segment.HasBeenVisited = false;

                activeSegments.AddLast(segment);
                OnSegmentSpawned?.Invoke(segment);
            }

            return segment;
        }

        private void SpawnNextSegment()
        {
            if (activeSegments.Count == 0) return;

            RoadSegment lastSegment = activeSegments.Last.Value;
            Vector3 nextPosition = lastSegment.GetNextSegmentPosition();
            Quaternion nextRotation = lastSegment.GetNextSegmentRotation();

            SpawnSegment(nextPosition, nextRotation);
        }

        private void DespawnOldestSegment()
        {
            if (activeSegments.Count == 0) return;

            RoadSegment oldest = activeSegments.First.Value;
            activeSegments.RemoveFirst();

            // Return to pool
            ReturnToPool(oldest);
            OnSegmentDespawned?.Invoke(oldest);
        }

        private SegmentType ChooseSegmentType()
        {
            // Simple weighted random selection
            float totalWeight = straightWeight + curveWeight + specialWeight;
            float roll = Random.Range(0f, totalWeight);

            if (roll < straightWeight)
            {
                return SegmentType.Straight;
            }
            else if (roll < straightWeight + curveWeight)
            {
                // Alternate curves for variety
                return Random.value > 0.5f ? SegmentType.SlightLeft : SegmentType.SlightRight;
            }
            else
            {
                // Special segments - pick randomly from available
                SegmentType[] specials = { SegmentType.Overpass, SegmentType.Bridge, SegmentType.RestStop };
                SegmentType chosen = specials[Random.Range(0, specials.Length)];

                // Fall back to straight if we don't have this prefab
                if (!prefabsByType.ContainsKey(chosen))
                {
                    return SegmentType.Straight;
                }
                return chosen;
            }
        }

        private RoadSegment GetOrCreateSegment(SegmentType type)
        {
            // Try to get from pool
            if (segmentPools.TryGetValue(type, out List<RoadSegment> pool) && pool.Count > 0)
            {
                RoadSegment segment = pool[pool.Count - 1];
                pool.RemoveAt(pool.Count - 1);
                return segment;
            }

            // Create new if no pooled segment available
            if (prefabsByType.TryGetValue(type, out RoadSegment prefab))
            {
                RoadSegment newSegment = Instantiate(prefab, transform);
                return newSegment;
            }

            // Fall back to straight
            if (type != SegmentType.Straight && prefabsByType.TryGetValue(SegmentType.Straight, out RoadSegment straightPrefab))
            {
                RoadSegment newSegment = Instantiate(straightPrefab, transform);
                return newSegment;
            }

            Debug.LogError($"[RoadGenerator] No prefab found for segment type: {type}");
            return null;
        }

        private void ReturnToPool(RoadSegment segment)
        {
            segment.gameObject.SetActive(false);
            segment.ResetState();

            if (segmentPools.TryGetValue(segment.Type, out List<RoadSegment> pool))
            {
                pool.Add(segment);
            }
        }

        private RoadSegment GetCurrentSegment()
        {
            if (player == null) return null;

            foreach (var segment in activeSegments)
            {
                if (IsPlayerInSegment(segment))
                {
                    return segment;
                }
            }

            // Default to first segment
            return activeSegments.Count > 0 ? activeSegments.First.Value : null;
        }

        private int GetCurrentSegmentIndex()
        {
            RoadSegment current = GetCurrentSegment();
            return current != null ? current.SegmentIndex : 0;
        }

        private bool IsPlayerInSegment(RoadSegment segment)
        {
            if (player == null || segment == null) return false;

            // Transform player position to segment's local space
            Vector3 localPos = segment.transform.InverseTransformPoint(player.position);

            // Check if within segment length (Z axis)
            return localPos.z >= 0 && localPos.z <= segment.SegmentLength;
        }

        private float GetPlayerProgressAlongRoad()
        {
            if (player == null) return 0f;

            // Simple Z-axis progress
            return player.position.z;
        }

        private int CountSegmentsAhead(RoadSegment currentSegment)
        {
            int count = 0;
            bool foundCurrent = false;

            foreach (var segment in activeSegments)
            {
                if (foundCurrent)
                {
                    count++;
                }
                if (segment == currentSegment)
                {
                    foundCurrent = true;
                }
            }

            return count;
        }

        private int CountSegmentsBehind(RoadSegment currentSegment)
        {
            int count = 0;

            foreach (var segment in activeSegments)
            {
                if (segment == currentSegment)
                {
                    break;
                }
                count++;
            }

            return count;
        }

        private float GetAverageSegmentLength()
        {
            if (segmentPrefabs == null || segmentPrefabs.Length == 0) return 50f;

            float total = 0f;
            foreach (var prefab in segmentPrefabs)
            {
                if (prefab != null)
                {
                    total += prefab.SegmentLength;
                }
            }
            return total / segmentPrefabs.Length;
        }

        private void OnDrawGizmos()
        {
            if (!showDebugInfo) return;

            // Draw active segments info
            Gizmos.color = Color.green;
            foreach (var segment in activeSegments)
            {
                if (segment != null)
                {
                    Gizmos.DrawWireCube(segment.transform.position + Vector3.up * 2f, Vector3.one);
                }
            }
        }

        private void OnGUI()
        {
            if (!showDebugInfo || !Application.isPlaying) return;

            GUILayout.BeginArea(new Rect(10, 10, 250, 100));
            GUILayout.Label($"Active Segments: {activeSegments.Count}");
            GUILayout.Label($"Current Segment: {CurrentSegmentIndex}");
            GUILayout.Label($"Distance: {TotalDistanceTraveled:F0}m");
            GUILayout.EndArea();
        }
    }
}
