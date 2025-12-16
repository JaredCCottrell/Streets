#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Streets.Atmosphere;

namespace Streets.Editor
{
    public class AtmosphereSetupTool : EditorWindow
    {
        [MenuItem("Streets/Setup Atmosphere")]
        public static void ShowWindow()
        {
            GetWindow<AtmosphereSetupTool>("Atmosphere Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Eerie Atmosphere Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This tool sets up the eerie atmosphere for Streets:\n" +
                "- Creates atmosphere config with horror settings\n" +
                "- Configures post-processing volume\n" +
                "- Sets up fog and lighting\n" +
                "- Adds AtmosphereController to scene",
                MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("1. Create Atmosphere Config", GUILayout.Height(30)))
            {
                CreateAtmosphereConfig();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("2. Setup Post-Processing Volume", GUILayout.Height(30)))
            {
                SetupPostProcessingVolume();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("3. Add Atmosphere Controller to Scene", GUILayout.Height(30)))
            {
                AddAtmosphereController();
            }

            GUILayout.Space(20);

            if (GUILayout.Button("Do All Steps", GUILayout.Height(40)))
            {
                CreateAtmosphereConfig();
                SetupPostProcessingVolume();
                AddAtmosphereController();
                EditorUtility.DisplayDialog("Complete", "Eerie atmosphere has been set up!", "OK");
            }

            GUILayout.Space(20);
            EditorGUILayout.HelpBox(
                "After setup:\n" +
                "1. Assign RoadGenerator reference (if using distance fog)\n" +
                "2. Tweak config values to your preference\n" +
                "3. Use 'Apply Settings Now' context menu to preview",
                MessageType.Info);
        }

        private static void CreateAtmosphereConfig()
        {
            EnsureDirectory("Assets/Settings");

            string configPath = "Assets/Settings/EerieAtmosphereConfig.asset";

            // Check if exists
            AtmosphereConfig existing = AssetDatabase.LoadAssetAtPath<AtmosphereConfig>(configPath);
            if (existing != null)
            {
                Debug.Log($"Atmosphere config already exists: {configPath}");
                Selection.activeObject = existing;
                return;
            }

            // Create new config with eerie settings
            AtmosphereConfig config = ScriptableObject.CreateInstance<AtmosphereConfig>();

            // Fog - thick and oppressive
            config.enableFog = true;
            config.fogMode = FogMode.ExponentialSquared;
            config.fogColor = new Color(0.12f, 0.12f, 0.15f); // Dark blue-gray
            config.fogDensity = 0.025f;
            config.fogStartDistance = 5f;
            config.fogEndDistance = 120f;

            // Lighting - dim and cold
            config.ambientColor = new Color(0.08f, 0.08f, 0.12f); // Very dark blue
            config.ambientIntensity = 0.25f;
            config.directionalLightColor = new Color(0.35f, 0.35f, 0.45f); // Cold blue-white
            config.directionalLightIntensity = 0.15f;
            config.lightDirection = new Vector3(45f, -30f, 0f);

            // Skybox - dark and ominous
            config.skyTint = new Color(0.1f, 0.1f, 0.15f);
            config.skyExposure = 0.3f;

            // Post-processing - horror feel
            config.colorGradingContrast = 0.25f;
            config.colorGradingSaturation = -30f; // Desaturated
            config.vignetteIntensity = 0.4f;
            config.filmGrainIntensity = 0.25f;
            config.bloomIntensity = 0.3f;

            // Flicker effects
            config.enableFlicker = true;
            config.flickerChance = 0.03f;
            config.flickerIntensity = 0.4f;
            config.minFlickerInterval = 8f;
            config.maxFlickerInterval = 45f;

            // Distance fog increase
            config.varyFogWithDistance = true;
            config.fogIncreasePerKm = 0.008f;
            config.maxFogDensity = 0.06f;

            AssetDatabase.CreateAsset(config, configPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"Created eerie atmosphere config: {configPath}");
            Selection.activeObject = config;
        }

        private static void SetupPostProcessingVolume()
        {
            // Find or create global volume
            Volume globalVolume = FindObjectOfType<Volume>();

            if (globalVolume == null)
            {
                GameObject volumeObj = new GameObject("Global Volume");
                globalVolume = volumeObj.AddComponent<Volume>();
                globalVolume.isGlobal = true;
                Debug.Log("Created new Global Volume");
            }

            // Create or get volume profile
            EnsureDirectory("Assets/Settings");
            string profilePath = "Assets/Settings/EerieVolumeProfile.asset";

            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);

            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, profilePath);
            }

            // Add/configure effects
            ConfigureColorAdjustments(profile);
            ConfigureVignette(profile);
            ConfigureBloom(profile);
            ConfigureFilmGrain(profile);
            ConfigureLiftGammaGain(profile);

            globalVolume.profile = profile;

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            Debug.Log($"Post-processing volume configured: {profilePath}");
            Selection.activeObject = globalVolume;
        }

        private static void ConfigureColorAdjustments(VolumeProfile profile)
        {
            if (!profile.Has<ColorAdjustments>())
            {
                profile.Add<ColorAdjustments>();
            }

            if (profile.TryGet(out ColorAdjustments ca))
            {
                ca.active = true;
                ca.contrast.Override(25f);
                ca.saturation.Override(-30f);
                ca.postExposure.Override(-0.3f);
            }
        }

        private static void ConfigureVignette(VolumeProfile profile)
        {
            if (!profile.Has<Vignette>())
            {
                profile.Add<Vignette>();
            }

            if (profile.TryGet(out Vignette v))
            {
                v.active = true;
                v.intensity.Override(0.4f);
                v.smoothness.Override(0.4f);
                v.color.Override(Color.black);
            }
        }

        private static void ConfigureBloom(VolumeProfile profile)
        {
            if (!profile.Has<Bloom>())
            {
                profile.Add<Bloom>();
            }

            if (profile.TryGet(out Bloom b))
            {
                b.active = true;
                b.intensity.Override(0.3f);
                b.threshold.Override(0.9f);
                b.scatter.Override(0.7f);
            }
        }

        private static void ConfigureFilmGrain(VolumeProfile profile)
        {
            if (!profile.Has<FilmGrain>())
            {
                profile.Add<FilmGrain>();
            }

            if (profile.TryGet(out FilmGrain fg))
            {
                fg.active = true;
                fg.intensity.Override(0.25f);
                fg.response.Override(0.8f);
            }
        }

        private static void ConfigureLiftGammaGain(VolumeProfile profile)
        {
            if (!profile.Has<LiftGammaGain>())
            {
                profile.Add<LiftGammaGain>();
            }

            if (profile.TryGet(out LiftGammaGain lgg))
            {
                lgg.active = true;
                // Slight blue tint to shadows for cold feel
                lgg.lift.Override(new Vector4(0f, 0f, 0.05f, 0f));
                // Slightly lower midtones
                lgg.gamma.Override(new Vector4(0f, 0f, 0f, -0.1f));
            }
        }

        private static void AddAtmosphereController()
        {
            // Check if already exists
            AtmosphereController existing = FindObjectOfType<AtmosphereController>();
            if (existing != null)
            {
                Debug.Log("AtmosphereController already exists in scene");
                Selection.activeObject = existing.gameObject;
                return;
            }

            // Create controller object
            GameObject controllerObj = new GameObject("AtmosphereController");
            AtmosphereController controller = controllerObj.AddComponent<AtmosphereController>();

            // Assign config
            string configPath = "Assets/Settings/EerieAtmosphereConfig.asset";
            AtmosphereConfig config = AssetDatabase.LoadAssetAtPath<AtmosphereConfig>(configPath);
            if (config != null)
            {
                SerializedObject so = new SerializedObject(controller);
                so.FindProperty("config").objectReferenceValue = config;
                so.ApplyModifiedProperties();
            }

            // Try to find and assign references
            Volume volume = FindObjectOfType<Volume>();
            if (volume != null)
            {
                SerializedObject so = new SerializedObject(controller);
                so.FindProperty("postProcessVolume").objectReferenceValue = volume;
                so.ApplyModifiedProperties();
            }

            Light dirLight = FindDirectionalLight();
            if (dirLight != null)
            {
                SerializedObject so = new SerializedObject(controller);
                so.FindProperty("directionalLight").objectReferenceValue = dirLight;
                so.ApplyModifiedProperties();
            }

            // Apply settings immediately
            controller.EditorApplySettings();

            Debug.Log("Created AtmosphereController in scene");
            Selection.activeObject = controllerObj;
        }

        private static Light FindDirectionalLight()
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

        private static void EnsureDirectory(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
                string folder = System.IO.Path.GetFileName(path);

                if (!AssetDatabase.IsValidFolder(parent))
                {
                    EnsureDirectory(parent);
                }

                AssetDatabase.CreateFolder(parent, folder);
            }
        }
    }
}
#endif
