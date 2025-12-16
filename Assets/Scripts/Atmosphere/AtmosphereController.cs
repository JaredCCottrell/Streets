using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Streets.Road;

namespace Streets.Atmosphere
{
    public class AtmosphereController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private AtmosphereConfig config;

        [Header("References")]
        [SerializeField] private Light directionalLight;
        [SerializeField] private Volume postProcessVolume;
        [SerializeField] private RoadGenerator roadGenerator;

        [Header("Runtime State")]
        [SerializeField] private bool isInitialized;

        // Post-processing components
        private ColorAdjustments colorAdjustments;
        private Vignette vignette;
        private Bloom bloom;
        private FilmGrain filmGrain;

        // Flicker state
        private float nextFlickerTime;
        private float originalLightIntensity;
        private bool isFlickering;
        private float flickerEndTime;

        // Distance tracking
        private float currentFogDensity;

        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (config == null)
            {
                Debug.LogWarning("[AtmosphereController] No config assigned!");
                return;
            }

            ApplyFogSettings();
            ApplyLightingSettings();
            ApplySkyboxSettings();
            SetupPostProcessing();

            if (config.enableFlicker)
            {
                ScheduleNextFlicker();
            }

            isInitialized = true;
            Debug.Log("[AtmosphereController] Atmosphere initialized");
        }

        private void Update()
        {
            if (!isInitialized || config == null) return;

            // Handle light flicker
            if (config.enableFlicker)
            {
                UpdateFlicker();
            }

            // Vary fog with distance traveled
            if (config.varyFogWithDistance && roadGenerator != null)
            {
                UpdateDistanceFog();
            }
        }

        private void ApplyFogSettings()
        {
            RenderSettings.fog = config.enableFog;
            RenderSettings.fogMode = config.fogMode;
            RenderSettings.fogColor = config.fogColor;
            RenderSettings.fogDensity = config.fogDensity;
            RenderSettings.fogStartDistance = config.fogStartDistance;
            RenderSettings.fogEndDistance = config.fogEndDistance;

            currentFogDensity = config.fogDensity;
        }

        private void ApplyLightingSettings()
        {
            // Ambient lighting
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = config.ambientColor * config.ambientIntensity;

            // Directional light
            if (directionalLight != null)
            {
                directionalLight.color = config.directionalLightColor;
                directionalLight.intensity = config.directionalLightIntensity;
                directionalLight.transform.rotation = Quaternion.Euler(config.lightDirection);
                originalLightIntensity = config.directionalLightIntensity;
            }
            else
            {
                // Try to find main light
                directionalLight = FindDirectionalLight();
                if (directionalLight != null)
                {
                    directionalLight.color = config.directionalLightColor;
                    directionalLight.intensity = config.directionalLightIntensity;
                    directionalLight.transform.rotation = Quaternion.Euler(config.lightDirection);
                    originalLightIntensity = config.directionalLightIntensity;
                }
            }
        }

        private void ApplySkyboxSettings()
        {
            if (config.skyboxMaterial != null)
            {
                RenderSettings.skybox = config.skyboxMaterial;

                // Apply tint and exposure if material supports it
                if (config.skyboxMaterial.HasProperty("_Tint"))
                {
                    config.skyboxMaterial.SetColor("_Tint", config.skyTint);
                }
                if (config.skyboxMaterial.HasProperty("_Exposure"))
                {
                    config.skyboxMaterial.SetFloat("_Exposure", config.skyExposure);
                }
            }
            else
            {
                // Create a simple dark gradient skybox color
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientSkyColor = config.skyTint;
            }
        }

        private void SetupPostProcessing()
        {
            if (postProcessVolume == null)
            {
                // Try to find global volume
                postProcessVolume = FindObjectOfType<Volume>();
            }

            if (postProcessVolume == null || postProcessVolume.profile == null)
            {
                Debug.LogWarning("[AtmosphereController] No post-processing volume found");
                return;
            }

            VolumeProfile profile = postProcessVolume.profile;

            // Color Adjustments
            if (profile.TryGet(out colorAdjustments))
            {
                colorAdjustments.contrast.Override(config.colorGradingContrast * 100f);
                colorAdjustments.saturation.Override(config.colorGradingSaturation);
            }

            // Vignette
            if (profile.TryGet(out vignette))
            {
                vignette.intensity.Override(config.vignetteIntensity);
            }

            // Bloom
            if (profile.TryGet(out bloom))
            {
                bloom.intensity.Override(config.bloomIntensity);
            }

            // Film Grain
            if (profile.TryGet(out filmGrain))
            {
                filmGrain.intensity.Override(config.filmGrainIntensity);
            }
        }

        private void UpdateFlicker()
        {
            if (directionalLight == null) return;

            if (isFlickering)
            {
                // During flicker - rapidly vary intensity
                float flicker = 1f + Random.Range(-config.flickerIntensity, config.flickerIntensity);
                directionalLight.intensity = originalLightIntensity * flicker;

                // Also flicker ambient slightly
                float ambientFlicker = 1f + Random.Range(-config.flickerIntensity * 0.5f, config.flickerIntensity * 0.5f);
                RenderSettings.ambientLight = config.ambientColor * config.ambientIntensity * ambientFlicker;

                if (Time.time >= flickerEndTime)
                {
                    // End flicker
                    isFlickering = false;
                    directionalLight.intensity = originalLightIntensity;
                    RenderSettings.ambientLight = config.ambientColor * config.ambientIntensity;
                    ScheduleNextFlicker();
                }
            }
            else if (Time.time >= nextFlickerTime)
            {
                // Random chance to start flicker
                if (Random.value < config.flickerChance)
                {
                    StartFlicker();
                }
                else
                {
                    ScheduleNextFlicker();
                }
            }
        }

        private void StartFlicker()
        {
            isFlickering = true;
            flickerEndTime = Time.time + Random.Range(0.1f, 0.5f);
        }

        private void ScheduleNextFlicker()
        {
            nextFlickerTime = Time.time + Random.Range(config.minFlickerInterval, config.maxFlickerInterval);
        }

        private void UpdateDistanceFog()
        {
            float distanceKm = roadGenerator.TotalDistanceTraveled / 1000f;
            float targetDensity = Mathf.Min(
                config.fogDensity + (distanceKm * config.fogIncreasePerKm),
                config.maxFogDensity
            );

            currentFogDensity = Mathf.Lerp(currentFogDensity, targetDensity, Time.deltaTime * 0.1f);
            RenderSettings.fogDensity = currentFogDensity;
        }

        private Light FindDirectionalLight()
        {
            Light[] lights = FindObjectsOfType<Light>();
            foreach (var light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    return light;
                }
            }
            return null;
        }

        // Public methods for runtime atmosphere changes

        public void SetFogDensity(float density)
        {
            currentFogDensity = density;
            RenderSettings.fogDensity = density;
        }

        public void SetFogColor(Color color)
        {
            RenderSettings.fogColor = color;
        }

        public void TriggerFlicker()
        {
            if (!isFlickering)
            {
                StartFlicker();
            }
        }

        public void SetAmbientIntensity(float intensity)
        {
            RenderSettings.ambientLight = config.ambientColor * intensity;
        }

        public void PulseDarkness(float duration, float intensity)
        {
            StartCoroutine(DarknessPulseCoroutine(duration, intensity));
        }

        private System.Collections.IEnumerator DarknessPulseCoroutine(float duration, float intensity)
        {
            float originalAmbient = config.ambientIntensity;
            float elapsed = 0f;

            // Fade to dark
            while (elapsed < duration * 0.3f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (duration * 0.3f);
                SetAmbientIntensity(Mathf.Lerp(originalAmbient, intensity, t));
                yield return null;
            }

            // Hold
            yield return new WaitForSeconds(duration * 0.4f);

            // Fade back
            elapsed = 0f;
            while (elapsed < duration * 0.3f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (duration * 0.3f);
                SetAmbientIntensity(Mathf.Lerp(intensity, originalAmbient, t));
                yield return null;
            }

            SetAmbientIntensity(originalAmbient);
        }

#if UNITY_EDITOR
        [ContextMenu("Apply Settings Now")]
        public void EditorApplySettings()
        {
            if (config != null)
            {
                ApplyFogSettings();
                ApplyLightingSettings();
                ApplySkyboxSettings();
                Debug.Log("[AtmosphereController] Settings applied in editor");
            }
        }

        private void OnValidate()
        {
            // Auto-apply in editor when config changes
            if (config != null && !Application.isPlaying)
            {
                ApplyFogSettings();
            }
        }
#endif
    }
}
