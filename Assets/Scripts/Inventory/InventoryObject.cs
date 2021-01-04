using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

public enum InterfaceType 
{
    Inventory,
    Equipment,
    Merchant,
    Chest
}

namespace Inventory
{
    [CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System/Inventory")]
    public class InventoryObject : ScriptableObject
    {
        public string savePath;
        public ItemDatabaseObject database;
        public InterfaceType type;
        public Inventory Container;
        public InventorySlot[] GetSlots
        {
            get { return Container.Slots; }
        }

        public bool AddItem(Item _item, int _amount)
        {
            if (EmptySlotCount <= 0)
                return false;

            InventorySlot slot = FindItemOnInventory(_item);
            if (!database.ItemObjects[_item.Id].stackable || slot == null)
            {
                SetEmptySlot(_item, _amount);
                return true;
            }
            slot.AddAmount(_amount);
            return true;
        }

        private InventorySlot FindItemOnInventory(Item _item)
        {
            for (int i = 0; i < GetSlots.Length; i++)
            {
                if (GetSlots[i].item.Id == _item.Id)
                    return GetSlots[i];
            }
            return null;
        }

        public int EmptySlotCount
        {
            get
            {
                int counter = 0;
                for (int i = 0; i < GetSlots.Length; i++)
                {
                    if (GetSlots[i].item.Id <= -1)
                        counter++;
                }
                return counter;
            }
        }

        private InventorySlot SetEmptySlot(Item _item, int _amount)
        {
            for (int i = 0; i < GetSlots.Length; i++)
            {
                if (GetSlots[i].item.Id < 0)
                {
                    GetSlots[i].UpdateSlot(_item, _amount);
                    return GetSlots[i]; 
                }
            }

            return null;
        }

        public virtual void SwapItems(InventorySlot item1, InventorySlot item2)
        {
            if (item2.CanPlaceInSlot(item1.ItemObject) && item1.CanPlaceInSlot(item2.ItemObject))
            {
                if (item1.parent.inventory.type == InterfaceType.Merchant && item2.parent.inventory.type != InterfaceType.Merchant)
                {
                  //  Debug.Log("zaplat ");
                    if (item1.ItemObject != null && item2.ItemObject == null && PlayerManager.LocalPlayer.Wallet.AddCoins(-item1.ItemObject.price))
                    {
                        item2.UpdateSlot(item1.item, item1.amount);
                    }
                }
                else if
                (item1.parent.inventory.type != InterfaceType.Merchant && item2.parent.inventory.type == InterfaceType.Merchant)
                {
                  //  Debug.Log("dostanes prachy ");
                    if (item1.ItemObject != null)
                    {
                        PlayerManager.LocalPlayer.Wallet.AddCoins(item1.ItemObject.price / 3);
                        item1.RemoveItem();
                    }
                }
                else if (item1.parent.inventory.type == InterfaceType.Merchant && item2.parent.inventory.type == InterfaceType.Merchant)
                {
                    Debug.Log("nelze hybat s inventarem merchanta ");
                }
                else
                {
                    InventorySlot temp = new InventorySlot(item2.item, item2.amount);
                    item2.UpdateSlot(item1.item, item1.amount);
                    item1.UpdateSlot(temp.item, temp.amount);
                }
            }
        }

        public void RemoveItem(Item _item)
        {
            for (int i = 0; i < GetSlots.Length; i++)
            {
                if (GetSlots[i].item == _item)
                {
                    GetSlots[i].UpdateSlot(null, 0);
                }
            }
        }

        [ContextMenu("Save")]
        public void Save()
        {
            Debug.Log("Saving " + name);

            #region Optional Save
            string saveData = JsonUtility.ToJson(Container, true);
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(string.Concat(Application.persistentDataPath, savePath));
            bf.Serialize(file, saveData);
            file.Close();
            #endregion
            /*
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Create, FileAccess.Write);
            formatter.Serialize(stream, Container);
            stream.Close();*/
        }

        [ContextMenu("Load")]
        public void Load()
        {
            Debug.Log("Loading " + name);

            if (File.Exists(string.Concat(Application.persistentDataPath, savePath)))
            {
                #region Optional Load
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(string.Concat(Application.persistentDataPath, savePath), FileMode.Open, FileAccess.Read);
                JsonUtility.FromJsonOverwrite(bf.Deserialize(file).ToString(), Container);
                file.Close();
                #endregion

                //IFormatter formatter = new BinaryFormatter();
                //Stream stream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Open, FileAccess.Read);
                //Inventory newContainer = (Inventory)formatter.Deserialize(stream);
                //for (int i = 0; i < GetSlots.Length; i++)
                //{
                //    GetSlots[i].UpdateSlot(newContainer.Slots[i].item, newContainer.Slots[i].amount);
                //}
                //stream.Close();
            }
        }

        [ContextMenu("Clear")]
        public void Clear()
        {
            Container.Clear();
            savePath = "/name.save";
        }
    }

}
