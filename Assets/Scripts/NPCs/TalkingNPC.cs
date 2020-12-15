using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TalkingNPC : NPC
{
    public bool randomOneLine = true;

    protected override void EndInteraction()
    {
        DialogueUI.Instance.EndDialogue();
    }

    protected override void StartInteraction()
    {
        if (dialogueText.Length > 0)
        {
            if (randomOneLine)
            {
                int rng = Random.Range(0, dialogueText.Length);
                string[] oneLine = new string[1];
                oneLine[0] = dialogueText[rng];
                DialogueUI.Instance.StartDialogue(dialogueSprite, oneLine);
            }
            else
                DialogueUI.Instance.StartDialogue(dialogueSprite, dialogueText);
        }
    }
}
