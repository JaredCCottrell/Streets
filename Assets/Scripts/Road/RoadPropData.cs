using UnityEngine;

namespace Streets.Road
{
    [CreateAssetMenu(fileName = "RoadProp", menuName = "Streets/Road Prop")]
    public class RoadPropData : ScriptableObject
    {
        [Header("Prop Settings")]
        public string propName;
        public GameObject prefab;

        [Header("Spawn Settings")]
        [Range(0f, 1f)]
        public float spawnChance = 0.5f;
        [Range(0f, 100f)]
        public float weight = 10f;

        [Header("Placement")]
        public PropSide allowedSides = PropSide.Both;
        public bool randomRotation = false;
        [Range(0f, 360f)]
        public float maxRandomRotation = 15f;
        public Vector3 positionOffset = Vector3.zero;
        public Vector3 rotationOffset = Vector3.zero;

        [Header("Variation")]
        public bool randomScale = false;
        public float minScale = 0.9f;
        public float maxScale = 1.1f;

        [Header("Clustering")]
        public bool canCluster = false;
        [Range(1, 5)]
        public int maxClusterSize = 3;
        public float clusterRadius = 2f;
    }

    public enum PropSide
    {
        Left,
        Right,
        Both
    }
}
