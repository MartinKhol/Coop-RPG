using Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailsUI : MonoBehaviour
{
    public Text itemName;
    public Text itemStats;
    public Text itemDescription;
    public Text itemPrice;
    public Text itemType;

    public Vector3 offset;

    void Start()
    {
        gameObject.SetActive(false);
    }

    public void ShowStats(InventorySlot slot)
    {
        var item = slot.item;
        var itemObject = slot.ItemObject;

        if (item.Id != itemObject.data.Id)
        {
            Debug.LogWarning("doslo k pregenerovani a item ma spatne id");
            slot.parent.inventory.database.UpdateID();
            /*
            slot.UpdateSlot(itemObject.CreateItem(), slot.amount);
            item = slot.item;*/
        }

        gameObject.SetActive(true);
        Vector3 slotPosition = slot.slotDisplay.transform.position;

        var halfScreen = Screen.width / 2;
        if (Input.mousePosition.x > halfScreen)
        {
            var leftOffset = offset;
            leftOffset.x *= -1;
            transform.position = slotPosition + leftOffset;
        }
        else
        {
            transform.position = slotPosition + offset;
        }

        itemName.text = itemObject.data.Name;
        itemType.text = itemObject.type.ToString();
        itemStats.text = "";

        foreach (var buff in item.buffs)
        {
            if (buff.value == 0) continue;
            switch (buff.attribute)
            {
                case Attributes.AttackSpeed:
                    itemStats.text = string.Concat(itemStats.text, "Attack Speed: ", buff.value, "\n");
                    break;
                case Attributes.MagicDamage:
                    itemStats.text = string.Concat(itemStats.text, "Magic Damage: ", buff.value, "\n");
                    break;
                case Attributes.Health:
                    itemStats.text = string.Concat(itemStats.text, "Health: ", buff.value, "\n");
                    break;
                case Attributes.PhysicalDamage:
                    itemStats.text = string.Concat(itemStats.text, "Physical Damage: ", buff.value, "\n");
                    break;
                case Attributes.Armor:
                    itemStats.text = string.Concat(itemStats.text, "Armor: ", buff.value, "\n");
                    break;
                default:
                    break;
            }
        }

        itemDescription.text = itemObject.description;
        itemPrice.text = itemObject.price.ToString();
        StartCoroutine(CloseStats());
    }

    IEnumerator CloseStats()
    {
        yield return new WaitUntil(() => MouseData.slotHoveredOver == null);
        yield return new WaitForSeconds(0.1f);
        if (MouseData.slotHoveredOver == null || MouseData.interfaceMouseIsOver.slotsOnInterface[MouseData.slotHoveredOver].ItemObject == null)
            gameObject.SetActive(false);
    }
}
