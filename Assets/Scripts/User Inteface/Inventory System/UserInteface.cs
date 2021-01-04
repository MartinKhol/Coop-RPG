using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inventory;
using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Photon.Realtime;
using System.Runtime.InteropServices;
using Launcher;

namespace Inventory
{
    public abstract class UserInteface : MonoBehaviour
    {
        public InventoryObject inventory;

        public Dictionary<GameObject, InventorySlot> slotsOnInterface = new Dictionary<GameObject, InventorySlot>();
        private Vector2 dropOffset = new Vector2(1f, 0f);

        private void Awake()
        {
            for (int i = 0; i < inventory.Container.Slots.Length; i++)
            {
                inventory.Container.Slots[i].parent = this;
                inventory.Container.Slots[i].OnAfterUpdate += OnSlotUpdate;
            }
            CreateSlots();

            AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { OnEnterInterface(gameObject); });
            AddEvent(gameObject, EventTriggerType.PointerExit, delegate { OnExitInterface(gameObject); });

            slotsOnInterface.UpdateSlotDisplay();
        }

        private void OnSlotUpdate(InventorySlot _slot)
        {
           // print(string.Concat("updating slot ", _slot.slotDisplay));

            if (_slot.item.Id > -1)
            {
                _slot.slotDisplay.transform.GetChild(0).GetComponentInChildren<Image>().sprite = _slot.ItemObject.uiDisplay;
                _slot.slotDisplay.transform.GetChild(0).GetComponentInChildren<Image>().color = new Color(1, 1, 1, 1);
                _slot.slotDisplay.transform.GetComponentInChildren<TextMeshProUGUI>().text = _slot.amount == 1 ? "" : _slot.amount.ToString("n0");
            }
            else
            {
                _slot.slotDisplay.transform.GetChild(0).GetComponentInChildren<Image>().sprite = null;
                _slot.slotDisplay.transform.GetChild(0).GetComponentInChildren<Image>().color = new Color(1, 1, 1, 0);
                _slot.slotDisplay.transform.GetComponentInChildren<TextMeshProUGUI>().text = "";
            }
        }

        private void OnExitInterface(GameObject obj)
        {
            MouseData.interfaceMouseIsOver = null;
        }

        private void OnEnterInterface(GameObject obj)
        {
            MouseData.interfaceMouseIsOver = obj.GetComponent<UserInteface>();
        }

        protected void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
        {
            EventTrigger trigger = obj.GetComponent<EventTrigger>();
            var eventTrigger = new EventTrigger.Entry();
            eventTrigger.eventID = type;
            eventTrigger.callback.AddListener(action);
            trigger.triggers.Add(eventTrigger);
        }

        protected abstract void CreateSlots();

        protected void OnDrag(GameObject obj)
        {
            if (MouseData.tempItemBeingDragged != null)
            {
                MouseData.tempItemBeingDragged.GetComponent<RectTransform>().position = Input.mousePosition;
            }
        }

        protected void OnDragEnd(GameObject obj)
        {
            Destroy(MouseData.tempItemBeingDragged);

            if (MouseData.interfaceMouseIsOver == null && slotsOnInterface[obj].parent.inventory.type != InterfaceType.Merchant)
            {
                var item = slotsOnInterface[obj].item;

                PlayerManager.LocalPlayer.CallDropRPC(item.Id, PlayerManager.LocalPlayer.transform.position + (Vector3)dropOffset * PlayerManager.LocalPlayer.transform.localScale.x, item);
                slotsOnInterface[obj].RemoveItem();
                return;
            }
            if (MouseData.slotHoveredOver != null)
            {
                InventorySlot mouseHoverSlotData = MouseData.interfaceMouseIsOver.slotsOnInterface[MouseData.slotHoveredOver];
                inventory.SwapItems(slotsOnInterface[obj], mouseHoverSlotData);
            }
        }

        protected void OnDragStart(GameObject obj)
        {
            MouseData.tempItemBeingDragged = CreateTempItem(obj);
        }

        public GameObject CreateTempItem(GameObject obj)
        {
            GameObject tempItem = null;
            if (slotsOnInterface[obj].item.Id >= 0)
            {
                tempItem = new GameObject();
                var rt = tempItem.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(80, 80);
                tempItem.transform.SetParent(transform.parent);
                var img = tempItem.AddComponent<Image>();
                img.sprite = slotsOnInterface[obj].ItemObject.uiDisplay;
                img.raycastTarget = false;
                img.preserveAspect = true;
            }
            return tempItem;
        }


        protected void OnExit(GameObject obj)
        {
            MouseData.slotHoveredOver = null;
        }

        protected void OnEnter(GameObject obj)
        {
            MouseData.slotHoveredOver = obj;

            if ((MouseData.interfaceMouseIsOver != null) &&
                (MouseData.slotHoveredOver != null))
            {
                InventorySlot mouseHoverSlotData = MouseData.interfaceMouseIsOver.slotsOnInterface[MouseData.slotHoveredOver];
                if ((mouseHoverSlotData.item.Id > -1) && mouseHoverSlotData.ItemObject != null)
                    PlayerUI.Instance.itemDetails.ShowStats(mouseHoverSlotData);
            }
        }
    }

    public static class MouseData
    {
        public static UserInteface interfaceMouseIsOver;
        public static GameObject tempItemBeingDragged;
        public static GameObject slotHoveredOver;
    }

    public static class ExtensionMethods
    {
        public static void UpdateSlotDisplay(this Dictionary<GameObject, InventorySlot> _slotsOnInterface)
        {

            foreach (KeyValuePair<GameObject, InventorySlot> slot in _slotsOnInterface)
            {
                if (slot.Value.item.Id > -1)
                {
                    slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().sprite = slot.Value.ItemObject.uiDisplay;
                    slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().color = new Color(1, 1, 1, 1);
                    slot.Key.transform.GetComponentInChildren<TextMeshProUGUI>().text = slot.Value.amount == 1 ? "" : slot.Value.amount.ToString("n0");
                }
                else
                {
                    slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().sprite = null;
                    slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().color = new Color(1, 1, 1, 0);
                    slot.Key.transform.GetComponentInChildren<TextMeshProUGUI>().text = "";
                }
            }
        }
    }
}

