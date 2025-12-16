using UnityEngine;
using System.Collections.Generic;

namespace Streets.Road
{
    [CreateAssetMenu(fileName = "RoadPropPool", menuName = "Streets/Road Prop Pool")]
    public class RoadPropPool : ScriptableObject
    {
        [Header("Pool Settings")]
        public string poolName;

        [Header("Props")]
        public RoadPropData[] props;

        [Header("Spawn Settings")]
        [Range(0f, 1f)]
        [Tooltip("Chance that any prop spawns at a given spawn point")]
        public float spawnPointUsageChance = 0.6f;

        [Range(1, 10)]
        [Tooltip("Maximum props per segment")]
        public int maxPropsPerSegment = 5;

        [Range(0, 5)]
        [Tooltip("Minimum props per segment")]
        public int minPropsPerSegment = 1;

        /// <summary>
        /// Select a random prop from the pool based on weights
        /// </summary>
        public RoadPropData GetRandomProp()
        {
            if (props == null || props.Length == 0) return null;

            float totalWeight = 0f;
            foreach (var prop in props)
            {
                if (prop != null)
                {
                    totalWeight += prop.weight;
                }
            }

            if (totalWeight <= 0f) return null;

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var prop in props)
            {
                if (prop != null)
                {
                    cumulative += prop.weight;
                    if (roll <= cumulative)
                    {
                        return prop;
                    }
                }
            }

            return props[0];
        }

        /// <summary>
        /// Get a random prop that's valid for the given side
        /// </summary>
        public RoadPropData GetRandomPropForSide(PropSide side)
        {
            if (props == null || props.Length == 0) return null;

            // Filter to valid props for this side
            List<RoadPropData> validProps = new List<RoadPropData>();
            float totalWeight = 0f;

            foreach (var prop in props)
            {
                if (prop != null && (prop.allowedSides == PropSide.Both || prop.allowedSides == side))
                {
                    validProps.Add(prop);
                    totalWeight += prop.weight;
                }
            }

            if (validProps.Count == 0 || totalWeight <= 0f) return null;

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var prop in validProps)
            {
                cumulative += prop.weight;
                if (roll <= cumulative)
                {
                    return prop;
                }
            }

            return validProps[0];
        }
    }
}
