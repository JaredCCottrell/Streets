using UnityEngine;
using UnityEditor;

namespace Streets.Editor
{
    public static class SidewalkPrefabCreator
    {
        [MenuItem("Streets/Create Sidewalk Prefab")]
        public static void CreateSidewalkPrefab()
        {
            // Sidewalk dimensions (matches road segment length)
            float length = 30f;
            float sidewalkWidth = 2f;
            float sidewalkHeight = 0.15f;
            float curbWidth = 0.2f;
            float curbHeight = 0.15f;

            // Create parent GameObject
            GameObject sidewalk = new GameObject("SidewalkSegment");

            // Create sidewalk surface
            GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
            surface.name = "Surface";
            surface.transform.SetParent(sidewalk.transform);
            surface.transform.localPosition = new Vector3(sidewalkWidth / 2f + curbWidth, sidewalkHeight / 2f, 0f);
            surface.transform.localScale = new Vector3(sidewalkWidth, sidewalkHeight, length);

            // Create curb (raised edge next to road)
            GameObject curb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            curb.name = "Curb";
            curb.transform.SetParent(sidewalk.transform);
            curb.transform.localPosition = new Vector3(curbWidth / 2f, (sidewalkHeight + curbHeight) / 2f, 0f);
            curb.transform.localScale = new Vector3(curbWidth, sidewalkHeight + curbHeight, length);

            // Apply materials
            Material concreteMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Concrete.mat");
            Material concreteDarkMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ConcreteDark.mat");

            if (concreteMat != null)
            {
                surface.GetComponent<MeshRenderer>().sharedMaterial = concreteMat;
            }
            if (concreteDarkMat != null)
            {
                curb.GetComponent<MeshRenderer>().sharedMaterial = concreteDarkMat;
            }

            // Make sure Prefabs folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            // Save as prefab
            string prefabPath = "Assets/Prefabs/SidewalkSegment.prefab";

            // Remove existing prefab if it exists
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }

            PrefabUtility.SaveAsPrefabAsset(sidewalk, prefabPath);

            // Cleanup scene object
            Object.DestroyImmediate(sidewalk);

            AssetDatabase.Refresh();

            Debug.Log($"Sidewalk prefab created at {prefabPath}");

            // Select the new prefab
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }
    }
}
