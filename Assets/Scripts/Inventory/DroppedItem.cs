using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inventory;
using Photon.Pun;
using UnityEditor;

public class DroppedItem : MonoBehaviourPun, ISerializationCallbackReceiver
{
    [HideInInspector]
    public Item generatedItem;

    [SerializeField]
    private ItemObject item;
    public ItemObject Item
    {
        get
        {
            return item;
        }
        set
        {
            item = value;
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = item.uiDisplay;
        }
    }

    private SpriteRenderer spriteRenderer;

    public void OnBeforeSerialize()
    {
#if UNITY_EDITOR
        if (item != null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = item.uiDisplay;
            EditorUtility.SetDirty(spriteRenderer);
        }
#endif
    }

    public void OnAfterDeserialize()
    {

    }

}