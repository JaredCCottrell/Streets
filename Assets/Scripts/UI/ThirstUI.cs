using UnityEngine;
using UnityEngine.UI;
using Streets.Survival;

namespace Streets.UI
{
    public class ThirstUI : MonoBehaviour
    {
        [SerializeField] private ThirstSystem thirstSystem;
        [SerializeField] private Image thirstFill;

        [Header("Color Settings")]
        [SerializeField] private Color fullColor = new Color(0.2f, 0.6f, 1f); // Light blue
        [SerializeField] private Color emptyColor = new Color(0.1f, 0.2f, 0.4f); // Dark blue
        [SerializeField] private Color dehydratedColor = Color.red;

        [Header("Dehydrated Effect")]
        [SerializeField] private float pulseSpeed = 2f;
        private float pulseTimer;

        private void OnEnable()
        {
            if (thirstSystem != null)
            {
                thirstSystem.OnThirstChanged += UpdateThirstBar;
            }
        }

        private void OnDisable()
        {
            if (thirstSystem != null)
            {
                thirstSystem.OnThirstChanged -= UpdateThirstBar;
            }
        }

        private void Start()
        {
            if (thirstSystem != null)
            {
                UpdateThirstBar(thirstSystem.CurrentThirst, thirstSystem.MaxThirst);
            }
        }

        private void Update()
        {
            if (thirstSystem != null && thirstSystem.IsDehydrated)
            {
                PulseDehydratedEffect();
            }
        }

        private void UpdateThirstBar(float current, float max)
        {
            float percent = current / max;

            if (thirstFill != null)
            {
                thirstFill.fillAmount = percent;

                if (!thirstSystem.IsDehydrated)
                {
                    thirstFill.color = Color.Lerp(emptyColor, fullColor, percent);
                }
            }
        }

        private void PulseDehydratedEffect()
        {
            if (thirstFill == null) return;

            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = (Mathf.Sin(pulseTimer) + 1f) / 2f;
            thirstFill.color = Color.Lerp(emptyColor, dehydratedColor, pulse);
        }
    }
}
