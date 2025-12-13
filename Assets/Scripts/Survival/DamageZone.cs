using UnityEngine;

namespace Streets.Survival
{
    public class DamageZone : MonoBehaviour
    {
        [SerializeField] private float damageAmount = 10f;
        [SerializeField] private float damageInterval = 1f;
        [SerializeField] private bool instantKill = false;

        private float damageTimer;

        private void OnTriggerEnter(Collider other)
        {
            TryDamage(other.gameObject);
        }

        private void OnTriggerStay(Collider other)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                TryDamage(other.gameObject);
                damageTimer = 0f;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            damageTimer = 0f;
        }

        private void TryDamage(GameObject target)
        {
            HealthSystem health = target.GetComponent<HealthSystem>();
            if (health != null)
            {
                if (instantKill)
                {
                    health.SetHealth(0);
                }
                else
                {
                    health.TakeDamage(damageAmount);
                }
            }
        }
    }
}
