using System.Collections.Generic;
using UnityEngine;

namespace Streets.Events
{
    /// <summary>
    /// ScriptableObject containing a pool of events organized by category and difficulty.
    /// Used by EventManager to select appropriate events based on player sanity.
    /// </summary>
    [CreateAssetMenu(fileName = "New Event Pool", menuName = "Streets/Events/Event Pool")]
    public class EventPool : ScriptableObject
    {
        [Header("Pool Settings")]
        [SerializeField] private string poolName;

        [Tooltip("Base chance (0-1) for an event segment to trigger an event")]
        [Range(0f, 1f)]
        [SerializeField] private float baseTriggerChance = 0.4f;

        [Tooltip("Additional trigger chance per missing sanity percent (0-1 scale)")]
        [Range(0f, 1f)]
        [SerializeField] private float sanityTriggerBonus = 0.5f;

        [Header("Events")]
        [SerializeField] private List<EventData> events = new List<EventData>();

        // Properties
        public string PoolName => string.IsNullOrEmpty(poolName) ? name : poolName;
        public float BaseTriggerChance => baseTriggerChance;
        public float SanityTriggerBonus => sanityTriggerBonus;
        public IReadOnlyList<EventData> Events => events;

        // Cached lookups (built on first access)
        private Dictionary<EventCategory, List<EventData>> eventsByCategory;
        private Dictionary<EventDifficulty, List<EventData>> eventsByDifficulty;
        private bool cacheBuilt = false;

        private void OnEnable()
        {
            // Rebuild cache when asset is loaded/modified
            cacheBuilt = false;
        }

        private void BuildCache()
        {
            eventsByCategory = new Dictionary<EventCategory, List<EventData>>();
            eventsByDifficulty = new Dictionary<EventDifficulty, List<EventData>>();

            // Initialize all categories and difficulties
            foreach (EventCategory cat in System.Enum.GetValues(typeof(EventCategory)))
                eventsByCategory[cat] = new List<EventData>();

            foreach (EventDifficulty diff in System.Enum.GetValues(typeof(EventDifficulty)))
                eventsByDifficulty[diff] = new List<EventData>();

            // Populate
            foreach (var evt in events)
            {
                if (evt == null) continue;
                eventsByCategory[evt.Category].Add(evt);
                eventsByDifficulty[evt.Difficulty].Add(evt);
            }

            cacheBuilt = true;
        }

        /// <summary>
        /// Calculate trigger chance based on player sanity.
        /// Lower sanity = higher chance.
        /// </summary>
        public float GetTriggerChance(float sanityPercent)
        {
            // sanityPercent is 0-1, where 1 = full sanity
            float missingSanity = 1f - Mathf.Clamp01(sanityPercent);
            return Mathf.Clamp01(baseTriggerChance + (missingSanity * sanityTriggerBonus));
        }

        /// <summary>
        /// Get all events of a specific category
        /// </summary>
        public List<EventData> GetEventsByCategory(EventCategory category)
        {
            if (!cacheBuilt) BuildCache();
            return eventsByCategory.TryGetValue(category, out var list) ? list : new List<EventData>();
        }

        /// <summary>
        /// Get all events of a specific difficulty
        /// </summary>
        public List<EventData> GetEventsByDifficulty(EventDifficulty difficulty)
        {
            if (!cacheBuilt) BuildCache();
            return eventsByDifficulty.TryGetValue(difficulty, out var list) ? list : new List<EventData>();
        }

        /// <summary>
        /// Get events matching both category and difficulty
        /// </summary>
        public List<EventData> GetEvents(EventCategory category, EventDifficulty difficulty)
        {
            if (!cacheBuilt) BuildCache();

            var result = new List<EventData>();
            foreach (var evt in events)
            {
                if (evt != null && evt.Category == category && evt.Difficulty == difficulty)
                    result.Add(evt);
            }
            return result;
        }

        /// <summary>
        /// Get events of a category at or below a maximum difficulty
        /// </summary>
        public List<EventData> GetEventsUpToDifficulty(EventCategory category, EventDifficulty maxDifficulty)
        {
            if (!cacheBuilt) BuildCache();

            var result = new List<EventData>();
            foreach (var evt in events)
            {
                if (evt != null && evt.Category == category && evt.Difficulty <= maxDifficulty)
                    result.Add(evt);
            }
            return result;
        }

        /// <summary>
        /// Get a random event from the pool (unfiltered)
        /// </summary>
        public EventData GetRandomEvent()
        {
            if (events.Count == 0) return null;
            return events[Random.Range(0, events.Count)];
        }

        /// <summary>
        /// Get a random event of a specific category
        /// </summary>
        public EventData GetRandomEvent(EventCategory category)
        {
            var categoryEvents = GetEventsByCategory(category);
            if (categoryEvents.Count == 0) return null;
            return categoryEvents[Random.Range(0, categoryEvents.Count)];
        }

        /// <summary>
        /// Get a random event based on sanity-weighted difficulty.
        /// Lower sanity = higher chance of harder events.
        /// </summary>
        public EventData GetSanityWeightedEvent(EventCategory category, float sanityPercent)
        {
            var categoryEvents = GetEventsByCategory(category);
            if (categoryEvents.Count == 0) return null;

            // Determine max difficulty based on sanity
            EventDifficulty maxDiff = GetMaxDifficultyForSanity(sanityPercent);

            // Get events up to that difficulty
            var eligibleEvents = GetEventsUpToDifficulty(category, maxDiff);
            if (eligibleEvents.Count == 0)
            {
                // Fallback to any event in category
                return categoryEvents[Random.Range(0, categoryEvents.Count)];
            }

            // Weight toward harder events as sanity drops
            return WeightedRandomSelect(eligibleEvents, sanityPercent);
        }

        /// <summary>
        /// Determine maximum event difficulty based on sanity level
        /// </summary>
        public EventDifficulty GetMaxDifficultyForSanity(float sanityPercent)
        {
            // 100-75% sanity: up to Unsettling
            // 75-50% sanity: up to Dangerous
            // 50-25% sanity: up to Terrifying
            // 25-0% sanity: up to Nightmare

            if (sanityPercent > 0.75f) return EventDifficulty.Unsettling;
            if (sanityPercent > 0.50f) return EventDifficulty.Dangerous;
            if (sanityPercent > 0.25f) return EventDifficulty.Terrifying;
            return EventDifficulty.Nightmare;
        }

        /// <summary>
        /// Select an event with weighting toward harder events as sanity drops
        /// </summary>
        private EventData WeightedRandomSelect(List<EventData> events, float sanityPercent)
        {
            if (events.Count == 0) return null;
            if (events.Count == 1) return events[0];

            // Calculate weights: higher difficulty = higher weight when sanity is low
            float[] weights = new float[events.Count];
            float totalWeight = 0f;

            // Invert sanity: 0% sanity = 1.0 insanity factor, 100% sanity = 0.0
            float insanityFactor = 1f - Mathf.Clamp01(sanityPercent);

            for (int i = 0; i < events.Count; i++)
            {
                // Base weight of 1, plus difficulty bonus scaled by insanity
                float difficultyBonus = events[i].DifficultyLevel * insanityFactor * 2f;
                weights[i] = 1f + difficultyBonus;
                totalWeight += weights[i];
            }

            // Weighted random selection
            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            for (int i = 0; i < events.Count; i++)
            {
                cumulative += weights[i];
                if (roll <= cumulative)
                    return events[i];
            }

            return events[events.Count - 1];
        }
    }
}
