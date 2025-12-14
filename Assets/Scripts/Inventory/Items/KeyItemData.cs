using UnityEngine;

namespace Streets.Inventory
{
    [CreateAssetMenu(fileName = "NewKeyItem", menuName = "Streets/Items/KeyItem")]
    public class KeyItemData : ItemData
    {
        [Header("Key Item Settings")]
        [Tooltip("Unique identifier for quest/trigger checks")]
        public string keyId;

        [Tooltip("Can this item be dropped?")]
        public bool canDrop = false;

        private void OnValidate()
        {
            itemType = ItemType.KeyItem;
            isStackable = false;
            maxStackSize = 1;
        }
    }
}
