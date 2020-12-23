using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Inventory;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI Instance;

    public Text text;
    public GameObject inventoryPanel;

    public Text staminaText;
    public Text strengthText;
    public Text intelectText;
    public Text agilityText;
    public Text armorText;
    public Text classText;
    public Text coinsText;

    public Text ping;

    public ItemDetailsUI itemDetails;

    public GameObject escapeMenu;

    bool inventoryOpen = false;
    bool escapeMenuOpen = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        text.text = PhotonNetwork.NickName;
        classText.text = CharacterSelect.selectedClass.ToString();

        if (Settings.displayPing)
        {
            if (ping != null)
                InvokeRepeating("DisplayPing", 1f, 1f);
        }

        if (PlayerManager.localPlayer != null)
            foreach (var attribute in PlayerManager.localPlayer.attributes)
            {
                AttribureModified(attribute);
            }

        StartCoroutine(WaitForPlayerSpawn());

        inventoryPanel.SetActive(inventoryOpen);
        escapeMenu.SetActive(escapeMenuOpen);
    }

    IEnumerator WaitForPlayerSpawn()
    {
        yield return new WaitUntil(() => PlayerManager.localPlayer != null);
        PlayerManager.localPlayer.wallet.OnValueChanged += UpdateMoneyDisplay;

        UpdateMoneyDisplay(PlayerManager.localPlayer.wallet.Coins);
        
    }

    private void OnDestroy()
    {
        if (PlayerManager.localPlayer != null)
            PlayerManager.localPlayer.wallet.OnValueChanged -= UpdateMoneyDisplay;
    }

    private void Update()
    {
        if (Input.GetKeyDown(Settings.inventoryKey))
        {
            inventoryOpen = !inventoryOpen;
            inventoryPanel.SetActive(inventoryOpen);

            if (!inventoryOpen)
                itemDetails.gameObject.SetActive(false);
        }
        if (Input.GetKeyDown(Settings.escapeKey))
        {
            if (inventoryOpen)
            {
                inventoryOpen = false;
                inventoryPanel.SetActive(inventoryOpen);
                itemDetails.gameObject.SetActive(false);
            }
            else 
            {
                escapeMenuOpen = !escapeMenuOpen;
                escapeMenu.SetActive(escapeMenuOpen);
            }
        }
    }

    public void OpenInventory()
    {
        inventoryOpen = true;
        inventoryPanel.SetActive(inventoryOpen);
    }

    private void DisplayPing()
    {
        ping.text = PhotonNetwork.GetPing() + " ms";
    }

    public void AttribureModified(Attribute attribute)
    {
        switch (attribute.type)
        {
            case Attributes.AttackSpeed:
                agilityText.text = string.Concat("Attack Speed: ", attribute.value.ModifiedValue.ToString());
                break;
            case Attributes.MagicDamage:
                intelectText.text = string.Concat("Magic Damage: ", attribute.value.ModifiedValue.ToString());
                break;
            case Attributes.Health:
                staminaText.text = string.Concat("Health Points: ", attribute.value.ModifiedValue.ToString());
                break;
            case Attributes.PhysicalDamage:
                strengthText.text = string.Concat("Physical Damage: ", attribute.value.ModifiedValue.ToString());
                break;
            case Attributes.Armor:
                armorText.text = string.Concat("Armor: ", attribute.value.ModifiedValue.ToString());
                break;
            default:
                break;
        }
    }

    void UpdateMoneyDisplay(int value)
    {
        coinsText.text = value.ToString();
    }
}