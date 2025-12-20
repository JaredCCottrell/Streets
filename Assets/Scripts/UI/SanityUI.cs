using UnityEngine;
using UnityEngine.UI;
using Streets.Survival;

namespace Streets.UI
{
    public class SanityUI : MonoBehaviour
    {
        [SerializeField] private SanitySystem sanitySystem;
        [SerializeField] private Image sanityFill;
        [SerializeField] private Image sanityBackground;

        [Header("Insane Effect")]
        [SerializeField] private float pulseSpeed = 3f;
        [SerializeField] private float minAlpha = 0.5f;
        [SerializeField] private float maxAlpha = 1f;
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
            else if (sanityFill != null)
            {
                // Reset to full alpha when not insane
                Color c = sanityFill.color;
                c.a = 1f;
                sanityFill.color = c;
            }
        }

        private void UpdateSanityBar(float current, float max)
        {
            float percent = current / max;

            if (sanityFill != null)
            {
                sanityFill.fillAmount = percent;
            }
        }

        private void PulseInsaneEffect()
        {
            if (sanityFill == null) return;

            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = (Mathf.Sin(pulseTimer) + 1f) / 2f;
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, pulse);

            Color c = sanityFill.color;
            c.a = alpha;
            sanityFill.color = c;
        }
    }
}
