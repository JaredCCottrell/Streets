#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Streets.Editor
{
    public class SkyboxSetup : EditorWindow
    {
        private static readonly string[] skyboxPaths = new string[]
        {
            "Assets/AllSkyFree/Cold Night/Cold Night.mat",
            "Assets/AllSkyFree/Night MoonBurst/Night Moon Burst.mat",
            "Assets/AllSkyFree/Deep Dusk/Deep Dusk.mat",
            "Assets/AllSkyFree/Overcast Low/AllSky_Overcast4_Low.mat",
            "Assets/AllSkyFree/Cold Sunset/Cold Sunset.mat"
        };

        private static readonly string[] skyboxNames = new string[]
        {
            "Cold Night (Recommended)",
            "Night MoonBurst",
            "Deep Dusk",
            "Overcast Low",
            "Cold Sunset"
        };

        private int selectedSkybox = 0;

        [MenuItem("Streets/Setup Skybox")]
        public static void ShowWindow()
        {
            GetWindow<SkyboxSetup>("Skybox Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("AllSky Skybox Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Select a skybox for your horror highway.\n" +
                "Cold Night is recommended for the eerie atmosphere.",
                MessageType.Info);

            GUILayout.Space(10);

            selectedSkybox = EditorGUILayout.Popup("Skybox", selectedSkybox, skyboxNames);

            GUILayout.Space(10);

            if (GUILayout.Button("Apply Skybox", GUILayout.Height(35)))
            {
                ApplySkybox(skyboxPaths[selectedSkybox]);
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Apply Cold Night (Quick)", GUILayout.Height(30)))
            {
                ApplySkybox(skyboxPaths[0]);
            }
        }

        public static void ApplySkybox(string path)
        {
            Material skyboxMat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (skyboxMat == null)
            {
                Debug.LogError($"Skybox not found: {path}");
                return;
            }

            // Apply to render settings
            RenderSettings.skybox = skyboxMat;

            // Mark scene dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log($"Applied skybox: {skyboxMat.name}");
        }

        [MenuItem("Streets/Quick Apply Cold Night Skybox")]
        public static void QuickApplyColdNight()
        {
            ApplySkybox("Assets/AllSkyFree/Cold Night/Cold Night.mat");
        }
    }
}
#endif
