using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MerchantNPC : NPC
{
    public GameObject merchantInventory;

    protected override void Start()
    {
        base.Start();
        merchantInventory.SetActive(false);
    }

    protected override void StartInteraction()
    {
        merchantInventory.SetActive(true);
        PlayerUI.Instance.OpenInventory();
        if (dialogueText.Length > 0)
        {
            DialogueUI.Instance.StartDialogue(dialogueSprite, dialogueText);
        }
    }
    protected override void EndInteraction()
    {
        merchantInventory.SetActive(false);
        DialogueUI.Instance.EndDialogue();
    }

}
