using UnityEngine;

namespace Streets.Events
{
    /// <summary>
    /// ScriptableObject defining a single event type.
    /// </summary>
    [CreateAssetMenu(fileName = "New Event", menuName = "Streets/Events/Event Data")]
    public class EventData : ScriptableObject
    {
        [Header("Event Info")]
        [SerializeField] private string eventName;
        [SerializeField][TextArea(2, 4)] private string description;

        [Header("Classification")]
        [SerializeField] private EventCategory category = EventCategory.Atmospheric;
        [SerializeField] private EventDifficulty difficulty = EventDifficulty.Harmless;

        [Header("Spawning")]
        [Tooltip("Prefab to instantiate when this event triggers")]
        [SerializeField] private GameObject eventPrefab;

        [Tooltip("If true, spawns at a random event spawn point. If false, spawns at segment center.")]
        [SerializeField] private bool useSpawnPoints = true;

        [Header("Effects")]
        [Tooltip("Sanity lost when this event triggers (can be 0 for harmless events)")]
        [SerializeField] private float sanityImpact = 0f;

        [Tooltip("How long this event lasts in seconds (0 = instant/managed by prefab)")]
        [SerializeField] private float duration = 0f;

        [Tooltip("If true, event cleans itself up. If false, EventManager handles cleanup.")]
        [SerializeField] private bool selfManaged = true;

        [Header("Audio")]
        [Tooltip("Sound to play when event triggers")]
        [SerializeField] private AudioClip triggerSound;

        [Tooltip("Volume for trigger sound")]
        [Range(0f, 1f)]
        [SerializeField] private float soundVolume = 1f;

        // Properties
        public string EventName => string.IsNullOrEmpty(eventName) ? name : eventName;
        public string Description => description;
        public EventCategory Category => category;
        public EventDifficulty Difficulty => difficulty;
        public GameObject EventPrefab => eventPrefab;
        public bool UseSpawnPoints => useSpawnPoints;
        public float SanityImpact => sanityImpact;
        public float Duration => duration;
        public bool SelfManaged => selfManaged;
        public AudioClip TriggerSound => triggerSound;
        public float SoundVolume => soundVolume;

        /// <summary>
        /// Returns true if this event has no direct threat to the player
        /// </summary>
        public bool IsHarmless => difficulty == EventDifficulty.Harmless;

        /// <summary>
        /// Get difficulty as int (0-4) for calculations
        /// </summary>
        public int DifficultyLevel => (int)difficulty;
    }
}
