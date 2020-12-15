using UnityEngine;

namespace Inventory
{
    public delegate void SlotUpdated(InventorySlot _slot);

    [System.Serializable]
    public class InventorySlot
    {
        public ItemType[] AllowedItems = new ItemType[0];
        [System.NonSerialized]
        public UserInteface parent;
        [System.NonSerialized]
        public GameObject slotDisplay;
        [System.NonSerialized]
        public SlotUpdated OnAfterUpdate;
        [System.NonSerialized]
        public SlotUpdated OnBeforeUpdate;
        public Item item;
        public int amount;

        public ItemObject ItemObject
        {
            get
            {
                if (item.Id >= 0)
                {
                    return parent.inventory.database.ItemObjects[item.Id];
                }
                return null;
            }
        }
        public InventorySlot(Item _item, int _amount)
        {
            UpdateSlot(_item, _amount);
        }
        public InventorySlot()
        {
            UpdateSlot(new Item(), 0);
        }
        public void UpdateSlot(Item _item, int _amount)
        {
            if (OnBeforeUpdate != null)
                OnBeforeUpdate.Invoke(this);
            item = _item;
            amount = _amount;
            if (OnAfterUpdate != null)
                OnAfterUpdate.Invoke(this);
        }
        public void AddAmount(int value)
        {
            UpdateSlot(item, amount + value);
        }
        public bool CanPlaceInSlot(ItemObject _itemObject)
        {
            if (AllowedItems.Length < 1 || _itemObject == null || _itemObject.data.Id < 0)
                return true;

            for (int i = 0; i < AllowedItems.Length; i++)
            {
                if (_itemObject.type == AllowedItems[i])
                    return true;
            }
            return false;
        }
        public void SetAllowedItems(ItemType[] _allowedItems)
        {
            AllowedItems = _allowedItems;
        }
        public void RemoveItem()
        {
            UpdateSlot(new Item(), 0);
        }
    }
}