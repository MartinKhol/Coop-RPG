using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NPC : MonoBehaviour
{
    public GameObject interactionText;
    public float interactionRange = 1f;
    bool interactable;
    bool inInteraction;

    [TextArea(5, 15)]
    public string[] dialogueText;
    public Sprite dialogueSprite;

    protected virtual void Start()
    {
        interactionText.SetActive(false);
        var col = gameObject.AddComponent<CircleCollider2D>();
        col.radius = interactionRange;
        col.isTrigger = true;
    }

    void Update()
    {
        if (interactable && Input.GetKeyDown(Settings.interactKey))
        {
            if (inInteraction)
            {
                inInteraction = false;
                interactionText.SetActive(true);
                EndInteraction();
            }
            else
            {
                inInteraction = true;
                interactionText.SetActive(false);
                StartInteraction();
            }
        }
    }

    protected abstract void StartInteraction();

    protected abstract void EndInteraction();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (PlayerManager.localPlayer.gameObject == collision.gameObject)
        {
            interactable = true;
            interactionText.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (PlayerManager.localPlayer.gameObject == collision.gameObject)
        {
            interactable = false;
            interactionText.SetActive(false);

            if (inInteraction)
            {
                inInteraction = false;
                EndInteraction();
            }
        }
    }
}
