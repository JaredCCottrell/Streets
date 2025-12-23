using UnityEngine;

namespace Streets.Events
{
    /// <summary>
    /// Simple test event that shows a visible sphere and logs to console.
    /// Destroys itself after duration.
    /// </summary>
    public class TestEvent : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float duration = 5f;
        [SerializeField] private Color sphereColor = Color.red;
        [SerializeField] private float sphereRadius = 2f;
        [SerializeField] private float floatHeight = 3f;
        [SerializeField] private float pulseSpeed = 2f;

        [Header("Debug")]
        [SerializeField] private string eventMessage = "TEST EVENT TRIGGERED!";

        private float spawnTime;
        private GameObject visualSphere;

        private void Start()
        {
            spawnTime = Time.time;

            Debug.Log($"<color=yellow>[TestEvent] {eventMessage} at {transform.position}</color>");

            // Create a visible sphere
            visualSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visualSphere.transform.SetParent(transform);
            visualSphere.transform.localPosition = Vector3.up * floatHeight;
            visualSphere.transform.localScale = Vector3.one * sphereRadius;

            // Remove collider so player doesn't bump into it
            Destroy(visualSphere.GetComponent<Collider>());

            // Set material color (unlit so it's always visible)
            var renderer = visualSphere.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            renderer.material.color = sphereColor;
        }

        private void Update()
        {
            // Pulse effect
            if (visualSphere != null)
            {
                float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.2f;
                visualSphere.transform.localScale = Vector3.one * sphereRadius * pulse;

                // Bob up and down
                float bob = Mathf.Sin(Time.time * pulseSpeed * 0.5f) * 0.5f;
                visualSphere.transform.localPosition = Vector3.up * (floatHeight + bob);
            }

            // Self-destruct after duration
            if (Time.time - spawnTime >= duration)
            {
                Debug.Log($"<color=gray>[TestEvent] Event ended after {duration}s</color>");
                Destroy(gameObject);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = sphereColor;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * floatHeight, sphereRadius);
        }
    }
}
