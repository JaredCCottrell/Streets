using System;
using UnityEngine;
using Streets.Survival;
using Streets.Effects;

namespace Streets.Inventory
{
    public class InventorySystem : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private int maxSlots = 12;

        [Header("Survival References")]
        [SerializeField] private HealthSystem healthSystem;

        [Header("Item Dropping")]
        [SerializeField] private ItemPickup dropItemPrefab;
        [SerializeField] private Transform dropPoint;
        [SerializeField] private float dropForce = 3f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;

        // State
        private InventorySlot[] slots;

        // Events
        public event Action<int, InventorySlot> OnSlotChanged;
        public event Action OnInventoryFull;
        public event Action<ItemData> OnItemAdded;
        public event Action<ItemData> OnItemRemoved;
        public event Action<ItemData> OnItemUsed;

        // Properties
        public int MaxSlots => maxSlots;
        public InventorySlot[] Slots => slots;
        public int UsedSlots
        {
            get
            {
                int count = 0;
                for (int i = 0; i < slots.Length; i++)
                {
                    if (!slots[i].IsEmpty) count++;
                }
                return count;
            }
        }
        public bool IsFull => UsedSlots >= maxSlots;

        private void Awake()
        {
            slots = new InventorySlot[maxSlots];
            for (int i = 0; i < maxSlots; i++)
            {
                slots[i] = new InventorySlot();
            }
        }

        public bool AddItem(ItemData item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;

            int remaining = quantity;

            // Try to stack with existing slots first
            if (item.isStackable)
            {
                for (int i = 0; i < slots.Length && remaining > 0; i++)
                {
                    if (slots[i].item == item && slots[i].CanAddMore)
                    {
                        remaining = slots[i].AddQuantity(remaining);
                        OnSlotChanged?.Invoke(i, slots[i]);
                    }
                }
            }

            // Add to empty slots
            for (int i = 0; i < slots.Length && remaining > 0; i++)
            {
                if (slots[i].IsEmpty)
                {
                    slots[i].item = item;
                    int toAdd = item.isStackable ? Math.Min(remaining, item.maxStackSize) : 1;
                    slots[i].quantity = toAdd;
                    remaining -= toAdd;
                    OnSlotChanged?.Invoke(i, slots[i]);
                }
            }

            if (remaining < quantity)
            {
                OnItemAdded?.Invoke(item);
                PlaySound(item.pickupSound);
            }

            if (remaining > 0)
            {
                OnInventoryFull?.Invoke();
                return false;
            }

            return true;
        }

        public bool RemoveItem(ItemData item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;

            int remaining = quantity;

            for (int i = slots.Length - 1; i >= 0 && remaining > 0; i--)
            {
                if (slots[i].item == item)
                {
                    int removed = slots[i].RemoveQuantity(remaining);
                    remaining -= removed;
                    OnSlotChanged?.Invoke(i, slots[i]);
                }
            }

            if (remaining < quantity)
            {
                OnItemRemoved?.Invoke(item);
                return true;
            }

            return false;
        }

        public bool RemoveItemAt(int slotIndex, int quantity = 1)
        {
            if (slotIndex < 0 || slotIndex >= slots.Length) return false;
            if (slots[slotIndex].IsEmpty) return false;

            ItemData item = slots[slotIndex].item;
            int removed = slots[slotIndex].RemoveQuantity(quantity);
            OnSlotChanged?.Invoke(slotIndex, slots[slotIndex]);

            if (removed > 0)
            {
                OnItemRemoved?.Invoke(item);
                return true;
            }

            return false;
        }

        public void UseItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slots.Length) return;
            if (slots[slotIndex].IsEmpty) return;

            ItemData item = slots[slotIndex].item;

            if (item is ConsumableData consumable)
            {
                ApplyConsumableEffects(consumable);
                RemoveItemAt(slotIndex, 1);
                PlaySound(item.useSound);
                OnItemUsed?.Invoke(item);
            }
            else if (item is EquipmentData equipment)
            {
                // Equipment use - could trigger equip/unequip
                OnItemUsed?.Invoke(item);
            }
            else if (item is KeyItemData keyItem)
            {
                // Key items typically can't be "used" directly
                OnItemUsed?.Invoke(item);
            }
        }

        private void ApplyConsumableEffects(ConsumableData consumable)
        {
            // Health effects
            if (consumable.healthRestore > 0 && healthSystem != null)
            {
                healthSystem.Heal(consumable.healthRestore);
            }
            if (consumable.healthDamage > 0 && healthSystem != null)
            {
                healthSystem.TakeDamage(consumable.healthDamage);
            }

            // Intoxication effect
            if (consumable.intoxicationAmount > 0)
            {
                var intoxSystem = FindObjectOfType<IntoxicationSystem>();
                if (intoxSystem != null)
                {
                    intoxSystem.AddIntoxication(consumable.intoxicationAmount);
                }
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        public bool HasItem(ItemData item)
        {
            return GetItemCount(item) > 0;
        }

        public bool HasKeyItem(string keyId)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].item is KeyItemData keyItem && keyItem.keyId == keyId)
                {
                    return true;
                }
            }
            return false;
        }

        public int GetItemCount(ItemData item)
        {
            int count = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].item == item)
                {
                    count += slots[i].quantity;
                }
            }
            return count;
        }

        public InventorySlot GetSlot(int index)
        {
            if (index < 0 || index >= slots.Length) return null;
            return slots[index];
        }

        public int FindItemSlot(ItemData item)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].item == item)
                {
                    return i;
                }
            }
            return -1;
        }

        public void SwapSlots(int indexA, int indexB)
        {
            if (indexA < 0 || indexA >= slots.Length) return;
            if (indexB < 0 || indexB >= slots.Length) return;
            if (indexA == indexB) return;

            InventorySlot temp = slots[indexA];
            slots[indexA] = slots[indexB];
            slots[indexB] = temp;

            OnSlotChanged?.Invoke(indexA, slots[indexA]);
            OnSlotChanged?.Invoke(indexB, slots[indexB]);
        }

        public void DropItem(int slotIndex, int quantityToDrop = -1)
        {
            if (slotIndex < 0 || slotIndex >= slots.Length) return;
            if (slots[slotIndex].IsEmpty) return;

            ItemData item = slots[slotIndex].item;
            int currentQuantity = slots[slotIndex].quantity;

            // Key items typically can't be dropped
            if (item is KeyItemData keyItem && !keyItem.canDrop)
            {
                return;
            }

            // If quantityToDrop is -1 or greater than current, drop all
            int dropAmount = (quantityToDrop < 0 || quantityToDrop > currentQuantity)
                ? currentQuantity
                : quantityToDrop;

            // Spawn dropped item in world
            SpawnDroppedItem(item, dropAmount);

            // Remove from inventory
            RemoveItemAt(slotIndex, dropAmount);
        }

        private void SpawnDroppedItem(ItemData item, int quantity)
        {
            if (dropItemPrefab == null || item == null) return;

            // Determine spawn position
            Vector3 spawnPosition;
            Vector3 dropDirection;

            if (dropPoint != null)
            {
                spawnPosition = dropPoint.position;
                dropDirection = dropPoint.forward;
            }
            else
            {
                // Fallback: spawn in front of player
                spawnPosition = transform.position + transform.forward * 1.5f + Vector3.up * 0.5f;
                dropDirection = transform.forward;
            }

            // Instantiate the pickup
            ItemPickup droppedItem = Instantiate(dropItemPrefab, spawnPosition, Quaternion.identity);
            droppedItem.Initialize(item, quantity);

            // Add force to throw the item
            Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 force = (dropDirection + Vector3.up * 0.5f).normalized * dropForce;
                rb.AddForce(force, ForceMode.Impulse);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (!slots[i].IsEmpty)
                {
                    slots[i].Clear();
                    OnSlotChanged?.Invoke(i, slots[i]);
                }
            }
        }
    }
}
