#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Streets.Road;
using System.IO;
using System.Collections.Generic;

namespace Streets.Editor
{
    public class RoadPropSetupTool : EditorWindow
    {
        [MenuItem("Streets/Setup Road Props")]
        public static void ShowWindow()
        {
            GetWindow<RoadPropSetupTool>("Road Prop Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Road Prop Setup Tool", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("1. Create All Prop Prefabs", GUILayout.Height(30)))
            {
                CreateAllPropPrefabs();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("2. Create All Prop Data Assets", GUILayout.Height(30)))
            {
                CreateAllPropDataAssets();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("3. Create Prop Pool", GUILayout.Height(30)))
            {
                CreatePropPool();
            }

            GUILayout.Space(20);

            if (GUILayout.Button("Do All Steps", GUILayout.Height(40)))
            {
                CreateAllPropPrefabs();
                CreateAllPropDataAssets();
                CreatePropPool();
                EditorUtility.DisplayDialog("Complete", "All road props have been set up!", "OK");
            }
        }

        private static Dictionary<string, Material> materialCache = new Dictionary<string, Material>();

        private static void CreateAllPropPrefabs()
        {
            EnsureDirectory("Assets/Prefabs/Props");
            EnsureDirectory("Assets/Materials/Props");
            materialCache.Clear();

            var propTypes = System.Enum.GetValues(typeof(RoadPropBuilder.PropType));

            foreach (RoadPropBuilder.PropType propType in propTypes)
            {
                string prefabPath = $"Assets/Prefabs/Props/Prop_{propType}.prefab";

                // Skip if already exists
                if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
                {
                    Debug.Log($"Prefab already exists: {prefabPath}");
                    continue;
                }

                // Create GameObject
                GameObject propObj = new GameObject($"Prop_{propType}");
                RoadPropBuilder builder = propObj.AddComponent<RoadPropBuilder>();

                // Set prop type via serialized property
                SerializedObject so = new SerializedObject(builder);
                so.FindProperty("propType").enumValueIndex = (int)propType;
                so.ApplyModifiedPropertiesWithoutUndo();

                // Build the prop
                builder.BuildProp();

                // Remove the builder component (not needed at runtime)
                DestroyImmediate(builder);

                // Fix materials - replace with saved material assets
                FixMaterials(propObj);

                // Save as prefab
                PrefabUtility.SaveAsPrefabAsset(propObj, prefabPath);
                DestroyImmediate(propObj);

                Debug.Log($"Created prefab: {prefabPath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void FixMaterials(GameObject obj)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                Material[] mats = renderer.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    Color color = Color.gray;

                    if (mats[i] != null)
                    {
                        // Try to get color from various properties
                        if (mats[i].HasProperty("_BaseColor"))
                            color = mats[i].GetColor("_BaseColor");
                        else if (mats[i].HasProperty("_Color"))
                            color = mats[i].GetColor("_Color");

                        // Check if color is valid (not default/unset)
                        if (color.a < 0.01f)
                            color = Color.gray;
                    }

                    mats[i] = GetOrCreateMaterial(color);
                }
                renderer.sharedMaterials = mats;
            }
        }

        private static Material GetOrCreateMaterial(Color color)
        {
            // Ensure alpha is 1
            color.a = 1f;

            // Create a key from rounded color values
            string colorKey = $"{Mathf.RoundToInt(color.r * 255):X2}{Mathf.RoundToInt(color.g * 255):X2}{Mathf.RoundToInt(color.b * 255):X2}";
            string matPath = $"Assets/Materials/Props/Mat_{colorKey}.mat";

            if (materialCache.TryGetValue(colorKey, out Material cached))
                return cached;

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                // Create from default primitive material
                GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mat = new Material(temp.GetComponent<Renderer>().sharedMaterial);
                DestroyImmediate(temp);

                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", color);
                if (mat.HasProperty("_Color"))
                    mat.SetColor("_Color", color);

                AssetDatabase.CreateAsset(mat, matPath);
                Debug.Log($"Created material: {matPath} with color {color}");
            }

            materialCache[colorKey] = mat;
            return mat;
        }

        private static void CreateAllPropDataAssets()
        {
            EnsureDirectory("Assets/Settings/Props");

            var propTypes = System.Enum.GetValues(typeof(RoadPropBuilder.PropType));

            foreach (RoadPropBuilder.PropType propType in propTypes)
            {
                string dataPath = $"Assets/Settings/Props/{propType}Data.asset";

                // Skip if already exists
                if (AssetDatabase.LoadAssetAtPath<RoadPropData>(dataPath) != null)
                {
                    Debug.Log($"PropData already exists: {dataPath}");
                    continue;
                }

                // Create PropData asset
                RoadPropData propData = ScriptableObject.CreateInstance<RoadPropData>();
                propData.propName = propType.ToString();

                // Load corresponding prefab
                string prefabPath = $"Assets/Prefabs/Props/Prop_{propType}.prefab";
                propData.prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                // Set default values based on prop type
                ConfigurePropData(propData, propType);

                AssetDatabase.CreateAsset(propData, dataPath);
                Debug.Log($"Created PropData: {dataPath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void ConfigurePropData(RoadPropData propData, RoadPropBuilder.PropType propType)
        {
            // Default settings
            propData.spawnChance = 0.7f;
            propData.weight = 10f;
            propData.allowedSides = PropSide.Both;
            propData.randomRotation = false;
            propData.maxRandomRotation = 15f;
            propData.randomScale = false;
            propData.minScale = 0.9f;
            propData.maxScale = 1.1f;
            propData.canCluster = false;

            // Customize per type
            switch (propType)
            {
                case RoadPropBuilder.PropType.Barrel:
                    propData.weight = 20f;
                    propData.randomRotation = true;
                    propData.canCluster = true;
                    propData.maxClusterSize = 3;
                    propData.clusterRadius = 1.5f;
                    break;

                case RoadPropBuilder.PropType.TrafficCone:
                    propData.weight = 25f;
                    propData.randomRotation = true;
                    propData.maxRandomRotation = 360f;
                    propData.canCluster = true;
                    propData.maxClusterSize = 4;
                    propData.clusterRadius = 1f;
                    break;

                case RoadPropBuilder.PropType.Guardrail:
                    propData.weight = 30f;
                    propData.spawnChance = 0.8f;
                    propData.randomRotation = false;
                    break;

                case RoadPropBuilder.PropType.Jersey_Barrier:
                    propData.weight = 15f;
                    propData.spawnChance = 0.5f;
                    break;

                case RoadPropBuilder.PropType.StreetLight:
                    propData.weight = 10f;
                    propData.spawnChance = 0.4f;
                    propData.randomScale = true;
                    propData.minScale = 0.95f;
                    propData.maxScale = 1.05f;
                    break;

                case RoadPropBuilder.PropType.SpeedLimitSign:
                    propData.weight = 8f;
                    propData.spawnChance = 0.3f;
                    break;

                case RoadPropBuilder.PropType.MileMarker:
                    propData.weight = 5f;
                    propData.spawnChance = 0.2f;
                    break;

                case RoadPropBuilder.PropType.AbandonedCar:
                    propData.weight = 3f;
                    propData.spawnChance = 0.15f;
                    propData.randomRotation = true;
                    propData.maxRandomRotation = 30f;
                    propData.randomScale = true;
                    propData.minScale = 0.9f;
                    propData.maxScale = 1.1f;
                    break;
            }
        }

        private static void CreatePropPool()
        {
            EnsureDirectory("Assets/Settings");

            string poolPath = "Assets/Settings/RoadsidePropPool.asset";

            // Check if exists
            RoadPropPool existingPool = AssetDatabase.LoadAssetAtPath<RoadPropPool>(poolPath);
            if (existingPool != null)
            {
                Debug.Log("Updating existing prop pool...");
            }

            RoadPropPool pool = existingPool ?? ScriptableObject.CreateInstance<RoadPropPool>();
            pool.poolName = "Roadside Props";
            pool.spawnPointUsageChance = 0.5f;
            pool.minPropsPerSegment = 1;
            pool.maxPropsPerSegment = 4;

            // Find all prop data assets
            string[] guids = AssetDatabase.FindAssets("t:RoadPropData", new[] { "Assets/Settings/Props" });
            RoadPropData[] propDataArray = new RoadPropData[guids.Length];

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                propDataArray[i] = AssetDatabase.LoadAssetAtPath<RoadPropData>(path);
            }

            pool.props = propDataArray;

            if (existingPool == null)
            {
                AssetDatabase.CreateAsset(pool, poolPath);
            }
            else
            {
                EditorUtility.SetDirty(pool);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Prop pool created/updated with {propDataArray.Length} props");
        }

        private static void EnsureDirectory(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace("\\", "/");
                string folder = Path.GetFileName(path);

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
