using System;
using UnityEngine;

namespace Streets.Survival
{
    public class SanitySystem : MonoBehaviour
    {
        [Header("Sanity Settings")]
        [SerializeField] private float maxSanity = 100f;
        [SerializeField] private float currentSanity;

        [Header("Insanity Settings")]
        [SerializeField] private float insanityThreshold = 0f;
        [SerializeField] private float insanityDamage = 3f;
        [SerializeField] private float insanityDamageInterval = 4f;

        [Header("References")]
        [SerializeField] private HealthSystem healthSystem;

        // State
        private float insanityTimer;
        private bool isInsane;

        // Events
        public event Action<float, float> OnSanityChanged; // current, max
        public event Action OnInsanityStarted;
        public event Action OnInsanityEnded;
        public event Action<float> OnSanityRestored; // amount restored
        public event Action<float> OnSanityLost; // amount lost

        // Properties
        public float CurrentSanity => currentSanity;
        public float MaxSanity => maxSanity;
        public float SanityPercent => currentSanity / maxSanity;
        public bool IsInsane => isInsane;
        public bool IsSane => currentSanity >= maxSanity;

        private void Awake()
        {
            currentSanity = maxSanity;
        }

        private void Update()
        {
            HandleInsanity();
        }

        private void HandleInsanity()
        {
            if (!isInsane || healthSystem == null) return;

            insanityTimer += Time.deltaTime;

            if (insanityTimer >= insanityDamageInterval)
            {
                healthSystem.TakeDamage(insanityDamage);
                insanityTimer = 0f;
            }
        }

        /// <summary>
        /// Reduce sanity by an amount (called by events, encounters, etc.)
        /// </summary>
        public void LoseSanity(float amount)
        {
            if (amount <= 0) return;

            float previousSanity = currentSanity;
            currentSanity = Mathf.Max(currentSanity - amount, 0);

            float actualLoss = previousSanity - currentSanity;
            if (actualLoss > 0)
            {
                OnSanityLost?.Invoke(actualLoss);
                OnSanityChanged?.Invoke(currentSanity, maxSanity);
            }

            // Check insanity state
            if (currentSanity <= insanityThreshold && !isInsane)
            {
                isInsane = true;
                OnInsanityStarted?.Invoke();
            }
        }

        /// <summary>
        /// Restore sanity by an amount (rest, items, safe zones, etc.)
        /// </summary>
        public void RestoreSanity(float amount)
        {
            if (amount <= 0) return;

            float previousSanity = currentSanity;
            currentSanity = Mathf.Min(currentSanity + amount, maxSanity);

            float actualAmount = currentSanity - previousSanity;
            if (actualAmount > 0)
            {
                OnSanityRestored?.Invoke(actualAmount);
                OnSanityChanged?.Invoke(currentSanity, maxSanity);
            }

            // Check if no longer insane
            if (isInsane && currentSanity > insanityThreshold)
            {
                isInsane = false;
                insanityTimer = 0f;
                OnInsanityEnded?.Invoke();
            }
        }

        public void SetSanity(float amount)
        {
            currentSanity = Mathf.Clamp(amount, 0, maxSanity);
            OnSanityChanged?.Invoke(currentSanity, maxSanity);

            // Update insanity state
            if (currentSanity <= insanityThreshold && !isInsane)
            {
                isInsane = true;
                OnInsanityStarted?.Invoke();
            }
            else if (currentSanity > insanityThreshold && isInsane)
            {
                isInsane = false;
                insanityTimer = 0f;
                OnInsanityEnded?.Invoke();
            }
        }

        public void FillSanity()
        {
            RestoreSanity(maxSanity - currentSanity);
        }
    }
}
