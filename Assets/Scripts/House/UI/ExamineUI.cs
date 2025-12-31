using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Streets.House.UI
{
    /// <summary>
    /// UI for examining items, notes, and documents.
    /// Displays title, text content, and optional images.
    /// </summary>
    public class ExamineUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private Image examineImage;
        [SerializeField] private GameObject imageContainer;

        [Header("Close Hint")]
        [SerializeField] private TextMeshProUGUI closeHintText;
        [SerializeField] private string closeHintMessage = "Press any key to close";

        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.2f;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;

        private bool isVisible = false;
        private float fadeTimer = 0f;

        private void Awake()
        {
            // Hide panel initially
            if (panel != null)
            {
                panel.SetActive(false);
            }

            // Get or add canvas group
            if (canvasGroup == null && panel != null)
            {
                canvasGroup = panel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = panel.AddComponent<CanvasGroup>();
                }
            }
        }

        private void Update()
        {
            if (!isVisible) return;

            // Fade in animation
            if (fadeTimer < fadeInDuration)
            {
                fadeTimer += Time.unscaledDeltaTime;
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = Mathf.Clamp01(fadeTimer / fadeInDuration);
                }
            }
        }

        /// <summary>
        /// Show the examine UI with content
        /// </summary>
        public void Show(string title, string body, Sprite image = null)
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }

            // Set content
            if (titleText != null)
            {
                titleText.text = title;
            }

            if (bodyText != null)
            {
                bodyText.text = body;
            }

            // Handle image
            if (examineImage != null && imageContainer != null)
            {
                if (image != null)
                {
                    examineImage.sprite = image;
                    imageContainer.SetActive(true);
                }
                else
                {
                    imageContainer.SetActive(false);
                }
            }

            // Set close hint
            if (closeHintText != null)
            {
                closeHintText.text = closeHintMessage;
            }

            // Start fade in
            fadeTimer = 0f;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            isVisible = true;

            // Play sound
            PlaySound(openSound);
        }

        /// <summary>
        /// Hide the examine UI
        /// </summary>
        public void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }

            isVisible = false;

            // Play sound
            PlaySound(closeSound);
        }

        /// <summary>
        /// Check if UI is currently visible
        /// </summary>
        public bool IsVisible => isVisible;

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// Update just the body text (for dynamic content)
        /// </summary>
        public void SetBodyText(string text)
        {
            if (bodyText != null)
            {
                bodyText.text = text;
            }
        }

        /// <summary>
        /// Append text to body (for revealing clues progressively)
        /// </summary>
        public void AppendText(string text)
        {
            if (bodyText != null)
            {
                bodyText.text += text;
            }
        }
    }
}
