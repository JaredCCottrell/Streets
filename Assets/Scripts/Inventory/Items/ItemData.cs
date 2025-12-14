using UnityEngine;

namespace Streets.Inventory
{
    public enum ItemType
    {
        Consumable,
        KeyItem,
        Equipment
    }

    [CreateAssetMenu(fileName = "NewItem", menuName = "Streets/Items/Item")]
    public class ItemData : ScriptableObject
    {
        [Header("Basic Info")]
        public string itemName;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;

        [Header("Item Properties")]
        public ItemType itemType;
        public bool isStackable = true;
        public int maxStackSize = 5;
        public float weight = 0.5f;

        [Header("Audio")]
        public AudioClip useSound;
        public AudioClip pickupSound;
    }
}
