using System;
using UnityEngine;

namespace Streets.Effects
{
    /// <summary>
    /// Manages player intoxication level from consuming alcohol.
    /// Intoxication stacks when drinking multiple drinks in a short period
    /// and gradually wears off over time.
    /// </summary>
    public class IntoxicationSystem : MonoBehaviour
    {
        [Header("Intoxication Settings")]
        [SerializeField] private float maxIntoxication = 100f;
        [SerializeField] private float decayRate = 5f; // Per second
        [SerializeField] private float decayDelay = 3f; // Seconds before decay starts

        [Header("Effect Thresholds")]
        [Tooltip("Intoxication level where effects start to show")]
        [SerializeField] private float mildThreshold = 20f;
        [Tooltip("Intoxication level for moderate effects")]
        [SerializeField] private float moderateThreshold = 50f;
        [Tooltip("Intoxication level for severe effects")]
        [SerializeField] private float severeThreshold = 80f;

        [Header("Debug")]
        [SerializeField] private bool showDebugUI = false;

        // State
        private float currentIntoxication = 0f;
        private float timeSinceLastDrink = 0f;

        // Events
        public event Action<float> OnIntoxicationChanged;
        public event Action<IntoxicationLevel> OnIntoxicationLevelChanged;

        // Properties
        public float CurrentIntoxication => currentIntoxication;
        public float MaxIntoxication => maxIntoxication;
        public float IntoxicationPercent => currentIntoxication / maxIntoxication;
        public IntoxicationLevel CurrentLevel => GetIntoxicationLevel();

        public bool IsSober => currentIntoxication < mildThreshold;
        public bool IsMild => currentIntoxication >= mildThreshold && currentIntoxication < moderateThreshold;
        public bool IsModerate => currentIntoxication >= moderateThreshold && currentIntoxication < severeThreshold;
        public bool IsSevere => currentIntoxication >= severeThreshold;

        private IntoxicationLevel previousLevel = IntoxicationLevel.Sober;

        private void Update()
        {
            timeSinceLastDrink += Time.deltaTime;

            // Decay intoxication over time after delay
            if (currentIntoxication > 0 && timeSinceLastDrink >= decayDelay)
            {
                float oldIntox = currentIntoxication;
                currentIntoxication -= decayRate * Time.deltaTime;
                currentIntoxication = Mathf.Max(0f, currentIntoxication);

                if (Mathf.Abs(oldIntox - currentIntoxication) > 0.01f)
                {
                    OnIntoxicationChanged?.Invoke(currentIntoxication);
                }
            }

            // Check for level changes
            IntoxicationLevel currentLevel = GetIntoxicationLevel();
            if (currentLevel != previousLevel)
            {
                previousLevel = currentLevel;
                OnIntoxicationLevelChanged?.Invoke(currentLevel);
            }
        }

        /// <summary>
        /// Add intoxication from consuming alcohol
        /// </summary>
        public void AddIntoxication(float amount)
        {
            if (amount <= 0) return;

            float oldIntox = currentIntoxication;
            currentIntoxication += amount;
            currentIntoxication = Mathf.Min(currentIntoxication, maxIntoxication);

            // Reset decay timer
            timeSinceLastDrink = 0f;

            Debug.Log($"[IntoxicationSystem] Drank! Intoxication: {oldIntox:F1} -> {currentIntoxication:F1}");
            OnIntoxicationChanged?.Invoke(currentIntoxication);
        }

        /// <summary>
        /// Instantly sober up (for debugging or special items)
        /// </summary>
        public void Sober()
        {
            currentIntoxication = 0f;
            OnIntoxicationChanged?.Invoke(currentIntoxication);
        }

        /// <summary>
        /// Set intoxication directly (for loading saved state)
        /// </summary>
        public void SetIntoxication(float amount)
        {
            currentIntoxication = Mathf.Clamp(amount, 0f, maxIntoxication);
            OnIntoxicationChanged?.Invoke(currentIntoxication);
        }

        private IntoxicationLevel GetIntoxicationLevel()
        {
            if (currentIntoxication >= severeThreshold) return IntoxicationLevel.Severe;
            if (currentIntoxication >= moderateThreshold) return IntoxicationLevel.Moderate;
            if (currentIntoxication >= mildThreshold) return IntoxicationLevel.Mild;
            return IntoxicationLevel.Sober;
        }

        private void OnGUI()
        {
            if (!showDebugUI) return;

            GUILayout.BeginArea(new Rect(Screen.width - 200, 10, 190, 80));
            GUILayout.BeginVertical("box");
            GUILayout.Label($"Intoxication: {currentIntoxication:F1}/{maxIntoxication}");
            GUILayout.Label($"Level: {CurrentLevel}");
            GUILayout.Label($"Decay in: {Mathf.Max(0, decayDelay - timeSinceLastDrink):F1}s");
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }

    public enum IntoxicationLevel
    {
        Sober,
        Mild,
        Moderate,
        Severe
    }
}
