using UnityEngine;

namespace Streets.Inventory
{
    [CreateAssetMenu(fileName = "NewConsumable", menuName = "Streets/Items/Consumable")]
    public class ConsumableData : ItemData
    {
        [Header("Health Effects")]
        public float healthRestore;
        public float healthDamage;

        [Header("Intoxication")]
        [Tooltip("Amount of intoxication added when consumed (0-100 scale)")]
        [Range(0f, 50f)]
        public float intoxicationAmount;

        private void OnValidate()
        {
            itemType = ItemType.Consumable;
            if (maxStackSize < 1) maxStackSize = 1;
        }
    }
}
