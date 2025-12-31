using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Streets.Core
{
    /// <summary>
    /// Handles scene transitions with fade effects.
    /// Persists across scenes via DontDestroyOnLoad.
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [Header("Fade Settings")]
        [SerializeField] private float fadeDuration = 1f;
        [SerializeField] private Color fadeColor = Color.black;

        [Header("Loading Screen (Optional)")]
        [SerializeField] private GameObject loadingScreen;
        [SerializeField] private Slider loadingBar;

        // Runtime UI
        private Canvas fadeCanvas;
        private CanvasGroup fadeCanvasGroup;
        private Image fadeImage;

        // State
        private bool isTransitioning = false;

        // Events
        public event Action OnTransitionStarted;
        public event Action OnFadeOutComplete;
        public event Action OnFadeInComplete;
        public event Action<string> OnSceneLoaded;

        public bool IsTransitioning => isTransitioning;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            CreateFadeUI();
        }

        private void CreateFadeUI()
        {
            // Create canvas for fade effect
            GameObject canvasObj = new GameObject("FadeCanvas");
            canvasObj.transform.SetParent(transform);

            fadeCanvas = canvasObj.AddComponent<Canvas>();
            fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            fadeCanvas.sortingOrder = 999; // Always on top

            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            fadeCanvasGroup = canvasObj.AddComponent<CanvasGroup>();
            fadeCanvasGroup.alpha = 0;
            fadeCanvasGroup.blocksRaycasts = false;

            // Create fade image
            GameObject imageObj = new GameObject("FadeImage");
            imageObj.transform.SetParent(canvasObj.transform);

            fadeImage = imageObj.AddComponent<Image>();
            fadeImage.color = fadeColor;

            // Stretch to fill screen
            RectTransform rt = fadeImage.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// Load a scene by name with fade transition
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (isTransitioning)
            {
                Debug.LogWarning("[SceneTransitionManager] Already transitioning!");
                return;
            }

            StartCoroutine(TransitionToScene(sceneName));
        }

        /// <summary>
        /// Load a scene by build index with fade transition
        /// </summary>
        public void LoadScene(int sceneIndex)
        {
            if (isTransitioning)
            {
                Debug.LogWarning("[SceneTransitionManager] Already transitioning!");
                return;
            }

            string sceneName = SceneManager.GetSceneByBuildIndex(sceneIndex).name;
            StartCoroutine(TransitionToScene(sceneName));
        }

        private IEnumerator TransitionToScene(string sceneName)
        {
            isTransitioning = true;
            OnTransitionStarted?.Invoke();

            // Unlock cursor during transition
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Fade out
            yield return StartCoroutine(Fade(0, 1));
            OnFadeOutComplete?.Invoke();

            // Show loading screen if available
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(true);
            }

            // Load scene async
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

                if (loadingBar != null)
                {
                    loadingBar.value = progress;
                }

                // Scene is ready when progress reaches 0.9
                if (asyncLoad.progress >= 0.9f)
                {
                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            // Hide loading screen
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(false);
            }

            OnSceneLoaded?.Invoke(sceneName);

            // Small delay to let scene initialize
            yield return new WaitForSeconds(0.1f);

            // Fade in
            yield return StartCoroutine(Fade(1, 0));
            OnFadeInComplete?.Invoke();

            isTransitioning = false;
        }

        private IEnumerator Fade(float startAlpha, float endAlpha)
        {
            fadeCanvasGroup.blocksRaycasts = endAlpha > 0;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime; // Use unscaled so it works even if time is paused
                float t = elapsed / fadeDuration;
                fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
                yield return null;
            }

            fadeCanvasGroup.alpha = endAlpha;
        }

        /// <summary>
        /// Fade out only (useful for custom transitions)
        /// </summary>
        public void FadeOut(Action onComplete = null)
        {
            StartCoroutine(FadeOutCoroutine(onComplete));
        }

        private IEnumerator FadeOutCoroutine(Action onComplete)
        {
            yield return StartCoroutine(Fade(0, 1));
            onComplete?.Invoke();
        }

        /// <summary>
        /// Fade in only (useful for custom transitions)
        /// </summary>
        public void FadeIn(Action onComplete = null)
        {
            StartCoroutine(FadeInCoroutine(onComplete));
        }

        private IEnumerator FadeInCoroutine(Action onComplete)
        {
            yield return StartCoroutine(Fade(1, 0));
            onComplete?.Invoke();
        }

        /// <summary>
        /// Set fade color at runtime
        /// </summary>
        public void SetFadeColor(Color color)
        {
            fadeColor = color;
            if (fadeImage != null)
            {
                fadeImage.color = color;
            }
        }

        /// <summary>
        /// Set fade duration at runtime
        /// </summary>
        public void SetFadeDuration(float duration)
        {
            fadeDuration = Mathf.Max(0.1f, duration);
        }
    }
}
