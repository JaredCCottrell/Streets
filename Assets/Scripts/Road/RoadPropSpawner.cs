using System.Collections.Generic;
using UnityEngine;

namespace Streets.Road
{
    public class RoadPropSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RoadGenerator roadGenerator;

        [Header("Prop Pools")]
        [SerializeField] private RoadPropPool[] propPools;

        [Header("Spawn Settings")]
        [SerializeField] private bool spawnOnStart = true;
        [SerializeField] private int randomSeed = -1; // -1 = random each time

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        // Track spawned props per segment for cleanup
        private Dictionary<RoadSegment, List<GameObject>> spawnedProps = new Dictionary<RoadSegment, List<GameObject>>();

        // Object pools for props
        private Dictionary<RoadPropData, Queue<GameObject>> propObjectPools = new Dictionary<RoadPropData, Queue<GameObject>>();

        private void Start()
        {
            if (randomSeed >= 0)
            {
                Random.InitState(randomSeed);
            }

            if (roadGenerator == null)
            {
                roadGenerator = FindFirstObjectByType<RoadGenerator>();
            }

            if (roadGenerator != null)
            {
                roadGenerator.OnSegmentSpawned += OnSegmentSpawned;
                roadGenerator.OnSegmentDespawned += OnSegmentDespawned;

                // Spawn props on existing segments if starting mid-game
                if (spawnOnStart)
                {
                    SpawnPropsOnExistingSegments();
                }
            }
        }

        private void OnDestroy()
        {
            if (roadGenerator != null)
            {
                roadGenerator.OnSegmentSpawned -= OnSegmentSpawned;
                roadGenerator.OnSegmentDespawned -= OnSegmentDespawned;
            }
        }

        private void SpawnPropsOnExistingSegments()
        {
            // Find all active road segments and spawn props
            RoadSegment[] segments = FindObjectsByType<RoadSegment>(FindObjectsSortMode.None);
            foreach (var segment in segments)
            {
                if (segment.gameObject.activeInHierarchy)
                {
                    SpawnPropsOnSegment(segment);
                }
            }
        }

        private void OnSegmentSpawned(RoadSegment segment)
        {
            SpawnPropsOnSegment(segment);
        }

        private void OnSegmentDespawned(RoadSegment segment)
        {
            CleanupPropsOnSegment(segment);
        }

        private void SpawnPropsOnSegment(RoadSegment segment)
        {
            if (segment == null || propPools == null || propPools.Length == 0) return;
            if (segment.PropSpawnPoints == null || segment.PropSpawnPoints.Length == 0) return;

            // Don't spawn twice on same segment
            if (spawnedProps.ContainsKey(segment)) return;

            List<GameObject> segmentProps = new List<GameObject>();
            spawnedProps[segment] = segmentProps;

            // Choose a random pool for this segment (or use all)
            RoadPropPool pool = propPools[Random.Range(0, propPools.Length)];

            // Determine how many props to spawn
            int propCount = Random.Range(pool.minPropsPerSegment, pool.maxPropsPerSegment + 1);
            propCount = Mathf.Min(propCount, segment.PropSpawnPoints.Length);

            // Shuffle spawn points and pick a subset
            List<int> spawnPointIndices = new List<int>();
            for (int i = 0; i < segment.PropSpawnPoints.Length; i++)
            {
                spawnPointIndices.Add(i);
            }
            ShuffleList(spawnPointIndices);

            int propsSpawned = 0;

            for (int i = 0; i < spawnPointIndices.Count && propsSpawned < propCount; i++)
            {
                int pointIndex = spawnPointIndices[i];
                Transform spawnPoint = segment.PropSpawnPoints[pointIndex];

                if (spawnPoint == null) continue;

                // Check spawn point usage chance
                if (Random.value > pool.spawnPointUsageChance) continue;

                // Determine which side this spawn point is on
                PropSide side = GetSpawnPointSide(segment, spawnPoint);

                // Get a random prop for this side
                RoadPropData propData = pool.GetRandomPropForSide(side);
                if (propData == null || propData.prefab == null) continue;

                // Check individual prop spawn chance
                if (Random.value > propData.spawnChance) continue;

                // Spawn the prop
                GameObject prop = SpawnProp(propData, spawnPoint, segment.transform);
                if (prop != null)
                {
                    segmentProps.Add(prop);
                    propsSpawned++;

                    // Handle clustering
                    if (propData.canCluster && propData.maxClusterSize > 1)
                    {
                        int clusterCount = Random.Range(1, propData.maxClusterSize);
                        for (int c = 0; c < clusterCount && propsSpawned < propCount; c++)
                        {
                            Vector3 clusterOffset = Random.insideUnitSphere * propData.clusterRadius;
                            clusterOffset.y = 0;

                            GameObject clusterProp = SpawnProp(propData, spawnPoint, segment.transform, clusterOffset);
                            if (clusterProp != null)
                            {
                                segmentProps.Add(clusterProp);
                                propsSpawned++;
                            }
                        }
                    }
                }
            }

            if (showDebugInfo)
            {
                Debug.Log($"[RoadPropSpawner] Spawned {propsSpawned} props on segment {segment.SegmentIndex}");
            }
        }

        private GameObject SpawnProp(RoadPropData propData, Transform spawnPoint, Transform parent, Vector3 additionalOffset = default)
        {
            // Try to get from pool first
            GameObject prop = GetFromPool(propData);

            if (prop == null)
            {
                prop = Instantiate(propData.prefab);
            }

            prop.transform.SetParent(parent);

            // Position
            Vector3 position = spawnPoint.position + propData.positionOffset + additionalOffset;
            prop.transform.position = position;

            // Rotation
            Quaternion rotation = spawnPoint.rotation * Quaternion.Euler(propData.rotationOffset);
            if (propData.randomRotation)
            {
                float randomY = Random.Range(-propData.maxRandomRotation, propData.maxRandomRotation);
                rotation *= Quaternion.Euler(0, randomY, 0);
            }
            prop.transform.rotation = rotation;

            // Scale
            if (propData.randomScale)
            {
                float scale = Random.Range(propData.minScale, propData.maxScale);
                prop.transform.localScale = Vector3.one * scale;
            }

            prop.SetActive(true);
            return prop;
        }

        private PropSide GetSpawnPointSide(RoadSegment segment, Transform spawnPoint)
        {
            // Determine if spawn point is on left or right side of road
            Vector3 localPos = segment.transform.InverseTransformPoint(spawnPoint.position);
            return localPos.x >= 0 ? PropSide.Right : PropSide.Left;
        }

        private void CleanupPropsOnSegment(RoadSegment segment)
        {
            if (!spawnedProps.TryGetValue(segment, out List<GameObject> props)) return;

            foreach (var prop in props)
            {
                if (prop != null)
                {
                    ReturnToPool(prop);
                }
            }

            props.Clear();
            spawnedProps.Remove(segment);
        }

        private GameObject GetFromPool(RoadPropData propData)
        {
            if (!propObjectPools.TryGetValue(propData, out Queue<GameObject> pool))
            {
                return null;
            }

            while (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                if (obj != null)
                {
                    return obj;
                }
            }

            return null;
        }

        private void ReturnToPool(GameObject prop)
        {
            prop.SetActive(false);

            // Find the prop data for this object (by name matching prefab)
            foreach (var pool in propPools)
            {
                if (pool == null || pool.props == null) continue;

                foreach (var propData in pool.props)
                {
                    if (propData != null && propData.prefab != null &&
                        prop.name.StartsWith(propData.prefab.name))
                    {
                        if (!propObjectPools.TryGetValue(propData, out Queue<GameObject> objPool))
                        {
                            objPool = new Queue<GameObject>();
                            propObjectPools[propData] = objPool;
                        }
                        objPool.Enqueue(prop);
                        return;
                    }
                }
            }

            // If no pool found, just destroy
            Destroy(prop);
        }

        private void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}
