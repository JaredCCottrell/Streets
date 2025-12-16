using UnityEngine;

namespace Streets.Road
{
    public class RoadPropBuilder : MonoBehaviour
    {
        [Header("Prop Type")]
        [SerializeField] private PropType propType = PropType.Barrel;

        [Header("Materials")]
        [SerializeField] private Material metalMaterial;
        [SerializeField] private Material orangeMaterial;
        [SerializeField] private Material signMaterial;
        [SerializeField] private Material glassMaterial;

        public enum PropType
        {
            Barrel,
            Guardrail,
            StreetLight,
            SpeedLimitSign,
            MileMarker,
            TrafficCone,
            AbandonedCar,
            Jersey_Barrier
        }

        [ContextMenu("Build Prop")]
        public void BuildProp()
        {
            ClearChildren();

            switch (propType)
            {
                case PropType.Barrel:
                    BuildBarrel();
                    break;
                case PropType.Guardrail:
                    BuildGuardrail();
                    break;
                case PropType.StreetLight:
                    BuildStreetLight();
                    break;
                case PropType.SpeedLimitSign:
                    BuildSpeedLimitSign();
                    break;
                case PropType.MileMarker:
                    BuildMileMarker();
                    break;
                case PropType.TrafficCone:
                    BuildTrafficCone();
                    break;
                case PropType.AbandonedCar:
                    BuildAbandonedCar();
                    break;
                case PropType.Jersey_Barrier:
                    BuildJerseyBarrier();
                    break;
            }
        }

        private void ClearChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        private static Material _baseMaterial;

        private Material GetMaterial(Color color)
        {
            // Get base material from a primitive (works with any render pipeline)
            if (_baseMaterial == null)
            {
                GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _baseMaterial = temp.GetComponent<Renderer>().sharedMaterial;
                DestroyImmediate(temp);
            }

            Material mat = new Material(_baseMaterial);

            // Set color based on shader type
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }
            if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", color);
            }

            return mat;
        }

        private void BuildBarrel()
        {
            // Orange traffic barrel
            GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = "Barrel";
            barrel.transform.SetParent(transform);
            barrel.transform.localPosition = new Vector3(0, 0.5f, 0);
            barrel.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            barrel.GetComponent<Renderer>().material = orangeMaterial ?? GetMaterial(new Color(1f, 0.4f, 0f));

            // White stripes
            GameObject stripe1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stripe1.name = "Stripe1";
            stripe1.transform.SetParent(barrel.transform);
            stripe1.transform.localPosition = new Vector3(0, 0.3f, 0);
            stripe1.transform.localScale = new Vector3(1.05f, 0.1f, 1.05f);
            stripe1.GetComponent<Renderer>().material = GetMaterial(Color.white);
            DestroyImmediate(stripe1.GetComponent<Collider>());

            GameObject stripe2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stripe2.name = "Stripe2";
            stripe2.transform.SetParent(barrel.transform);
            stripe2.transform.localPosition = new Vector3(0, -0.3f, 0);
            stripe2.transform.localScale = new Vector3(1.05f, 0.1f, 1.05f);
            stripe2.GetComponent<Renderer>().material = GetMaterial(Color.white);
            DestroyImmediate(stripe2.GetComponent<Collider>());
        }

        private void BuildGuardrail()
        {
            // Metal guardrail section
            GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = "Rail";
            rail.transform.SetParent(transform);
            rail.transform.localPosition = new Vector3(0, 0.6f, 0);
            rail.transform.localScale = new Vector3(0.1f, 0.3f, 4f);
            rail.GetComponent<Renderer>().material = metalMaterial ?? GetMaterial(new Color(0.7f, 0.7f, 0.7f));

            // Posts
            for (int i = 0; i < 3; i++)
            {
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
                post.name = $"Post_{i}";
                post.transform.SetParent(transform);
                post.transform.localPosition = new Vector3(0, 0.35f, -1.5f + i * 1.5f);
                post.transform.localScale = new Vector3(0.08f, 0.7f, 0.08f);
                post.GetComponent<Renderer>().material = metalMaterial ?? GetMaterial(new Color(0.5f, 0.5f, 0.5f));
            }
        }

        private void BuildStreetLight()
        {
            Material metal = metalMaterial ?? GetMaterial(new Color(0.3f, 0.3f, 0.3f));

            // Pole
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "Pole";
            pole.transform.SetParent(transform);
            pole.transform.localPosition = new Vector3(0, 3f, 0);
            pole.transform.localScale = new Vector3(0.15f, 3f, 0.15f);
            pole.GetComponent<Renderer>().material = metal;

            // Arm
            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            arm.name = "Arm";
            arm.transform.SetParent(transform);
            arm.transform.localPosition = new Vector3(1f, 5.8f, 0);
            arm.transform.localRotation = Quaternion.Euler(0, 0, 90);
            arm.transform.localScale = new Vector3(0.08f, 1f, 0.08f);
            arm.GetComponent<Renderer>().material = metal;

            // Light housing
            GameObject housing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            housing.name = "Housing";
            housing.transform.SetParent(transform);
            housing.transform.localPosition = new Vector3(1.8f, 5.6f, 0);
            housing.transform.localScale = new Vector3(0.6f, 0.2f, 0.3f);
            housing.GetComponent<Renderer>().material = metal;

            // Light (emissive)
            GameObject light = GameObject.CreatePrimitive(PrimitiveType.Cube);
            light.name = "Light";
            light.transform.SetParent(housing.transform);
            light.transform.localPosition = new Vector3(0, -0.4f, 0);
            light.transform.localScale = new Vector3(0.8f, 0.2f, 0.8f);
            Material lightMat = GetMaterial(new Color(1f, 0.95f, 0.8f));
            light.GetComponent<Renderer>().material = lightMat;
            DestroyImmediate(light.GetComponent<Collider>());
        }

        private void BuildSpeedLimitSign()
        {
            Material metal = metalMaterial ?? GetMaterial(new Color(0.4f, 0.4f, 0.4f));

            // Post
            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.name = "Post";
            post.transform.SetParent(transform);
            post.transform.localPosition = new Vector3(0, 1.5f, 0);
            post.transform.localScale = new Vector3(0.1f, 1.5f, 0.1f);
            post.GetComponent<Renderer>().material = metal;

            // Sign face
            GameObject sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.name = "Sign";
            sign.transform.SetParent(transform);
            sign.transform.localPosition = new Vector3(0, 2.8f, 0);
            sign.transform.localScale = new Vector3(0.6f, 0.8f, 0.05f);
            sign.GetComponent<Renderer>().material = signMaterial ?? GetMaterial(Color.white);
        }

        private void BuildMileMarker()
        {
            // Small green mile marker
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "Marker";
            marker.transform.SetParent(transform);
            marker.transform.localPosition = new Vector3(0, 0.4f, 0);
            marker.transform.localScale = new Vector3(0.15f, 0.8f, 0.05f);
            marker.GetComponent<Renderer>().material = GetMaterial(new Color(0f, 0.4f, 0f));
        }

        private void BuildTrafficCone()
        {
            // Orange cone
            GameObject cone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cone.name = "Cone";
            cone.transform.SetParent(transform);
            cone.transform.localPosition = new Vector3(0, 0.25f, 0);
            cone.transform.localScale = new Vector3(0.3f, 0.25f, 0.3f);
            cone.GetComponent<Renderer>().material = orangeMaterial ?? GetMaterial(new Color(1f, 0.4f, 0f));

            // Tip (narrower cylinder)
            GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tip.name = "Tip";
            tip.transform.SetParent(transform);
            tip.transform.localPosition = new Vector3(0, 0.55f, 0);
            tip.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            tip.GetComponent<Renderer>().material = orangeMaterial ?? GetMaterial(new Color(1f, 0.4f, 0f));
            DestroyImmediate(tip.GetComponent<Collider>());

            // White stripe
            GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stripe.name = "Stripe";
            stripe.transform.SetParent(cone.transform);
            stripe.transform.localPosition = new Vector3(0, 0.2f, 0);
            stripe.transform.localScale = new Vector3(0.85f, 0.15f, 0.85f);
            stripe.GetComponent<Renderer>().material = GetMaterial(Color.white);
            DestroyImmediate(stripe.GetComponent<Collider>());

            // Base
            GameObject baseObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseObj.name = "Base";
            baseObj.transform.SetParent(transform);
            baseObj.transform.localPosition = new Vector3(0, 0.02f, 0);
            baseObj.transform.localScale = new Vector3(0.4f, 0.04f, 0.4f);
            baseObj.GetComponent<Renderer>().material = GetMaterial(new Color(0.1f, 0.1f, 0.1f));
        }

        private void BuildAbandonedCar()
        {
            Material rusty = GetMaterial(new Color(0.4f, 0.25f, 0.2f));
            Material dark = GetMaterial(new Color(0.15f, 0.15f, 0.15f));
            Material glass = glassMaterial ?? GetMaterial(new Color(0.2f, 0.3f, 0.3f, 0.5f));

            // Body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(transform);
            body.transform.localPosition = new Vector3(0, 0.6f, 0);
            body.transform.localScale = new Vector3(1.8f, 0.6f, 4f);
            body.GetComponent<Renderer>().material = rusty;

            // Roof/cabin
            GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabin.name = "Cabin";
            cabin.transform.SetParent(transform);
            cabin.transform.localPosition = new Vector3(0, 1.1f, -0.3f);
            cabin.transform.localScale = new Vector3(1.6f, 0.5f, 2f);
            cabin.GetComponent<Renderer>().material = rusty;

            // Wheels
            Vector3[] wheelPositions = {
                new Vector3(-0.8f, 0.3f, 1.2f),
                new Vector3(0.8f, 0.3f, 1.2f),
                new Vector3(-0.8f, 0.3f, -1.2f),
                new Vector3(0.8f, 0.3f, -1.2f)
            };

            for (int i = 0; i < 4; i++)
            {
                GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wheel.name = $"Wheel_{i}";
                wheel.transform.SetParent(transform);
                wheel.transform.localPosition = wheelPositions[i];
                wheel.transform.localRotation = Quaternion.Euler(0, 0, 90);
                wheel.transform.localScale = new Vector3(0.5f, 0.15f, 0.5f);
                wheel.GetComponent<Renderer>().material = dark;
            }

            // Windshield
            GameObject windshield = GameObject.CreatePrimitive(PrimitiveType.Cube);
            windshield.name = "Windshield";
            windshield.transform.SetParent(transform);
            windshield.transform.localPosition = new Vector3(0, 1.0f, 0.8f);
            windshield.transform.localRotation = Quaternion.Euler(20, 0, 0);
            windshield.transform.localScale = new Vector3(1.4f, 0.4f, 0.05f);
            windshield.GetComponent<Renderer>().material = glass;
            DestroyImmediate(windshield.GetComponent<Collider>());
        }

        private void BuildJerseyBarrier()
        {
            // Concrete jersey barrier
            GameObject barrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barrier.name = "Barrier";
            barrier.transform.SetParent(transform);
            barrier.transform.localPosition = new Vector3(0, 0.4f, 0);
            barrier.transform.localScale = new Vector3(0.6f, 0.8f, 3f);
            barrier.GetComponent<Renderer>().material = GetMaterial(new Color(0.6f, 0.6f, 0.6f));

            // Sloped top
            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "Top";
            top.transform.SetParent(transform);
            top.transform.localPosition = new Vector3(0, 0.85f, 0);
            top.transform.localScale = new Vector3(0.3f, 0.1f, 3f);
            top.GetComponent<Renderer>().material = GetMaterial(new Color(0.55f, 0.55f, 0.55f));
            DestroyImmediate(top.GetComponent<Collider>());
        }

#if UNITY_EDITOR
        private void Reset()
        {
            gameObject.name = $"Prop_{propType}";
        }
#endif
    }
}
