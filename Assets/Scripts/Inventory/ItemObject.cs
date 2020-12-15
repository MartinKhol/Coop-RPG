using System.IO;
using UnityEngine;
using System.Collections.Generic;

public enum Attributes
{
    AttackSpeed,
    MagicDamage,
    Health,
    PhysicalDamage,
    Armor
}

namespace Inventory
{
    public enum ItemType
    {
        Sword,
        Staff,
        Bow,
        Dagger,
        Trinket,
        Ankh
    }

    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory System/Items/Item")]
    public class ItemObject : ScriptableObject
    {

        public Sprite uiDisplay;
        public bool stackable;
        public ItemType type;
        [TextArea]
        public string description;
        public int price;
        public Item data = new Item();

        public Item CreateItem()
        {
            Item newItem = new Item(this);
            return newItem;
        }
    }

    [System.Serializable]
    public class Item
    {
        public string Name;
        public int Id = -1;
        public ItemBuff[] buffs;
        public Item()
        {
            Name = "";
            Id = -1;
        }
        public Item(ItemObject item)
        {
            Name = item.data.Name;
            Id = item.data.Id;
            buffs = new ItemBuff[item.data.buffs.Length];
            for (int i = 0; i < buffs.Length; i++)
            {
                buffs[i] = new ItemBuff(item.data.buffs[i].min, item.data.buffs[i].max)
                {
                    attribute = item.data.buffs[i].attribute
                };
            }
        }
    }

  
    [System.Serializable]
    public class ItemBuff : IModifier
    {
        public Attributes attribute;
        public int value;
        public int min;
        public int max;
        public ItemBuff(int _min, int _max)
        {
            min = _min;
            max = _max;
            GenerateValue();
        }

        public void AddValue(ref int baseValue)
        {
            baseValue += value;
        }

        public void GenerateValue()
        {
            value = UnityEngine.Random.Range(min, max);
        }
    }
}


