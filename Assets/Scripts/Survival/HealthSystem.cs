using System;
using UnityEngine;

namespace Streets.Survival
{
    public class HealthSystem : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;

        [Header("Damage Settings")]
        [SerializeField] private float invincibilityDuration = 0.5f;

        [Header("Regeneration Settings")]
        [SerializeField] private bool canRegenerate = false;
        [SerializeField] private float regenRate = 5f;
        [SerializeField] private float regenDelay = 5f;

        // State
        private float timeSinceLastDamage;
        private bool isInvincible;
        private float invincibilityTimer;

        // Events for other systems to respond to
        public event Action<float, float> OnHealthChanged; // current, max
        public event Action<float> OnDamageTaken; // damage amount
        public event Action<float> OnHealed; // heal amount
        public event Action OnDeath;

        // Properties
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float HealthPercent => currentHealth / maxHealth;
        public bool IsAlive => currentHealth > 0;
        public bool IsFullHealth => currentHealth >= maxHealth;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        private void Update()
        {
            HandleInvincibility();
            HandleRegeneration();
        }

        private void HandleInvincibility()
        {
            if (isInvincible)
            {
                invincibilityTimer -= Time.deltaTime;
                if (invincibilityTimer <= 0)
                {
                    isInvincible = false;
                }
            }
        }

        private void HandleRegeneration()
        {
            if (!canRegenerate || !IsAlive || IsFullHealth) return;

            timeSinceLastDamage += Time.deltaTime;

            if (timeSinceLastDamage >= regenDelay)
            {
                Heal(regenRate * Time.deltaTime);
            }
        }

        public void TakeDamage(float damage)
        {
            if (!IsAlive || isInvincible || damage <= 0) return;

            currentHealth -= damage;
            timeSinceLastDamage = 0f;

            // Trigger invincibility frames
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;

            OnDamageTaken?.Invoke(damage);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0) return;

            float previousHealth = currentHealth;
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

            float actualHeal = currentHealth - previousHealth;
            if (actualHeal > 0)
            {
                OnHealed?.Invoke(actualHeal);
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
            }
        }

        public void SetHealth(float amount)
        {
            currentHealth = Mathf.Clamp(amount, 0, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void SetMaxHealth(float newMax, bool healToFull = false)
        {
            maxHealth = Mathf.Max(1, newMax);

            if (healToFull)
            {
                currentHealth = maxHealth;
            }
            else
            {
                currentHealth = Mathf.Min(currentHealth, maxHealth);
            }

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void FullHeal()
        {
            Heal(maxHealth - currentHealth);
        }

        private void Die()
        {
            OnDeath?.Invoke();
        }

        public void Revive(float healthPercent = 1f)
        {
            currentHealth = maxHealth * Mathf.Clamp01(healthPercent);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}
