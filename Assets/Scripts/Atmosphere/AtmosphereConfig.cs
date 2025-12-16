using UnityEngine;

namespace Streets.Atmosphere
{
    [CreateAssetMenu(fileName = "AtmosphereConfig", menuName = "Streets/Atmosphere Config")]
    public class AtmosphereConfig : ScriptableObject
    {
        [Header("Fog Settings")]
        public bool enableFog = true;
        public FogMode fogMode = FogMode.ExponentialSquared;
        public Color fogColor = new Color(0.15f, 0.15f, 0.18f);
        [Range(0f, 0.1f)]
        public float fogDensity = 0.02f;
        public float fogStartDistance = 10f;
        public float fogEndDistance = 150f;

        [Header("Lighting")]
        public Color ambientColor = new Color(0.1f, 0.1f, 0.15f);
        [Range(0f, 2f)]
        public float ambientIntensity = 0.3f;
        public Color directionalLightColor = new Color(0.4f, 0.4f, 0.5f);
        [Range(0f, 2f)]
        public float directionalLightIntensity = 0.2f;
        public Vector3 lightDirection = new Vector3(50f, -30f, 0f);

        [Header("Skybox")]
        public Material skyboxMaterial;
        public Color skyTint = new Color(0.2f, 0.2f, 0.25f);
        [Range(0f, 1f)]
        public float skyExposure = 0.5f;

        [Header("Post Processing Overrides")]
        [Range(-1f, 1f)]
        public float colorGradingContrast = 0.2f;
        [Range(-100f, 100f)]
        public float colorGradingSaturation = -20f;
        [Range(0f, 1f)]
        public float vignetteIntensity = 0.35f;
        [Range(0f, 1f)]
        public float filmGrainIntensity = 0.2f;
        [Range(0f, 1f)]
        public float bloomIntensity = 0.5f;

        [Header("Eerie Effects")]
        public bool enableFlicker = true;
        [Range(0f, 1f)]
        public float flickerChance = 0.02f;
        [Range(0f, 1f)]
        public float flickerIntensity = 0.3f;
        public float minFlickerInterval = 5f;
        public float maxFlickerInterval = 30f;

        [Header("Distance Fog Variation")]
        public bool varyFogWithDistance = true;
        public float fogIncreasePerKm = 0.005f;
        public float maxFogDensity = 0.08f;
    }
}
