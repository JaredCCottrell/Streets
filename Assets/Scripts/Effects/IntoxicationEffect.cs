using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Streets.Effects
{
    /// <summary>
    /// Applies visual effects based on intoxication level.
    /// Uses URP post-processing volume to create drunk effects.
    /// </summary>
    public class IntoxicationEffect : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private IntoxicationSystem intoxicationSystem;
        [SerializeField] private Volume postProcessVolume;
        [SerializeField] private Transform cameraTransform;

        [Header("Chromatic Aberration")]
        [SerializeField] private float maxChromaticAberration = 1f;
        [SerializeField] private AnimationCurve chromaticCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Lens Distortion")]
        [SerializeField] private float maxLensDistortion = -0.5f;
        [SerializeField] private float lensDistortionWobbleSpeed = 2f;
        [SerializeField] private float lensDistortionWobbleAmount = 0.1f;

        [Header("Vignette")]
        [SerializeField] private float maxVignetteIntensity = 0.5f;
        [SerializeField] private Color vignetteColor = new Color(0.1f, 0.05f, 0.1f);

        [Header("Color Grading")]
        [SerializeField] private float maxSaturationBoost = 20f;
        [SerializeField] private float maxContrastReduction = -10f;

        [Header("Camera Wobble")]
        [SerializeField] private float maxWobbleAmount = 2f;
        [SerializeField] private float wobbleSpeed = 1.5f;
        [SerializeField] private float wobbleRandomness = 0.5f;

        [Header("Blur (Screen Space)")]
        [SerializeField] private bool useBlur = true;
        [SerializeField] private float maxBlurIntensity = 0.3f;

        // Post-processing overrides
        private ChromaticAberration chromaticAberration;
        private LensDistortion lensDistortion;
        private Vignette vignette;
        private ColorAdjustments colorAdjustments;
        private MotionBlur motionBlur;

        // Camera wobble state
        private float wobbleTime;
        private float wobblePhaseX;
        private float wobblePhaseY;
        private Vector3 currentWobbleOffset;
        private Vector3 currentRotationOffset;
        private Vector3 appliedPositionOffset;
        private Quaternion appliedRotationOffset = Quaternion.identity;

        private void Start()
        {
            // Find intoxication system if not assigned
            if (intoxicationSystem == null)
            {
                intoxicationSystem = FindObjectOfType<IntoxicationSystem>();
            }

            // Find camera if not assigned
            if (cameraTransform == null)
            {
                cameraTransform = Camera.main?.transform;
            }

            // Initialize random phases for wobble
            wobblePhaseX = Random.Range(0f, Mathf.PI * 2f);
            wobblePhaseY = Random.Range(0f, Mathf.PI * 2f);

            // Create or get post-processing volume
            SetupPostProcessing();
        }

        private void SetupPostProcessing()
        {
            // Try to find existing volume
            if (postProcessVolume == null)
            {
                postProcessVolume = FindObjectOfType<Volume>();
            }

            // Create a new volume if needed
            if (postProcessVolume == null)
            {
                GameObject volumeObj = new GameObject("IntoxicationVolume");
                volumeObj.transform.SetParent(transform);
                postProcessVolume = volumeObj.AddComponent<Volume>();
                postProcessVolume.isGlobal = true;
                postProcessVolume.priority = 100; // High priority to override other volumes
                postProcessVolume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
            }

            // Get or add effects to the profile
            VolumeProfile profile = postProcessVolume.profile;

            if (!profile.TryGet(out chromaticAberration))
            {
                chromaticAberration = profile.Add<ChromaticAberration>(true);
            }

            if (!profile.TryGet(out lensDistortion))
            {
                lensDistortion = profile.Add<LensDistortion>(true);
            }

            if (!profile.TryGet(out vignette))
            {
                vignette = profile.Add<Vignette>(true);
            }

            if (!profile.TryGet(out colorAdjustments))
            {
                colorAdjustments = profile.Add<ColorAdjustments>(true);
            }

            if (!profile.TryGet(out motionBlur))
            {
                motionBlur = profile.Add<MotionBlur>(true);
            }

            // Initialize all overrides
            chromaticAberration.intensity.overrideState = true;
            lensDistortion.intensity.overrideState = true;
            vignette.intensity.overrideState = true;
            vignette.color.overrideState = true;
            colorAdjustments.saturation.overrideState = true;
            colorAdjustments.contrast.overrideState = true;
            motionBlur.intensity.overrideState = true;
        }

        private void Update()
        {
            if (intoxicationSystem == null) return;

            float intoxPercent = intoxicationSystem.IntoxicationPercent;

            UpdatePostProcessing(intoxPercent);
            CalculateWobble(intoxPercent);
        }

        private void LateUpdate()
        {
            // Apply wobble in LateUpdate so it happens after player look input
            ApplyCameraWobble();
        }

        private void UpdatePostProcessing(float intoxPercent)
        {
            if (postProcessVolume == null) return;

            float curvedIntox = chromaticCurve.Evaluate(intoxPercent);

            // Chromatic Aberration - color fringing
            if (chromaticAberration != null)
            {
                chromaticAberration.intensity.value = curvedIntox * maxChromaticAberration;
            }

            // Lens Distortion - wavey edges with wobble
            if (lensDistortion != null)
            {
                float wobble = Mathf.Sin(Time.time * lensDistortionWobbleSpeed) * lensDistortionWobbleAmount;
                float distortion = curvedIntox * maxLensDistortion;
                lensDistortion.intensity.value = distortion + (wobble * curvedIntox);
            }

            // Vignette - darkened edges
            if (vignette != null)
            {
                vignette.intensity.value = curvedIntox * maxVignetteIntensity;
                vignette.color.value = vignetteColor;
            }

            // Color Adjustments - boosted saturation, reduced contrast
            if (colorAdjustments != null)
            {
                colorAdjustments.saturation.value = curvedIntox * maxSaturationBoost;
                colorAdjustments.contrast.value = curvedIntox * maxContrastReduction;
            }

            // Motion Blur
            if (motionBlur != null && useBlur)
            {
                motionBlur.intensity.value = curvedIntox * maxBlurIntensity;
            }
        }

        private void CalculateWobble(float intoxPercent)
        {
            if (intoxPercent < 0.01f)
            {
                // No wobble when sober - smoothly return to zero
                currentWobbleOffset = Vector3.Lerp(currentWobbleOffset, Vector3.zero, Time.deltaTime * 5f);
                currentRotationOffset = Vector3.Lerp(currentRotationOffset, Vector3.zero, Time.deltaTime * 5f);
                return;
            }

            wobbleTime += Time.deltaTime * wobbleSpeed;

            // Add some randomness to the wobble
            float randomOffset = Mathf.PerlinNoise(wobbleTime * wobbleRandomness, 0f) * 2f - 1f;

            // Calculate rotation wobble offsets using sine waves with different phases
            float wobbleX = Mathf.Sin(wobbleTime + wobblePhaseX) * maxWobbleAmount * intoxPercent;
            float wobbleY = Mathf.Sin(wobbleTime * 0.7f + wobblePhaseY + randomOffset) * maxWobbleAmount * intoxPercent * 0.5f;

            currentRotationOffset = new Vector3(wobbleY, 0f, wobbleX * 0.3f);

            // Subtle position sway
            currentWobbleOffset = new Vector3(
                Mathf.Sin(wobbleTime * 0.5f) * 0.02f * intoxPercent,
                Mathf.Sin(wobbleTime * 0.3f + 1f) * 0.01f * intoxPercent,
                0f
            );
        }

        private void ApplyCameraWobble()
        {
            if (cameraTransform == null) return;

            // First, undo the previous frame's wobble
            cameraTransform.localPosition -= appliedPositionOffset;
            cameraTransform.localRotation = cameraTransform.localRotation * Quaternion.Inverse(appliedRotationOffset);

            // Calculate new rotation offset as quaternion
            appliedRotationOffset = Quaternion.Euler(currentRotationOffset);
            appliedPositionOffset = currentWobbleOffset;

            // Apply new wobble as additive offset
            // This preserves the player's look direction
            cameraTransform.localPosition += appliedPositionOffset;
            cameraTransform.localRotation = cameraTransform.localRotation * appliedRotationOffset;
        }

        private void OnDisable()
        {
            // Reset effects when disabled
            if (chromaticAberration != null) chromaticAberration.intensity.value = 0;
            if (lensDistortion != null) lensDistortion.intensity.value = 0;
            if (vignette != null) vignette.intensity.value = 0;
            if (colorAdjustments != null)
            {
                colorAdjustments.saturation.value = 0;
                colorAdjustments.contrast.value = 0;
            }
            if (motionBlur != null) motionBlur.intensity.value = 0;

            // Remove any applied wobble from camera
            if (cameraTransform != null)
            {
                cameraTransform.localPosition -= appliedPositionOffset;
                cameraTransform.localRotation = cameraTransform.localRotation * Quaternion.Inverse(appliedRotationOffset);
            }

            // Reset wobble offsets
            currentWobbleOffset = Vector3.zero;
            currentRotationOffset = Vector3.zero;
            appliedPositionOffset = Vector3.zero;
            appliedRotationOffset = Quaternion.identity;
        }
    }
}
