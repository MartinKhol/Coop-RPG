using UnityEngine;
using System.Collections;
using Inventory;

[CreateAssetMenu(fileName = "Inventory Populator", menuName = "Inventory System/InventoryPopulator")]
public class PopulateInventoryObject : ScriptableObject
{
    public InventoryObject inventoryObject;

    public ItemObject[] itemObjects;

    [ContextMenu("Populate")]
    public void PopulateInventory()
    {
        inventoryObject.Clear();
        foreach (var item in itemObjects)
        {
            inventoryObject.AddItem(item.CreateItem(), 1);
        }
    }
}
