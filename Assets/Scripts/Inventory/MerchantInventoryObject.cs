using Inventory;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class MerchantInventoryObject : InventoryObject
{
    public override void SwapItems(InventorySlot item1, InventorySlot item2)
    {
        if (item2.CanPlaceInSlot(item1.ItemObject) && item1.CanPlaceInSlot(item2.ItemObject))
        {
            if (item1.parent.inventory.type == InterfaceType.Merchant && item2.parent.inventory.type != InterfaceType.Merchant)
            {
                Debug.Log("zaplat ");
                if (item1.ItemObject != null 
                    && item2.ItemObject == null 
                    && PlayerManager.LocalPlayer.Wallet.AddCoins(-item1.ItemObject.price) )
                {
                    InventorySlot temp = new InventorySlot(item2.item, item2.amount);
                    item2.UpdateSlot(item1.item, item1.amount);
                    item1.UpdateSlot(temp.item, temp.amount);
                }
            }
            else if
            (item1.parent.inventory.type != InterfaceType.Merchant && item2.parent.inventory.type == InterfaceType.Merchant)
            {
                Debug.Log("dostanes prachy ");
                if (item1.ItemObject != null)
                {
                    PlayerManager.LocalPlayer.Wallet.AddCoins(item1.ItemObject.price);
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

}