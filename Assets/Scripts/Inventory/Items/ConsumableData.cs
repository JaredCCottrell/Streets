using UnityEngine;

namespace Streets.Inventory
{
    [CreateAssetMenu(fileName = "NewConsumable", menuName = "Streets/Items/Consumable")]
    public class ConsumableData : ItemData
    {
        [Header("Survival Effects")]
        public float healthRestore;
        public float hungerRestore;
        public float thirstRestore;
        public float sanityRestore;

        [Header("Negative Effects")]
        public float healthDamage;
        public float hungerDrain;
        public float thirstDrain;
        public float sanityDrain;

        private void OnValidate()
        {
            itemType = ItemType.Consumable;
            if (maxStackSize < 1) maxStackSize = 1;
        }
    }
}
