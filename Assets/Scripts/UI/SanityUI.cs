using UnityEngine;
using UnityEngine.UI;
using Streets.Survival;

namespace Streets.UI
{
    public class SanityUI : MonoBehaviour
    {
        [SerializeField] private SanitySystem sanitySystem;
        [SerializeField] private Image sanityFill;

        [Header("Color Settings")]
        [SerializeField] private Color saneColor = new Color(0.8f, 0.4f, 0.9f); // Purple
        [SerializeField] private Color lowColor = new Color(0.4f, 0.1f, 0.5f); // Dark purple
        [SerializeField] private Color insaneColor = Color.red;

        [Header("Insane Effect")]
        [SerializeField] private float pulseSpeed = 3f;
        private float pulseTimer;

        private void OnEnable()
        {
            if (sanitySystem != null)
            {
                sanitySystem.OnSanityChanged += UpdateSanityBar;
            }
        }

        private void OnDisable()
        {
            if (sanitySystem != null)
            {
                sanitySystem.OnSanityChanged -= UpdateSanityBar;
            }
        }

        private void Start()
        {
            if (sanitySystem != null)
            {
                UpdateSanityBar(sanitySystem.CurrentSanity, sanitySystem.MaxSanity);
            }
        }

        private void Update()
        {
            if (sanitySystem != null && sanitySystem.IsInsane)
            {
                PulseInsaneEffect();
            }
        }

        private void UpdateSanityBar(float current, float max)
        {
            float percent = current / max;

            if (sanityFill != null)
            {
                sanityFill.fillAmount = percent;

                if (!sanitySystem.IsInsane)
                {
                    sanityFill.color = Color.Lerp(lowColor, saneColor, percent);
                }
            }
        }

        private void PulseInsaneEffect()
        {
            if (sanityFill == null) return;

            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = (Mathf.Sin(pulseTimer) + 1f) / 2f;
            sanityFill.color = Color.Lerp(lowColor, insaneColor, pulse);
        }
    }
}
