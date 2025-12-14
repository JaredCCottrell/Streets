using System;

namespace Streets.Inventory
{
    [Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int quantity;

        public bool IsEmpty => item == null || quantity <= 0;
        public bool IsFull => item != null && quantity >= item.maxStackSize;
        public bool CanAddMore => item != null && item.isStackable && quantity < item.maxStackSize;
        public int RemainingSpace => item != null ? item.maxStackSize - quantity : 0;

        public InventorySlot()
        {
            item = null;
            quantity = 0;
        }

        public InventorySlot(ItemData item, int quantity = 1)
        {
            this.item = item;
            this.quantity = quantity;
        }

        public void Clear()
        {
            item = null;
            quantity = 0;
        }

        public int AddQuantity(int amount)
        {
            if (item == null) return amount;

            int canAdd = item.maxStackSize - quantity;
            int toAdd = Math.Min(canAdd, amount);
            quantity += toAdd;
            return amount - toAdd;
        }

        public int RemoveQuantity(int amount)
        {
            int toRemove = Math.Min(quantity, amount);
            quantity -= toRemove;

            if (quantity <= 0)
            {
                Clear();
            }

            return toRemove;
        }
    }
}
