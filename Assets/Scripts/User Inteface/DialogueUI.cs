using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance;

    public Text nextText;
    public Image characterImage;
    public Text dialogueText;

    int currentDialogue = 0;
    string[] text;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void StartDialogue(Sprite characterSprite, string[] text)
    {
        gameObject.SetActive(true);
        characterImage.sprite = characterSprite;
        currentDialogue = 0;
        dialogueText.text = text[currentDialogue];
        this.text = text;

        nextText.enabled = text.Length > 1;
    }

    public void EndDialogue()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(Settings.dialogueKey))
        {
            if (++currentDialogue < text.Length)
            {
                dialogueText.text = text[currentDialogue];
                nextText.enabled = text.Length > (currentDialogue+1);
            }
            /*else
            {
                gameObject.SetActive(false);
            }*/
        }
    }
}
