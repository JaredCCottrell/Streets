using UnityEngine;

namespace Streets.Road
{
    [CreateAssetMenu(fileName = "RoadConfig", menuName = "Streets/Road Config")]
    public class RoadConfig : ScriptableObject
    {
        [Header("Road Dimensions")]
        public float laneWidth = 3.5f;
        public int lanesPerDirection = 2;
        public float shoulderWidth = 2f;
        public float medianWidth = 1f;

        [Header("Segment Settings")]
        public float standardSegmentLength = 50f;
        public float curveAngle = 5f;

        [Header("Visual Settings")]
        public Material roadMaterial;
        public Material lineMaterial;
        public Material shoulderMaterial;

        [Header("Prop Settings")]
        public float guardrailHeight = 0.8f;
        public float signpostSpacing = 100f;
        public float streetlightSpacing = 50f;

        [Header("Environment")]
        public Color fogColor = new Color(0.3f, 0.3f, 0.35f);
        public float fogDensity = 0.02f;
        public Color ambientColor = new Color(0.15f, 0.15f, 0.2f);

        public float TotalRoadWidth => (laneWidth * lanesPerDirection * 2) + (shoulderWidth * 2) + medianWidth;
    }
}
