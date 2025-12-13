using System;
using UnityEngine;

namespace Streets.Survival
{
    public class HungerSystem : MonoBehaviour
    {
        [Header("Hunger Settings")]
        [SerializeField] private float maxHunger = 100f;
        [SerializeField] private float currentHunger;

        [Header("Depletion Settings")]
        [SerializeField] private float hungerDepletionRate = 1f;
        [SerializeField] private bool depleteWhileIdle = true;
        [SerializeField] private float sprintDepletionMultiplier = 2f;

        [Header("Starvation Settings")]
        [SerializeField] private float starvationThreshold = 0f;
        [SerializeField] private float starvationDamage = 5f;
        [SerializeField] private float starvationDamageInterval = 3f;

        [Header("References")]
        [SerializeField] private HealthSystem healthSystem;

        // State
        private float starvationTimer;
        private bool isStarving;
        private bool isSprinting;

        // Events
        public event Action<float, float> OnHungerChanged; // current, max
        public event Action OnStarvationStarted;
        public event Action OnStarvationEnded;
        public event Action<float> OnFoodConsumed; // amount restored

        // Properties
        public float CurrentHunger => currentHunger;
        public float MaxHunger => maxHunger;
        public float HungerPercent => currentHunger / maxHunger;
        public bool IsStarving => isStarving;
        public bool IsFull => currentHunger >= maxHunger;

        private void Awake()
        {
            currentHunger = maxHunger;
        }

        private void Update()
        {
            DepleteHunger();
            HandleStarvation();
        }

        private void DepleteHunger()
        {
            if (currentHunger <= 0) return;

            float depletionRate = hungerDepletionRate;

            if (isSprinting)
            {
                depletionRate *= sprintDepletionMultiplier;
            }
            else if (!depleteWhileIdle)
            {
                return;
            }

            currentHunger -= depletionRate * Time.deltaTime;
            currentHunger = Mathf.Max(currentHunger, 0);

            OnHungerChanged?.Invoke(currentHunger, maxHunger);

            // Check starvation state
            if (currentHunger <= starvationThreshold && !isStarving)
            {
                isStarving = true;
                OnStarvationStarted?.Invoke();
            }
        }

        private void HandleStarvation()
        {
            if (!isStarving || healthSystem == null) return;

            starvationTimer += Time.deltaTime;

            if (starvationTimer >= starvationDamageInterval)
            {
                healthSystem.TakeDamage(starvationDamage);
                starvationTimer = 0f;
            }
        }

        public void Eat(float amount)
        {
            if (amount <= 0) return;

            float previousHunger = currentHunger;
            currentHunger = Mathf.Min(currentHunger + amount, maxHunger);

            float actualAmount = currentHunger - previousHunger;
            if (actualAmount > 0)
            {
                OnFoodConsumed?.Invoke(actualAmount);
                OnHungerChanged?.Invoke(currentHunger, maxHunger);
            }

            // Check if no longer starving
            if (isStarving && currentHunger > starvationThreshold)
            {
                isStarving = false;
                starvationTimer = 0f;
                OnStarvationEnded?.Invoke();
            }
        }

        public void SetHunger(float amount)
        {
            currentHunger = Mathf.Clamp(amount, 0, maxHunger);
            OnHungerChanged?.Invoke(currentHunger, maxHunger);

            // Update starvation state
            if (currentHunger <= starvationThreshold && !isStarving)
            {
                isStarving = true;
                OnStarvationStarted?.Invoke();
            }
            else if (currentHunger > starvationThreshold && isStarving)
            {
                isStarving = false;
                starvationTimer = 0f;
                OnStarvationEnded?.Invoke();
            }
        }

        public void SetSprinting(bool sprinting)
        {
            isSprinting = sprinting;
        }

        public void FillHunger()
        {
            Eat(maxHunger - currentHunger);
        }
    }
}
