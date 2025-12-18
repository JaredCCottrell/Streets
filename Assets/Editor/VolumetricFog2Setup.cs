#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using VolumetricFogAndMist2;

namespace Streets.Editor
{
    public class VolumetricFog2Setup : EditorWindow
    {
        [MenuItem("Streets/Setup Volumetric Fog 2")]
        public static void ShowWindow()
        {
            GetWindow<VolumetricFog2Setup>("Fog Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Volumetric Fog & Mist 2 Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Create Fog Manager + Volume", GUILayout.Height(40)))
            {
                CreateFogSystem();
            }

            GUILayout.Space(10);
            GUILayout.Label("Apply Preset:", EditorStyles.boldLabel);

            if (GUILayout.Button("Heavy Fog (Spooky)", GUILayout.Height(30)))
            {
                ApplyPreset("Heavy Fog");
            }

            if (GUILayout.Button("Mist (Subtle)", GUILayout.Height(30)))
            {
                ApplyPreset("Mist");
            }

            if (GUILayout.Button("Distant Fog", GUILayout.Height(30)))
            {
                ApplyPreset("Distant Fog Only 1");
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Configure for Horror Highway", GUILayout.Height(35)))
            {
                ConfigureHorrorFog();
            }
        }

        [MenuItem("Streets/Quick Setup Horror Fog")]
        public static void QuickSetup()
        {
            CreateFogSystem();
            ConfigureHorrorFog();
        }

        [MenuItem("Streets/Create Local Fog Prefab")]
        public static void CreateLocalFogPrefab()
        {
            // Create fog volume
            GameObject fogGO = VolumetricFogManager.CreateFogVolume("LocalFog");
            VolumetricFog fog = fogGO.GetComponent<VolumetricFog>();

            // Configure for small local area
            fogGO.transform.position = Vector3.zero;
            fogGO.transform.localScale = new Vector3(20, 10, 20);

            // Load horror profile or leave default
            string profilePath = "Assets/Settings/HorrorFogProfile.asset";
            VolumetricFogProfile profile = AssetDatabase.LoadAssetAtPath<VolumetricFogProfile>(profilePath);
            if (profile != null)
            {
                fog.profile = profile;
            }

            // Save as prefab
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            string prefabPath = "Assets/Prefabs/LocalFog.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }

            PrefabUtility.SaveAsPrefabAsset(fogGO, prefabPath);
            Object.DestroyImmediate(fogGO);

            AssetDatabase.Refresh();
            Debug.Log($"Local fog prefab created at {prefabPath}");

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        [MenuItem("Streets/Create Road Fog Prefab")]
        public static void CreateRoadFogPrefab()
        {
            // Create fog volume
            GameObject fogGO = VolumetricFogManager.CreateFogVolume("RoadFog");
            VolumetricFog fog = fogGO.GetComponent<VolumetricFog>();

            // Configure for road coverage
            fogGO.transform.position = new Vector3(0, 5, 100);
            fogGO.transform.localScale = new Vector3(80, 25, 600);

            // Load or create horror profile
            string profilePath = "Assets/Settings/HorrorFogProfile.asset";
            VolumetricFogProfile profile = AssetDatabase.LoadAssetAtPath<VolumetricFogProfile>(profilePath);
            if (profile != null)
            {
                fog.profile = profile;
            }

            // Save as prefab
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            string prefabPath = "Assets/Prefabs/RoadFog.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }

            PrefabUtility.SaveAsPrefabAsset(fogGO, prefabPath);
            Object.DestroyImmediate(fogGO);

            AssetDatabase.Refresh();
            Debug.Log($"Road fog prefab created at {prefabPath}");

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        private static void CreateFogSystem()
        {
            // Create manager if needed
            VolumetricFogManager manager = Object.FindFirstObjectByType<VolumetricFogManager>();
            if (manager == null)
            {
                VolumetricFogAndMist2.Tools.CheckMainManager();
                manager = Object.FindFirstObjectByType<VolumetricFogManager>();
            }

            // Create fog volume if none exists
            VolumetricFog fog = Object.FindFirstObjectByType<VolumetricFog>();
            if (fog == null)
            {
                GameObject fogGO = VolumetricFogManager.CreateFogVolume("Highway Fog");
                fog = fogGO.GetComponent<VolumetricFog>();

                // Position it at origin, large scale for highway
                fogGO.transform.position = Vector3.zero;
                fogGO.transform.localScale = new Vector3(100, 20, 500);

                Selection.activeObject = fogGO;
            }

            Debug.Log("Fog system created");
        }

        private static void ApplyPreset(string presetName)
        {
            VolumetricFog fog = Object.FindFirstObjectByType<VolumetricFog>();
            if (fog == null)
            {
                EditorUtility.DisplayDialog("Error", "No fog volume found. Create one first.", "OK");
                return;
            }

            // Find preset
            string[] guids = AssetDatabase.FindAssets(presetName + " t:VolumetricFogProfile");
            if (guids.Length == 0)
            {
                Debug.LogWarning($"Preset '{presetName}' not found");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            VolumetricFogProfile profile = AssetDatabase.LoadAssetAtPath<VolumetricFogProfile>(path);

            if (profile != null)
            {
                fog.profile = profile;
                EditorUtility.SetDirty(fog);
                Debug.Log($"Applied preset: {presetName}");
            }
        }

        private static void ConfigureHorrorFog()
        {
            VolumetricFog fog = Object.FindFirstObjectByType<VolumetricFog>();
            if (fog == null)
            {
                CreateFogSystem();
                fog = Object.FindFirstObjectByType<VolumetricFog>();
            }

            if (fog == null) return;

            // Create a custom horror profile
            string profilePath = "Assets/Settings/HorrorFogProfile.asset";
            VolumetricFogProfile profile = AssetDatabase.LoadAssetAtPath<VolumetricFogProfile>(profilePath);

            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumetricFogProfile>();

                // Horror fog settings - dense, low-lying, eerie
                profile.raymarchQuality = 8;
                profile.density = 0.6f;
                profile.noiseStrength = 0.8f;
                profile.noiseScale = 20f;
                profile.jittering = 0.5f;
                profile.dithering = 1f;

                // Geometry - low fog that hugs the ground
                profile.shape = VolumetricFogShape.Box;
                profile.border = 0.1f;
                profile.customHeight = true;
                profile.height = 15f;
                profile.verticalOffset = -2f;

                // Distance settings
                profile.distance = 0f;
                profile.distanceFallOff = 0.9f;
                profile.maxDistance = 300f;
                profile.maxDistanceFallOff = 0.3f;

                // Ensure Settings folder exists
                if (!AssetDatabase.IsValidFolder("Assets/Settings"))
                {
                    AssetDatabase.CreateFolder("Assets", "Settings");
                }

                AssetDatabase.CreateAsset(profile, profilePath);
                Debug.Log("Created horror fog profile");
            }

            fog.profile = profile;

            // Configure fog volume size for endless highway
            fog.transform.localScale = new Vector3(60, 25, 600);
            fog.transform.position = new Vector3(0, 5, 150);

            EditorUtility.SetDirty(fog);
            EditorUtility.SetDirty(fog.gameObject);

            Debug.Log("Horror fog configured - large volume following highway");
            EditorUtility.DisplayDialog("Success",
                "Horror fog configured!\n\n" +
                "- Low-lying dense fog\n" +
                "- Covers 600m of road\n" +
                "- Adjust position to follow player if needed", "OK");
        }
    }
}
#endif
