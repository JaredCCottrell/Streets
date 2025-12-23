using System;
using UnityEngine;

namespace Streets.Road
{
    public class RoadSegment : MonoBehaviour
    {
        [Header("Segment Settings")]
        [SerializeField] private float segmentLength = 50f;
        [SerializeField] private float roadWidth = 12f;
        [SerializeField] private SegmentType segmentType = SegmentType.Straight;

        [Header("Connection Points")]
        [SerializeField] private Transform entryPoint;
        [SerializeField] private Transform exitPoint;

        [Header("Spawn Points")]
        [SerializeField] private Transform[] propSpawnPoints;
        [SerializeField] private Transform[] eventSpawnPoints;
        [SerializeField] private Transform[] itemSpawnPoints;

        [Header("Event Settings")]
        [Tooltip("If true, this segment can trigger events when the player enters")]
        [SerializeField] private bool canTriggerEvent = true;

        [Tooltip("Override trigger chance for this segment (-1 = use pool default)")]
        [Range(-1f, 1f)]
        [SerializeField] private float triggerChanceOverride = -1f;

        public float SegmentLength => segmentLength;
        public float RoadWidth => roadWidth;
        public SegmentType Type => segmentType;
        public Transform EntryPoint => entryPoint;
        public Transform ExitPoint => exitPoint;
        public Transform[] PropSpawnPoints => propSpawnPoints;
        public Transform[] EventSpawnPoints => eventSpawnPoints;
        public Transform[] ItemSpawnPoints => itemSpawnPoints;
        public bool CanTriggerEvent => canTriggerEvent;
        public float TriggerChanceOverride => triggerChanceOverride;

        // Runtime state
        private int segmentIndex;
        private bool hasBeenVisited;
        private bool eventTriggered;

        public int SegmentIndex
        {
            get => segmentIndex;
            set => segmentIndex = value;
        }

        public bool HasBeenVisited
        {
            get => hasBeenVisited;
            set => hasBeenVisited = value;
        }

        public bool EventTriggered
        {
            get => eventTriggered;
            set => eventTriggered = value;
        }

        // Events
        public event Action<RoadSegment> OnPlayerEntered;
        public event Action<RoadSegment> OnPlayerExited;

        /// <summary>
        /// Called when player enters this segment. Fires OnPlayerEntered event.
        /// </summary>
        public void NotifyPlayerEntered()
        {
            if (!hasBeenVisited)
            {
                hasBeenVisited = true;
                OnPlayerEntered?.Invoke(this);
            }
        }

        /// <summary>
        /// Called when player exits this segment. Fires OnPlayerExited event.
        /// </summary>
        public void NotifyPlayerExited()
        {
            OnPlayerExited?.Invoke(this);
        }

        /// <summary>
        /// Get a random event spawn point, or segment center if none defined
        /// </summary>
        public Transform GetRandomEventSpawnPoint()
        {
            if (eventSpawnPoints != null && eventSpawnPoints.Length > 0)
            {
                var validPoints = System.Array.FindAll(eventSpawnPoints, p => p != null);
                if (validPoints.Length > 0)
                    return validPoints[UnityEngine.Random.Range(0, validPoints.Length)];
            }
            return transform;
        }

        /// <summary>
        /// Reset runtime state (called when segment is recycled)
        /// </summary>
        public void ResetState()
        {
            hasBeenVisited = false;
            eventTriggered = false;
        }

        private void OnDrawGizmos()
        {
            // Draw segment bounds
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position + Vector3.forward * segmentLength / 2f,
                new Vector3(roadWidth, 0.1f, segmentLength));

            // Draw entry point
            if (entryPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(entryPoint.position, 0.5f);
                Gizmos.DrawRay(entryPoint.position, entryPoint.forward * 2f);
            }

            // Draw exit point
            if (exitPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(exitPoint.position, 0.5f);
                Gizmos.DrawRay(exitPoint.position, exitPoint.forward * 2f);
            }

            // Draw prop spawn points
            Gizmos.color = Color.yellow;
            if (propSpawnPoints != null)
            {
                foreach (var point in propSpawnPoints)
                {
                    if (point != null)
                        Gizmos.DrawWireCube(point.position, Vector3.one * 0.5f);
                }
            }

            // Draw event spawn points
            Gizmos.color = Color.magenta;
            if (eventSpawnPoints != null)
            {
                foreach (var point in eventSpawnPoints)
                {
                    if (point != null)
                        Gizmos.DrawWireSphere(point.position, 0.75f);
                }
            }

            // Draw item spawn points
            Gizmos.color = Color.blue;
            if (itemSpawnPoints != null)
            {
                foreach (var point in itemSpawnPoints)
                {
                    if (point != null)
                        Gizmos.DrawWireCube(point.position, Vector3.one * 0.3f);
                }
            }
        }

        /// <summary>
        /// Get the world position where the next segment should connect
        /// </summary>
        public Vector3 GetNextSegmentPosition()
        {
            if (exitPoint != null)
            {
                return exitPoint.position;
            }
            return transform.position + transform.forward * segmentLength;
        }

        /// <summary>
        /// Get the rotation for the next segment
        /// </summary>
        public Quaternion GetNextSegmentRotation()
        {
            if (exitPoint != null)
            {
                return exitPoint.rotation;
            }
            return transform.rotation;
        }

        /// <summary>
        /// Align this segment to connect with a previous segment's exit
        /// </summary>
        public void AlignToEntry(Vector3 position, Quaternion rotation)
        {
            if (entryPoint != null)
            {
                // Calculate offset from entry point to segment origin
                Vector3 entryOffset = transform.position - entryPoint.position;
                Quaternion entryRotationOffset = Quaternion.Inverse(entryPoint.rotation) * transform.rotation;

                transform.rotation = rotation * entryRotationOffset;
                transform.position = position + transform.rotation * Quaternion.Inverse(entryRotationOffset) * entryOffset;
            }
            else
            {
                transform.position = position;
                transform.rotation = rotation;
            }
        }
    }

    public enum SegmentType
    {
        Straight,
        SlightLeft,
        SlightRight,
        Overpass,
        Underpass,
        Bridge,
        Tunnel,
        RestStop,
        Intersection
    }
}
