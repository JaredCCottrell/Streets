using UnityEngine;

namespace Streets.Dialogue
{
    /// <summary>
    /// Visual component for a shadow figure entity.
    /// Creates a solid black humanoid shape with glowing white eyes.
    /// </summary>
    public class ShadowFigure : MonoBehaviour
    {
        [Header("Body Settings")]
        [SerializeField] private float bodyHeight = 2f;
        [SerializeField] private float bodyWidth = 0.5f;
        [SerializeField] private Color bodyColor = Color.black;

        [Header("Eye Settings")]
        [SerializeField] private float eyeSize = 0.1f;
        [SerializeField] private float eyeSpacing = 0.15f;
        [SerializeField] private float eyeHeight = 1.7f;
        [SerializeField] private Color eyeColor = Color.white;
        [SerializeField] private float eyeEmissionIntensity = 2f;

        [Header("Animation")]
        [SerializeField] private bool eyesPulse = true;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseMinIntensity = 1f;
        [SerializeField] private float pulseMaxIntensity = 3f;

        [Header("Behavior")]
        [SerializeField] private bool hoverEffect = true;
        [SerializeField] private float hoverAmount = 0.1f;
        [SerializeField] private float hoverSpeed = 1f;

        // References
        private GameObject body;
        private GameObject leftEye;
        private GameObject rightEye;
        private Material bodyMaterial;
        private Material eyeMaterial;
        private Vector3 startPosition;

        private void Awake()
        {
            CreateVisuals();
            startPosition = transform.position;
        }

        private void Update()
        {
            // Eye pulse animation
            if (eyesPulse && eyeMaterial != null)
            {
                float pulse = Mathf.Lerp(pulseMinIntensity, pulseMaxIntensity,
                    (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);
                eyeMaterial.SetColor("_EmissionColor", eyeColor * pulse);
            }

            // Hover effect
            if (hoverEffect)
            {
                float hover = Mathf.Sin(Time.time * hoverSpeed) * hoverAmount;
                transform.position = startPosition + Vector3.up * hover;
            }
        }

        private void CreateVisuals()
        {
            // Create body (capsule)
            body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(transform);
            body.transform.localPosition = Vector3.up * (bodyHeight * 0.5f);
            body.transform.localScale = new Vector3(bodyWidth, bodyHeight * 0.5f, bodyWidth);

            // Remove collider from visual (DialogueEntity handles interaction)
            Destroy(body.GetComponent<Collider>());

            // Create black unlit material for body
            bodyMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            bodyMaterial.color = bodyColor;
            body.GetComponent<Renderer>().material = bodyMaterial;

            // Create head (sphere) - slightly wider
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(transform);
            head.transform.localPosition = Vector3.up * (bodyHeight - 0.25f);
            head.transform.localScale = Vector3.one * (bodyWidth * 1.2f);
            Destroy(head.GetComponent<Collider>());
            head.GetComponent<Renderer>().material = bodyMaterial;

            // Create eye material (emissive white)
            eyeMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            eyeMaterial.color = eyeColor;
            eyeMaterial.EnableKeyword("_EMISSION");
            eyeMaterial.SetColor("_EmissionColor", eyeColor * eyeEmissionIntensity);

            // Create left eye
            leftEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftEye.name = "LeftEye";
            leftEye.transform.SetParent(transform);
            leftEye.transform.localPosition = new Vector3(-eyeSpacing, eyeHeight, bodyWidth * 0.5f);
            leftEye.transform.localScale = Vector3.one * eyeSize;
            Destroy(leftEye.GetComponent<Collider>());
            leftEye.GetComponent<Renderer>().material = eyeMaterial;

            // Create right eye
            rightEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightEye.name = "RightEye";
            rightEye.transform.SetParent(transform);
            rightEye.transform.localPosition = new Vector3(eyeSpacing, eyeHeight, bodyWidth * 0.5f);
            rightEye.transform.localScale = Vector3.one * eyeSize;
            Destroy(rightEye.GetComponent<Collider>());
            rightEye.GetComponent<Renderer>().material = eyeMaterial;
        }

        /// <summary>
        /// Set eye color at runtime
        /// </summary>
        public void SetEyeColor(Color color)
        {
            eyeColor = color;
            if (eyeMaterial != null)
            {
                eyeMaterial.color = color;
                eyeMaterial.SetColor("_EmissionColor", color * eyeEmissionIntensity);
            }
        }

        /// <summary>
        /// Set body color at runtime
        /// </summary>
        public void SetBodyColor(Color color)
        {
            bodyColor = color;
            if (bodyMaterial != null)
            {
                bodyMaterial.color = color;
            }
        }

        private void OnDrawGizmos()
        {
            // Preview in editor
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(transform.position + Vector3.up * bodyHeight * 0.5f,
                new Vector3(bodyWidth, bodyHeight, bodyWidth));

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position + new Vector3(-eyeSpacing, eyeHeight, bodyWidth * 0.5f), eyeSize);
            Gizmos.DrawWireSphere(transform.position + new Vector3(eyeSpacing, eyeHeight, bodyWidth * 0.5f), eyeSize);
        }
    }
}
