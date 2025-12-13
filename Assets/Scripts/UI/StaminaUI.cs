using UnityEngine;
using UnityEngine.UI;
using Streets.Player;

namespace Streets.UI
{
    public class StaminaUI : MonoBehaviour
    {
        [SerializeField] private FirstPersonController playerController;
        [SerializeField] private Image staminaFill;
        [SerializeField] private float fadeSpeed = 2f;
        [SerializeField] private float hideDelay = 2f;
        [SerializeField] private CanvasGroup canvasGroup;

        private float hideTimer;
        private float lastStaminaPercent = 1f;

        private void Update()
        {
            if (playerController == null) return;

            float currentPercent = playerController.StaminaPercent;
            staminaFill.fillAmount = currentPercent;

            // Show UI when stamina changes or is not full
            if (Mathf.Abs(currentPercent - lastStaminaPercent) > 0.001f || currentPercent < 0.99f)
            {
                hideTimer = 0f;
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 1f, fadeSpeed * Time.deltaTime);
            }
            else
            {
                hideTimer += Time.deltaTime;
                if (hideTimer >= hideDelay)
                {
                    canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, fadeSpeed * Time.deltaTime);
                }
            }

            lastStaminaPercent = currentPercent;
        }
    }
}
