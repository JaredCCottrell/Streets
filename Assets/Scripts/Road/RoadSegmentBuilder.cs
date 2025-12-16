using UnityEngine;

namespace Streets.Road
{
    [RequireComponent(typeof(RoadSegment))]
    public class RoadSegmentBuilder : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private RoadConfig config;

        [Header("Segment Settings")]
        [SerializeField] private float segmentLength = 50f;
        [SerializeField] private float curveAngle = 0f;

        [Header("Generated Objects")]
        [SerializeField] private GameObject roadMesh;
        [SerializeField] private Transform entryPoint;
        [SerializeField] private Transform exitPoint;

        private RoadSegment roadSegment;

        [ContextMenu("Build Road Segment")]
        public void BuildSegment()
        {
            roadSegment = GetComponent<RoadSegment>();

            ClearExisting();
            CreateRoadSurface();
            CreateConnectionPoints();
            CreateSpawnPoints();

            Debug.Log($"[RoadSegmentBuilder] Built segment: {segmentLength}m, curve: {curveAngle}°");
        }

        private void ClearExisting()
        {
            // Clear all children except specific preserved objects
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        private void CreateRoadSurface()
        {
            float roadWidth = config != null ? config.TotalRoadWidth : 14f;

            // Create road plane
            roadMesh = GameObject.CreatePrimitive(PrimitiveType.Plane);
            roadMesh.name = "RoadSurface";
            roadMesh.transform.SetParent(transform);
            roadMesh.transform.localPosition = new Vector3(0, 0, segmentLength / 2f);
            roadMesh.transform.localRotation = Quaternion.identity;

            // Scale plane (default plane is 10x10)
            float scaleX = roadWidth / 10f;
            float scaleZ = segmentLength / 10f;
            roadMesh.transform.localScale = new Vector3(scaleX, 1f, scaleZ);

            // Apply material if available
            if (config != null && config.roadMaterial != null)
            {
                roadMesh.GetComponent<Renderer>().material = config.roadMaterial;
            }
            else
            {
                // Create a simple dark gray material (URP compatible)
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                if (shader == null) shader = Shader.Find("Sprites/Default");

                Material roadMat = new Material(shader);
                roadMat.color = new Color(0.2f, 0.2f, 0.2f);
                roadMesh.GetComponent<Renderer>().material = roadMat;
            }

            // Set layer for ground detection
            roadMesh.layer = LayerMask.NameToLayer("Ground");

            // Add lane lines
            CreateLaneLines(roadWidth);
        }

        private void CreateLaneLines(float roadWidth)
        {
            float laneWidth = config != null ? config.laneWidth : 3.5f;
            int lanes = config != null ? config.lanesPerDirection : 2;

            // Center line (double yellow)
            CreateLine("CenterLine", 0f, Color.yellow, 0.15f);

            // Lane dividers
            for (int i = 1; i < lanes; i++)
            {
                float offset = i * laneWidth;
                CreateLine($"LaneLine_R{i}", offset, Color.white, 0.1f, true);
                CreateLine($"LaneLine_L{i}", -offset, Color.white, 0.1f, true);
            }

            // Edge lines
            float edgeOffset = (lanes * laneWidth) + 0.5f;
            CreateLine("EdgeLine_R", edgeOffset, Color.white, 0.15f);
            CreateLine("EdgeLine_L", -edgeOffset, Color.white, 0.15f);
        }

        private void CreateLine(string name, float xOffset, Color color, float width, bool dashed = false)
        {
            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.name = name;
            line.transform.SetParent(transform);
            line.transform.localPosition = new Vector3(xOffset, 0.01f, segmentLength / 2f);
            line.transform.localScale = new Vector3(width, 0.01f, segmentLength);

            // Remove collider from lines
            DestroyImmediate(line.GetComponent<Collider>());

            // URP compatible shader
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            Material lineMat = new Material(shader);
            lineMat.color = color;
            line.GetComponent<Renderer>().material = lineMat;
        }

        private void CreateConnectionPoints()
        {
            // Entry point (start of segment)
            GameObject entry = new GameObject("EntryPoint");
            entry.transform.SetParent(transform);
            entry.transform.localPosition = Vector3.zero;
            entry.transform.localRotation = Quaternion.identity;
            entryPoint = entry.transform;

            // Exit point (end of segment, with optional curve)
            GameObject exit = new GameObject("ExitPoint");
            exit.transform.SetParent(transform);

            if (Mathf.Abs(curveAngle) > 0.1f)
            {
                // Calculate curved exit position
                float radius = segmentLength / (Mathf.Deg2Rad * Mathf.Abs(curveAngle));
                float angleRad = Mathf.Deg2Rad * curveAngle;

                float x = radius * (1 - Mathf.Cos(angleRad)) * Mathf.Sign(curveAngle);
                float z = radius * Mathf.Sin(Mathf.Abs(angleRad));

                exit.transform.localPosition = new Vector3(x, 0, z);
                exit.transform.localRotation = Quaternion.Euler(0, curveAngle, 0);
            }
            else
            {
                exit.transform.localPosition = new Vector3(0, 0, segmentLength);
                exit.transform.localRotation = Quaternion.identity;
            }

            exitPoint = exit.transform;

            // Update RoadSegment references via serialized fields
            #if UNITY_EDITOR
            var so = new UnityEditor.SerializedObject(roadSegment);
            so.FindProperty("entryPoint").objectReferenceValue = entryPoint;
            so.FindProperty("exitPoint").objectReferenceValue = exitPoint;
            so.FindProperty("segmentLength").floatValue = segmentLength;
            so.ApplyModifiedProperties();
            #endif
        }

        private void CreateSpawnPoints()
        {
            float roadWidth = config != null ? config.TotalRoadWidth : 14f;
            float halfWidth = roadWidth / 2f + 2f; // Slightly outside road

            // Create prop spawn points along the sides
            int propCount = Mathf.FloorToInt(segmentLength / 25f);
            Transform[] propPoints = new Transform[propCount * 2];

            for (int i = 0; i < propCount; i++)
            {
                float z = (i + 0.5f) * (segmentLength / propCount);

                // Right side
                GameObject propR = new GameObject($"PropSpawn_R{i}");
                propR.transform.SetParent(transform);
                propR.transform.localPosition = new Vector3(halfWidth, 0, z);
                propPoints[i * 2] = propR.transform;

                // Left side
                GameObject propL = new GameObject($"PropSpawn_L{i}");
                propL.transform.SetParent(transform);
                propL.transform.localPosition = new Vector3(-halfWidth, 0, z);
                propPoints[i * 2 + 1] = propL.transform;
            }

            // Create event spawn point in center of segment
            GameObject eventPoint = new GameObject("EventSpawn");
            eventPoint.transform.SetParent(transform);
            eventPoint.transform.localPosition = new Vector3(0, 0, segmentLength / 2f);

            // Create item spawn points
            GameObject itemPoint1 = new GameObject("ItemSpawn_1");
            itemPoint1.transform.SetParent(transform);
            itemPoint1.transform.localPosition = new Vector3(halfWidth * 0.3f, 0.5f, segmentLength * 0.3f);

            GameObject itemPoint2 = new GameObject("ItemSpawn_2");
            itemPoint2.transform.SetParent(transform);
            itemPoint2.transform.localPosition = new Vector3(-halfWidth * 0.3f, 0.5f, segmentLength * 0.7f);

            // Update RoadSegment references
            #if UNITY_EDITOR
            var so = new UnityEditor.SerializedObject(roadSegment);
            so.FindProperty("propSpawnPoints").arraySize = propPoints.Length;
            for (int i = 0; i < propPoints.Length; i++)
            {
                so.FindProperty("propSpawnPoints").GetArrayElementAtIndex(i).objectReferenceValue = propPoints[i];
            }
            so.FindProperty("eventSpawnPoints").arraySize = 1;
            so.FindProperty("eventSpawnPoints").GetArrayElementAtIndex(0).objectReferenceValue = eventPoint.transform;
            so.FindProperty("itemSpawnPoints").arraySize = 2;
            so.FindProperty("itemSpawnPoints").GetArrayElementAtIndex(0).objectReferenceValue = itemPoint1.transform;
            so.FindProperty("itemSpawnPoints").GetArrayElementAtIndex(1).objectReferenceValue = itemPoint2.transform;
            so.ApplyModifiedProperties();
            #endif
        }

#if UNITY_EDITOR
        private void Reset()
        {
            // Auto-find config
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:RoadConfig");
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                config = UnityEditor.AssetDatabase.LoadAssetAtPath<RoadConfig>(path);
            }
        }
#endif
    }
}
