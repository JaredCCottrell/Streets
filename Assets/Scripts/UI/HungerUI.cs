using UnityEngine;
using UnityEngine.UI;
using Streets.Survival;

namespace Streets.UI
{
    public class HungerUI : MonoBehaviour
    {
        [SerializeField] private HungerSystem hungerSystem;
        [SerializeField] private Image hungerFill;

        [Header("Color Settings")]
        [SerializeField] private Color fullColor = new Color(0.8f, 0.5f, 0.2f); // Orange/brown
        [SerializeField] private Color emptyColor = new Color(0.4f, 0.2f, 0.1f); // Dark brown
        [SerializeField] private Color starvingColor = Color.red;

        [Header("Starving Effect")]
        [SerializeField] private float pulseSpeed = 2f;
        private float pulseTimer;

        private void OnEnable()
        {
            if (hungerSystem != null)
            {
                hungerSystem.OnHungerChanged += UpdateHungerBar;
            }
        }

        private void OnDisable()
        {
            if (hungerSystem != null)
            {
                hungerSystem.OnHungerChanged -= UpdateHungerBar;
            }
        }

        private void Start()
        {
            if (hungerSystem != null)
            {
                UpdateHungerBar(hungerSystem.CurrentHunger, hungerSystem.MaxHunger);
            }
        }

        private void Update()
        {
            if (hungerSystem != null && hungerSystem.IsStarving)
            {
                PulseStarvingEffect();
            }
        }

        private void UpdateHungerBar(float current, float max)
        {
            float percent = current / max;

            if (hungerFill != null)
            {
                hungerFill.fillAmount = percent;

                if (!hungerSystem.IsStarving)
                {
                    hungerFill.color = Color.Lerp(emptyColor, fullColor, percent);
                }
            }
        }

        private void PulseStarvingEffect()
        {
            if (hungerFill == null) return;

            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = (Mathf.Sin(pulseTimer) + 1f) / 2f;
            hungerFill.color = Color.Lerp(emptyColor, starvingColor, pulse);
        }
    }
}
