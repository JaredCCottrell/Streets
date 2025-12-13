using UnityEngine;
using UnityEngine.UI;
using Streets.Survival;

namespace Streets.UI
{
    public class HealthUI : MonoBehaviour
    {
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private Image healthFill;
        [SerializeField] private Image damageFill;
        [SerializeField] private float damageFillSpeed = 2f;

        [Header("Color Settings")]
        [SerializeField] private Color healthyColor = Color.green;
        [SerializeField] private Color damagedColor = Color.yellow;
        [SerializeField] private Color criticalColor = Color.red;
        [SerializeField] private float damagedThreshold = 0.5f;
        [SerializeField] private float criticalThreshold = 0.25f;

        [Header("Damage Flash")]
        [SerializeField] private Image damageFlashOverlay;
        [SerializeField] private float flashDuration = 0.2f;
        [SerializeField] private Color flashColor = new Color(1f, 0f, 0f, 0.3f);

        private float flashTimer;

        private void OnEnable()
        {
            if (healthSystem != null)
            {
                healthSystem.OnHealthChanged += UpdateHealthBar;
                healthSystem.OnDamageTaken += OnDamage;
            }
        }

        private void OnDisable()
        {
            if (healthSystem != null)
            {
                healthSystem.OnHealthChanged -= UpdateHealthBar;
                healthSystem.OnDamageTaken -= OnDamage;
            }
        }

        private void Start()
        {
            if (healthSystem != null)
            {
                UpdateHealthBar(healthSystem.CurrentHealth, healthSystem.MaxHealth);
            }

            if (damageFill != null)
            {
                damageFill.fillAmount = 1f;
            }
        }

        private void Update()
        {
            UpdateDamageFill();
            UpdateDamageFlash();
        }

        private void UpdateHealthBar(float current, float max)
        {
            float percent = current / max;

            if (healthFill != null)
            {
                healthFill.fillAmount = percent;
                healthFill.color = GetHealthColor(percent);
            }
        }

        private void UpdateDamageFill()
        {
            if (damageFill == null || healthFill == null) return;

            if (damageFill.fillAmount > healthFill.fillAmount)
            {
                damageFill.fillAmount = Mathf.MoveTowards(
                    damageFill.fillAmount,
                    healthFill.fillAmount,
                    damageFillSpeed * Time.deltaTime
                );
            }
            else
            {
                damageFill.fillAmount = healthFill.fillAmount;
            }
        }

        private void UpdateDamageFlash()
        {
            if (damageFlashOverlay == null) return;

            if (flashTimer > 0)
            {
                flashTimer -= Time.deltaTime;
                float alpha = (flashTimer / flashDuration) * flashColor.a;
                damageFlashOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            }
            else
            {
                damageFlashOverlay.color = Color.clear;
            }
        }

        private void OnDamage(float damage)
        {
            flashTimer = flashDuration;
        }

        private Color GetHealthColor(float percent)
        {
            if (percent <= criticalThreshold)
            {
                return criticalColor;
            }
            else if (percent <= damagedThreshold)
            {
                return Color.Lerp(criticalColor, damagedColor,
                    (percent - criticalThreshold) / (damagedThreshold - criticalThreshold));
            }
            else
            {
                return Color.Lerp(damagedColor, healthyColor,
                    (percent - damagedThreshold) / (1f - damagedThreshold));
            }
        }
    }
}
