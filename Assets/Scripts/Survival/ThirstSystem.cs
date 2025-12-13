using System;
using UnityEngine;

namespace Streets.Survival
{
    public class ThirstSystem : MonoBehaviour
    {
        [Header("Thirst Settings")]
        [SerializeField] private float maxThirst = 100f;
        [SerializeField] private float currentThirst;

        [Header("Depletion Settings")]
        [SerializeField] private float thirstDepletionRate = 1.5f;
        [SerializeField] private bool depleteWhileIdle = true;
        [SerializeField] private float sprintDepletionMultiplier = 2.5f;

        [Header("Dehydration Settings")]
        [SerializeField] private float dehydrationThreshold = 0f;
        [SerializeField] private float dehydrationDamage = 7f;
        [SerializeField] private float dehydrationDamageInterval = 2.5f;

        [Header("References")]
        [SerializeField] private HealthSystem healthSystem;

        // State
        private float dehydrationTimer;
        private bool isDehydrated;
        private bool isSprinting;

        // Events
        public event Action<float, float> OnThirstChanged; // current, max
        public event Action OnDehydrationStarted;
        public event Action OnDehydrationEnded;
        public event Action<float> OnDrinkConsumed; // amount restored

        // Properties
        public float CurrentThirst => currentThirst;
        public float MaxThirst => maxThirst;
        public float ThirstPercent => currentThirst / maxThirst;
        public bool IsDehydrated => isDehydrated;
        public bool IsQuenched => currentThirst >= maxThirst;

        private void Awake()
        {
            currentThirst = maxThirst;
        }

        private void Update()
        {
            DepleteThirst();
            HandleDehydration();
        }

        private void DepleteThirst()
        {
            if (currentThirst <= 0) return;

            float depletionRate = thirstDepletionRate;

            if (isSprinting)
            {
                depletionRate *= sprintDepletionMultiplier;
            }
            else if (!depleteWhileIdle)
            {
                return;
            }

            currentThirst -= depletionRate * Time.deltaTime;
            currentThirst = Mathf.Max(currentThirst, 0);

            OnThirstChanged?.Invoke(currentThirst, maxThirst);

            // Check dehydration state
            if (currentThirst <= dehydrationThreshold && !isDehydrated)
            {
                isDehydrated = true;
                OnDehydrationStarted?.Invoke();
            }
        }

        private void HandleDehydration()
        {
            if (!isDehydrated || healthSystem == null) return;

            dehydrationTimer += Time.deltaTime;

            if (dehydrationTimer >= dehydrationDamageInterval)
            {
                healthSystem.TakeDamage(dehydrationDamage);
                dehydrationTimer = 0f;
            }
        }

        public void Drink(float amount)
        {
            if (amount <= 0) return;

            float previousThirst = currentThirst;
            currentThirst = Mathf.Min(currentThirst + amount, maxThirst);

            float actualAmount = currentThirst - previousThirst;
            if (actualAmount > 0)
            {
                OnDrinkConsumed?.Invoke(actualAmount);
                OnThirstChanged?.Invoke(currentThirst, maxThirst);
            }

            // Check if no longer dehydrated
            if (isDehydrated && currentThirst > dehydrationThreshold)
            {
                isDehydrated = false;
                dehydrationTimer = 0f;
                OnDehydrationEnded?.Invoke();
            }
        }

        public void SetThirst(float amount)
        {
            currentThirst = Mathf.Clamp(amount, 0, maxThirst);
            OnThirstChanged?.Invoke(currentThirst, maxThirst);

            // Update dehydration state
            if (currentThirst <= dehydrationThreshold && !isDehydrated)
            {
                isDehydrated = true;
                OnDehydrationStarted?.Invoke();
            }
            else if (currentThirst > dehydrationThreshold && isDehydrated)
            {
                isDehydrated = false;
                dehydrationTimer = 0f;
                OnDehydrationEnded?.Invoke();
            }
        }

        public void SetSprinting(bool sprinting)
        {
            isSprinting = sprinting;
        }

        public void FillThirst()
        {
            Drink(maxThirst - currentThirst);
        }
    }
}
