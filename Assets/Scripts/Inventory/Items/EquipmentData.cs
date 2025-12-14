using UnityEngine;

namespace Streets.Inventory
{
    public enum EquipmentSlot
    {
        Hand,
        Head,
        Body
    }

    [CreateAssetMenu(fileName = "NewEquipment", menuName = "Streets/Items/Equipment")]
    public class EquipmentData : ItemData
    {
        [Header("Equipment Settings")]
        public EquipmentSlot slot;
        public GameObject equipPrefab;

        [Header("Stats")]
        public float lightRange;
        public float damage;
        public float defense;

        private void OnValidate()
        {
            itemType = ItemType.Equipment;
            isStackable = false;
            maxStackSize = 1;
        }
    }
}
