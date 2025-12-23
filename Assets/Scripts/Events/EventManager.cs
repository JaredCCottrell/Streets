using System;
using System.Collections.Generic;
using UnityEngine;
using Streets.Road;
using Streets.Survival;

namespace Streets.Events
{
    /// <summary>
    /// Manages event triggering, spawning, and lifecycle.
    /// Listens to RoadGenerator for segment changes and spawns events based on sanity.
    /// </summary>
    public class EventManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RoadGenerator roadGenerator;
        [SerializeField] private SanitySystem sanitySystem;

        [Header("Event Pools")]
        [SerializeField] private EventPool mainEventPool;

        [Header("Settings")]
        [Tooltip("Minimum segments between events (prevents event spam)")]
        [SerializeField] private int minSegmentsBetweenEvents = 2;

        [Tooltip("Maximum concurrent events per category")]
        [SerializeField] private int maxEventsPerCategory = 1;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private bool logEvents = true;

        // Active events tracking
        private Dictionary<EventCategory, List<ActiveEvent>> activeEvents = new Dictionary<EventCategory, List<ActiveEvent>>();
        private int segmentsSinceLastEvent = 0;

        // Events
        public event Action<EventData, RoadSegment> OnEventTriggered;
        public event Action<EventData> OnEventEnded;

        // Stats
        public int TotalEventsTriggered { get; private set; }
        public int SegmentsSinceLastEvent => segmentsSinceLastEvent;

        private void Awake()
        {
            // Initialize active events dictionary
            foreach (EventCategory category in Enum.GetValues(typeof(EventCategory)))
            {
                activeEvents[category] = new List<ActiveEvent>();
            }
        }

        private void Start()
        {
            if (roadGenerator != null)
            {
                roadGenerator.OnPlayerEnteredSegment += HandlePlayerEnteredSegment;
            }
            else
            {
                Debug.LogError("[EventManager] RoadGenerator reference not set!");
            }
        }

        private void OnDestroy()
        {
            if (roadGenerator != null)
            {
                roadGenerator.OnPlayerEnteredSegment -= HandlePlayerEnteredSegment;
            }
        }

        private void Update()
        {
            UpdateActiveEvents();
        }

        private void HandlePlayerEnteredSegment(RoadSegment segment)
        {
            segmentsSinceLastEvent++;

            // Check if this segment can trigger events
            if (!segment.CanTriggerEvent)
            {
                if (logEvents) Debug.Log($"[EventManager] Segment {segment.SegmentIndex}: Events disabled");
                return;
            }

            // Check if enough segments have passed since last event
            if (segmentsSinceLastEvent < minSegmentsBetweenEvents)
            {
                if (logEvents) Debug.Log($"[EventManager] Segment {segment.SegmentIndex}: Too soon since last event ({segmentsSinceLastEvent}/{minSegmentsBetweenEvents})");
                return;
            }

            // Check if already triggered on this segment
            if (segment.EventTriggered)
            {
                if (logEvents) Debug.Log($"[EventManager] Segment {segment.SegmentIndex}: Already triggered");
                return;
            }

            // Roll for event trigger
            if (TryTriggerEvent(segment))
            {
                segment.EventTriggered = true;
                segmentsSinceLastEvent = 0;
            }
        }

        private bool TryTriggerEvent(RoadSegment segment)
        {
            if (mainEventPool == null)
            {
                Debug.LogWarning("[EventManager] No event pool assigned!");
                return false;
            }

            float sanityPercent = sanitySystem != null ? sanitySystem.SanityPercent : 1f;

            // Get trigger chance (segment override or pool default)
            float triggerChance = segment.TriggerChanceOverride >= 0
                ? segment.TriggerChanceOverride
                : mainEventPool.GetTriggerChance(sanityPercent);

            float roll = UnityEngine.Random.value;

            if (logEvents)
            {
                Debug.Log($"[EventManager] Segment {segment.SegmentIndex}: Roll {roll:F2} vs {triggerChance:F2} (Sanity: {sanityPercent:P0})");
            }

            if (roll > triggerChance)
            {
                return false; // No event this time
            }

            // Event triggered! Select which categories to spawn
            return SpawnEventsForSegment(segment, sanityPercent);
        }

        private bool SpawnEventsForSegment(RoadSegment segment, float sanityPercent)
        {
            bool anySpawned = false;

            // Always try to spawn an Atmospheric event (they can always overlap)
            if (TrySpawnEventOfCategory(EventCategory.Atmospheric, segment, sanityPercent))
            {
                anySpawned = true;
            }

            // Roll for additional category based on sanity
            // Lower sanity = higher chance of multiple event types
            float multiEventChance = (1f - sanityPercent) * 0.5f; // 0-50% based on missing sanity

            if (UnityEngine.Random.value < multiEventChance || !anySpawned)
            {
                // Pick a secondary category
                EventCategory[] secondaryCategories = { EventCategory.Creature, EventCategory.Obstacle, EventCategory.Apparition };
                EventCategory chosen = secondaryCategories[UnityEngine.Random.Range(0, secondaryCategories.Length)];

                if (TrySpawnEventOfCategory(chosen, segment, sanityPercent))
                {
                    anySpawned = true;
                }
            }

            return anySpawned;
        }

        private bool TrySpawnEventOfCategory(EventCategory category, RoadSegment segment, float sanityPercent)
        {
            // Check if we can spawn more of this category
            if (activeEvents[category].Count >= maxEventsPerCategory)
            {
                if (logEvents) Debug.Log($"[EventManager] Max {category} events active ({maxEventsPerCategory})");
                return false;
            }

            // Get sanity-weighted event from pool
            EventData eventData = mainEventPool.GetSanityWeightedEvent(category, sanityPercent);

            if (eventData == null)
            {
                if (logEvents) Debug.Log($"[EventManager] No {category} events in pool");
                return false;
            }

            // Spawn the event
            SpawnEvent(eventData, segment);
            return true;
        }

        private void SpawnEvent(EventData eventData, RoadSegment segment)
        {
            Transform spawnPoint = eventData.UseSpawnPoints
                ? segment.GetRandomEventSpawnPoint()
                : segment.transform;

            GameObject eventInstance = null;

            if (eventData.EventPrefab != null)
            {
                eventInstance = Instantiate(
                    eventData.EventPrefab,
                    spawnPoint.position,
                    spawnPoint.rotation
                );
            }

            // Apply sanity impact
            if (sanitySystem != null && eventData.SanityImpact > 0)
            {
                sanitySystem.LoseSanity(eventData.SanityImpact);
            }

            // Play trigger sound
            if (eventData.TriggerSound != null)
            {
                AudioSource.PlayClipAtPoint(eventData.TriggerSound, spawnPoint.position, eventData.SoundVolume);
            }

            // Track active event
            var activeEvent = new ActiveEvent
            {
                Data = eventData,
                Instance = eventInstance,
                SpawnTime = Time.time,
                Segment = segment
            };

            activeEvents[eventData.Category].Add(activeEvent);
            TotalEventsTriggered++;

            if (logEvents)
            {
                Debug.Log($"[EventManager] Spawned {eventData.EventName} ({eventData.Category}/{eventData.Difficulty}) at segment {segment.SegmentIndex}");
            }

            OnEventTriggered?.Invoke(eventData, segment);
        }

        private void UpdateActiveEvents()
        {
            foreach (var category in activeEvents.Keys)
            {
                var events = activeEvents[category];

                for (int i = events.Count - 1; i >= 0; i--)
                {
                    var activeEvent = events[i];

                    // Check if event should end
                    bool shouldEnd = false;

                    // No prefab events end immediately (just a trigger, like a sound)
                    if (activeEvent.Data.EventPrefab == null)
                    {
                        shouldEnd = true;
                    }
                    // Self-managed: instance handles its own cleanup, check if destroyed
                    else if (activeEvent.Data.SelfManaged)
                    {
                        if (activeEvent.Instance == null)
                        {
                            shouldEnd = true;
                        }
                    }
                    // Duration-based: EventManager handles cleanup after duration
                    else if (activeEvent.Data.Duration > 0)
                    {
                        if (Time.time - activeEvent.SpawnTime >= activeEvent.Data.Duration)
                        {
                            shouldEnd = true;
                        }
                    }

                    if (shouldEnd)
                    {
                        EndEvent(activeEvent);
                        events.RemoveAt(i);
                    }
                }
            }
        }

        private void EndEvent(ActiveEvent activeEvent)
        {
            // Cleanup instance if it still exists
            if (activeEvent.Instance != null)
            {
                Destroy(activeEvent.Instance);
            }

            if (logEvents)
            {
                Debug.Log($"[EventManager] Ended {activeEvent.Data.EventName}");
            }

            OnEventEnded?.Invoke(activeEvent.Data);
        }

        /// <summary>
        /// Force spawn a specific event at a segment (for testing/scripted events)
        /// </summary>
        public void ForceSpawnEvent(EventData eventData, RoadSegment segment)
        {
            if (eventData == null || segment == null) return;
            SpawnEvent(eventData, segment);
        }

        /// <summary>
        /// Clear all active events
        /// </summary>
        public void ClearAllEvents()
        {
            foreach (var category in activeEvents.Keys)
            {
                foreach (var activeEvent in activeEvents[category])
                {
                    if (activeEvent.Instance != null)
                    {
                        Destroy(activeEvent.Instance);
                    }
                }
                activeEvents[category].Clear();
            }
        }

        /// <summary>
        /// Get count of active events in a category
        /// </summary>
        public int GetActiveEventCount(EventCategory category)
        {
            return activeEvents.TryGetValue(category, out var list) ? list.Count : 0;
        }

        /// <summary>
        /// Get total count of all active events
        /// </summary>
        public int GetTotalActiveEventCount()
        {
            int total = 0;
            foreach (var list in activeEvents.Values)
            {
                total += list.Count;
            }
            return total;
        }

        private void OnGUI()
        {
            if (!showDebugInfo || !Application.isPlaying) return;

            GUILayout.BeginArea(new Rect(10, 120, 300, 200));
            GUILayout.Label("=== Event Manager ===");
            GUILayout.Label($"Total Triggered: {TotalEventsTriggered}");
            GUILayout.Label($"Segments Since Event: {segmentsSinceLastEvent}");
            GUILayout.Label($"Active Events: {GetTotalActiveEventCount()}");

            if (sanitySystem != null)
            {
                GUILayout.Label($"Sanity: {sanitySystem.SanityPercent:P0}");
                GUILayout.Label($"Max Difficulty: {mainEventPool?.GetMaxDifficultyForSanity(sanitySystem.SanityPercent)}");
            }

            foreach (EventCategory cat in Enum.GetValues(typeof(EventCategory)))
            {
                int count = GetActiveEventCount(cat);
                if (count > 0)
                {
                    GUILayout.Label($"  {cat}: {count}");
                }
            }

            GUILayout.EndArea();
        }

        /// <summary>
        /// Tracks an active event instance
        /// </summary>
        private class ActiveEvent
        {
            public EventData Data;
            public GameObject Instance;
            public float SpawnTime;
            public RoadSegment Segment;
        }
    }
}
