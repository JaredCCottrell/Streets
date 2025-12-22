using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Streets.Rendering
{
    /// <summary>
    /// Runtime controller for the pixelation effect.
    /// Attach this to any GameObject to control pixelation at runtime.
    /// </summary>
    public class PixelationController : MonoBehaviour
    {
        [Header("Pixelation Settings")]
        [Tooltip("Size of each pixel block. Higher = more pixelated")]
        [Range(1, 32)]
        [SerializeField] private int pixelSize = 4;

        [Tooltip("Enable/disable the pixelation effect")]
        [SerializeField] private bool effectEnabled = true;

        [Header("Presets")]
        [Tooltip("Quick preset selections for common retro resolutions")]
        [SerializeField] private PixelPreset preset = PixelPreset.Custom;

        private PixelationRenderFeature pixelationFeature;
        private int lastPixelSize;
        private bool lastEnabled;

        public enum PixelPreset
        {
            Custom,         // Use manual pixelSize
            Subtle,         // 2 - barely noticeable
            Light,          // 4 - light pixelation
            Medium,         // 6 - noticeable retro feel
            Heavy,          // 8 - chunky pixels
            Extreme,        // 12 - very chunky
            UltraRetro      // 16 - extremely pixelated
        }

        /// <summary>
        /// Current pixel size
        /// </summary>
        public int PixelSize
        {
            get => pixelSize;
            set
            {
                pixelSize = Mathf.Clamp(value, 1, 32);
                preset = PixelPreset.Custom;
                ApplySettings();
            }
        }

        /// <summary>
        /// Whether the effect is enabled
        /// </summary>
        public bool EffectEnabled
        {
            get => effectEnabled;
            set
            {
                effectEnabled = value;
                ApplySettings();
            }
        }

        private void OnEnable()
        {
            FindPixelationFeature();
            ApplySettings();
        }

        private void OnValidate()
        {
            // Apply preset values when preset changes in inspector
            ApplyPreset();

            // Apply settings if playing
            if (Application.isPlaying)
            {
                ApplySettings();
            }
        }

        private void Update()
        {
            // Check for changes (for inspector tweaking during play mode)
            if (pixelSize != lastPixelSize || effectEnabled != lastEnabled)
            {
                ApplySettings();
            }

            // Debug controls (optional - remove in production)
#if UNITY_EDITOR
            HandleDebugInput();
#endif
        }

        private void HandleDebugInput()
        {
            // [ and ] keys to adjust pixel size
            if (UnityEngine.Input.GetKeyDown(KeyCode.LeftBracket))
            {
                PixelSize = Mathf.Max(1, pixelSize - 1);
                Debug.Log($"Pixelation: {pixelSize}");
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.RightBracket))
            {
                PixelSize = Mathf.Min(32, pixelSize + 1);
                Debug.Log($"Pixelation: {pixelSize}");
            }
            // P key to toggle effect
            if (UnityEngine.Input.GetKeyDown(KeyCode.P))
            {
                EffectEnabled = !effectEnabled;
                Debug.Log($"Pixelation: {(effectEnabled ? "ON" : "OFF")}");
            }
        }

        private void FindPixelationFeature()
        {
            // Find the PixelationRenderFeature in the current URP renderer
            var urpAsset = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urpAsset == null)
            {
                Debug.LogWarning("PixelationController: No URP asset found!");
                return;
            }

            // Get the renderer data via reflection (Unity doesn't expose this directly)
            var propertyInfo = urpAsset.GetType().GetProperty("scriptableRendererData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (propertyInfo != null)
            {
                var rendererData = propertyInfo.GetValue(urpAsset) as ScriptableRendererData;
                if (rendererData != null)
                {
                    foreach (var feature in rendererData.rendererFeatures)
                    {
                        if (feature is PixelationRenderFeature pf)
                        {
                            pixelationFeature = pf;
                            return;
                        }
                    }
                }
            }

            Debug.LogWarning("PixelationController: PixelationRenderFeature not found in URP Renderer. " +
                           "Add it via the PC_Renderer asset in Project Settings.");
        }

        private void ApplyPreset()
        {
            switch (preset)
            {
                case PixelPreset.Subtle:
                    pixelSize = 2;
                    break;
                case PixelPreset.Light:
                    pixelSize = 4;
                    break;
                case PixelPreset.Medium:
                    pixelSize = 6;
                    break;
                case PixelPreset.Heavy:
                    pixelSize = 8;
                    break;
                case PixelPreset.Extreme:
                    pixelSize = 12;
                    break;
                case PixelPreset.UltraRetro:
                    pixelSize = 16;
                    break;
                case PixelPreset.Custom:
                default:
                    // Keep current pixelSize
                    break;
            }
        }

        private void ApplySettings()
        {
            if (pixelationFeature != null)
            {
                pixelationFeature.SetPixelSize(pixelSize);
                pixelationFeature.SetEnabled(effectEnabled);
            }

            lastPixelSize = pixelSize;
            lastEnabled = effectEnabled;
        }

        /// <summary>
        /// Set a preset at runtime
        /// </summary>
        public void SetPreset(PixelPreset newPreset)
        {
            preset = newPreset;
            ApplyPreset();
            ApplySettings();
        }
    }
}
