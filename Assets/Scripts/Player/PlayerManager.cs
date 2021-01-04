using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Runtime.ExceptionServices;
using Launcher;
using Inventory;

public class PlayerManager : MonoBehaviourPun
{
    public Wallet Wallet;
    public InventoryObject PlayerInventory;
    public InventoryObject PlayerEquipment;
    public CharacterClass PlayerClass;
    public Attribute[] Attributes;

    private RuntimeAnimatorController animatorController;

    public static PlayerManager LocalPlayer;

    public int CurrentLevel = 0;  // aktualni level hry
    [HideInInspector]
    public HealthPoints HealthPoints;

    private Animator animator;

    public RuntimeAnimatorController TempAnimator; //pouziva se k vynuceni aktualizace animatoru, prohozonim s docasnym a zase zpet

    private void Awake()
    {
        if (photonView.IsMine)
        {
            LocalPlayer = this;
        }
        // tento objekt bude prenesen mezi scenami
        DontDestroyOnLoad(this.gameObject);

        HealthPoints = GetComponent<HealthPoints>();
        HealthPoints.OnDeath += CheckForGameOver;

        animator = GetComponent<Animator>();

        Wallet = new Wallet();

        if (photonView.IsMine)
        {
            InitAtributes();
        }
        SetPlayerClassAndVisual();

        //set name ui
        var hpBar = GetComponentInChildren<FloatingHPBarUI>();
        hpBar.chracterName = photonView.Owner.NickName;
    }

    private void Start()
    {
        animator.runtimeAnimatorController = TempAnimator;
        gameObject.SetActive(false);
        Invoke("SetAnimator", 0.2f);
    }

    private void SetAnimator()
    {
        animator.runtimeAnimatorController = animatorController;
        gameObject.SetActive(true);
    }

    private void InitAtributes()
    {
        for (int i = 0; i < Attributes.Length; i++)
        {
            Attributes[i].SetParent(this);
        }
        for (int i = 0; i < PlayerEquipment.GetSlots.Length; i++)
        {
            PlayerEquipment.GetSlots[i].OnBeforeUpdate += OnBeforeSlotUpdate;
            PlayerEquipment.GetSlots[i].OnAfterUpdate += OnAfterSlotUpdate;
        }
        PlayerEquipment.GetSlots[0].OnBeforeUpdate += SetWeapon;
        PlayerEquipment.GetSlots[0].OnAfterUpdate += SetWeapon;
        PlayerEquipment.GetSlots[0].OnBeforeUpdate += CallSetWeaponRPC;
        PlayerEquipment.GetSlots[0].OnAfterUpdate += CallSetWeaponRPC;
    }

    private void OnDestroy()
    {
        HealthPoints.OnDeath -= CheckForGameOver;

        for (int i = 0; i < PlayerEquipment.GetSlots.Length; i++)
        {
            PlayerEquipment.GetSlots[i].OnBeforeUpdate -= OnBeforeSlotUpdate;
            PlayerEquipment.GetSlots[i].OnAfterUpdate -= OnAfterSlotUpdate;
        }
        PlayerEquipment.GetSlots[0].OnBeforeUpdate -= SetWeapon;
        PlayerEquipment.GetSlots[0].OnAfterUpdate -= SetWeapon;
        PlayerEquipment.GetSlots[0].OnBeforeUpdate -= CallSetWeaponRPC;
        PlayerEquipment.GetSlots[0].OnAfterUpdate -= CallSetWeaponRPC;
    }

    public void SetPlayerClassAndVisual()
    {
        if (photonView.IsMine)
            PlayerClass = CharacterSelect.SelectedClass;
        else
        {
            PlayerClass = (CharacterClass)photonView.InstantiationData[0];
            print(string.Concat("Networked player instatiated with class: ", PlayerClass));
        }

        var classObject = GameManager.Instance.WarriorObject;
        switch (PlayerClass)
        {
            case CharacterClass.Warrior:
                classObject = GameManager.Instance.WarriorObject;
                break;
            case CharacterClass.Hunter:
                classObject = GameManager.Instance.HunterObject;
                break;
            case CharacterClass.Wizard:
                classObject = GameManager.Instance.WizardObject;
                break;
            case CharacterClass.Alchemyst:
                classObject = GameManager.Instance.AlchemystObject;
                break;
            case CharacterClass.Priest:
                classObject = GameManager.Instance.PriestObject;
                break;
        }

        //delete placeholder visual in the prefab
        var placeholder = transform.Find("Sprite");
        Destroy(placeholder.gameObject);


        var sprite = Instantiate(classObject.visualPrefab, transform.position + new Vector3(0f, -0.3f, 0f), Quaternion.identity, transform);
        sprite.name = "Sprite";

        animatorController = classObject.animatorController;

        if (photonView.IsMine)
        {
            PlayerEquipment.Container.Slots[0].SetAllowedItems(classObject.weaponTypes);

            foreach (var baseAttribute in classObject.baseAttributes)
            {
                foreach (var charAttribute in Attributes)
                {
                    if (baseAttribute.type == charAttribute.type)
                    {
                        charAttribute.value.BaseValue = baseAttribute.value.BaseValue;
                        break;
                    }
                }
            }

            //nastaveni equipu
            foreach (var item in PlayerEquipment.GetSlots)
            {
                if (item.ItemObject != null)
                    item.UpdateSlot(item.item, 1);
            }

            if (PlayerEquipment.GetSlots[0].ItemObject == null)
                PlayerEquipment.GetSlots[0].UpdateSlot(new Item(classObject.defaultWeapon), 1);

        }
        var attackObj = GetComponent<PlayerAttack>();
        attackObj.ability1 = classObject.ability1;
        attackObj.ability2 = classObject.ability2;
    }

    public void OnBeforeSlotUpdate(InventorySlot slot)
    {
        if (slot.ItemObject == null)
            return;
        switch (slot.parent.inventory.type)
        {
            case InterfaceType.Inventory:
                break;
            case InterfaceType.Equipment:
                for (int i = 0; i < slot.item.buffs.Length; i++)
                {
                    for (int j = 0; j < Attributes.Length; j++)
                    {
                        if (Attributes[j].type == slot.item.buffs[i].attribute)
                            Attributes[j].value.RemoveModifier(slot.item.buffs[i]);
                    }
                }
                break;
            case InterfaceType.Chest:
                break;
            default:
                break;
        }
    }

    public void OnAfterSlotUpdate(InventorySlot slot)
    {
        if (slot.ItemObject == null)
            return;
        switch (slot.parent.inventory.type)
        {
            case InterfaceType.Inventory:
                break;
            case InterfaceType.Equipment:
                for (int i = 0; i < slot.item.buffs.Length; i++)
                {
                    for (int j = 0; j < Attributes.Length; j++)
                    {
                        if (Attributes[j].type == slot.item.buffs[i].attribute)
                            Attributes[j].value.AddModifier(slot.item.buffs[i]);
                    }
                }
                break;
            case InterfaceType.Chest:
                break;
            default:
                break;
        }
    }

    public bool WeaponEquiped
    {
        get;
        private set;
    }
    public ItemType WeaponType
    {
        get;
        private set;
    }

    void SetWeapon(InventorySlot slot)
    {
        var itemObj = transform.Find("Item");
        var weaponRenderer = itemObj?.GetComponent<SpriteRenderer>();
        if (weaponRenderer == null)
        {
            Debug.LogWarning("Player does not have a weaponRenderer");
            return;
        }

        if (slot.ItemObject == null)
        {
            weaponRenderer.sprite = null;
            WeaponEquiped = false;
        }
        else
        {
            weaponRenderer.sprite = slot.ItemObject.uiDisplay;
            WeaponEquiped = true;
            WeaponType = slot.ItemObject.type;
        }
    }

    [PunRPC]
    void SetWeapon(int id)
    {
        var weaponRenderer = transform.Find("Item").GetComponent<SpriteRenderer>();
        if (weaponRenderer == null)
        {
            Debug.LogWarning("Player does not have a weaponRenderer");
            return;
        }

        if (id <= -1)
        {
            weaponRenderer.sprite = null;
            WeaponEquiped = false;
        }
        else
        {
            var itemObject = PlayerEquipment.database.ItemObjects[id];

            weaponRenderer.sprite = itemObject.uiDisplay;
            WeaponEquiped = true;
            WeaponType = itemObject.type;
        }
    }

    void CallSetWeaponRPC(InventorySlot slot)
    {
        if (!PhotonNetwork.InRoom) return;

        int weaponId = -1;
        if (slot.ItemObject != null)
            weaponId = slot.ItemObject.data.Id;

        photonView.RPC("SetWeapon", RpcTarget.OthersBuffered, weaponId);
    }

    void CheckForGameOver()
    {
        if (!PhotonNetwork.InRoom) return; //pokud jiz nejsi v mistnosti nema smysl to kontrolovat

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player"); //je alespon jeden hrac na zivu?
        foreach (var player in players)
        {
            if (player.activeInHierarchy)
                return; //pokud je nekdo nazivu vrat se
        }

        // --GAME OVER--
        
        GameManager.Instance.LeaveRoom(); // vsichni hraci umreli, opust server
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var drop = collision.GetComponent<DroppedItem>();
        if (drop)
        {
            if (!photonView.IsMine)
                Destroy(collision.gameObject);
            else
            {
                if (drop.generatedItem != null)
                {
                    if (PlayerInventory.AddItem(drop.generatedItem, 1))
                        Destroy(collision.gameObject);
                }
                else
                {
                    if (PlayerInventory.AddItem(drop.Item.CreateItem(), 1))
                        Destroy(collision.gameObject);
                }
            }
        }
    }

    public void CallDropRPC(int itemID, Vector2 position, Item item = null)
    {
        photonView.RPC("CreateItemDrop", RpcTarget.Others, itemID, position);
        GameManager.Instance.SpawnDrop(itemID, position, item);
    }

    [PunRPC]
    private void CreateItemDrop(int itemID, Vector2 position)
    {
        GameManager.Instance.SpawnDrop(itemID, position);
    }

    public void AttribureModified(Attribute attribute)
    {
        if (attribute.type == global::Attributes.Health)
        {
            HealthPoints.ChangeMaxHP(attribute.value.ModifiedValue);
        }

        PlayerUI.Instance.AttribureModified(attribute);
    }

    public int GetAttributeValue(Attributes attribute)
    {
        for (int j = 0; j < Attributes.Length; j++)
        {
            if (Attributes[j].type == attribute)
                return Attributes[j].value.ModifiedValue;
        }
        return 0;
    }

    public void AddAtributeModifier(Attributes attribute, IModifier modifier)
    {
        for (int j = 0; j < Attributes.Length; j++)
        {
            if (Attributes[j].type == attribute)
            {
                Attributes[j].value.AddModifier(modifier);
                return;
            }
        }
    }
    public void RemoveAtributeModifier(Attributes attribute, IModifier modifier)
    {
        for (int j = 0; j < Attributes.Length; j++)
        {
            if (Attributes[j].type == attribute)
            {
                Attributes[j].value.RemoveModifier(modifier);
                return;
            }
        }
    }
}