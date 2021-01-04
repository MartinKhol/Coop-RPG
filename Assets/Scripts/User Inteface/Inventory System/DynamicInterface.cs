using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Inventory
{
    public class DynamicInterface : UserInteface
    {
        public GameObject inventoryPrefab;

        private void Start()
        {
            StartCoroutine(WaitForPlayer());
        }

        IEnumerator WaitForPlayer()
        {
            yield return new WaitUntil(()=> PlayerManager.localPlayer != null);
            yield return new WaitForSeconds(.3f);
            slotsOnInterface.UpdateSlotDisplay();
        }

        protected override void CreateSlots()
        {
            slotsOnInterface = new Dictionary<GameObject, InventorySlot>();

            for (int i = 0; i < inventory.GetSlots.Length; i++)
            {
                var obj = Instantiate(inventoryPrefab, transform);

                AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnter(obj); });
                AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExit(obj); });
                AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnDragStart(obj); });
                AddEvent(obj, EventTriggerType.EndDrag, delegate { OnDragEnd(obj); });
                AddEvent(obj, EventTriggerType.Drag, delegate { OnDrag(obj); });
                inventory.GetSlots[i].slotDisplay = obj;
                slotsOnInterface.Add(obj, inventory.GetSlots[i]);
            }
        }
    }
}