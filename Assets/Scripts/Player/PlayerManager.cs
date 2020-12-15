using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Runtime.ExceptionServices;
using Launcher;
using Inventory;

public class PlayerManager : MonoBehaviourPun
{
    public Wallet wallet;
    public InventoryObject inventory;
    public InventoryObject equipment;
    public CharacterClass playerClass;
    public Attribute[] attributes;
    private RuntimeAnimatorController animatorController;

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static PlayerManager localPlayer;
    [HideInInspector]
    public HealthPoints HealthPoints;

    private Animator animator;
    public RuntimeAnimatorController tempAnimator;

    private void Awake()
    {
        if (photonView.IsMine)
        {
            localPlayer = this;
        }
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(this.gameObject);

        HealthPoints = GetComponent<HealthPoints>();
        HealthPoints.OnDeath += CheckForGameOver;

        animator = GetComponent<Animator>();

        wallet = new Wallet();

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
        animator.runtimeAnimatorController = tempAnimator;
        Invoke("StartAnimator", 0.1f);
    }

    private void StartAnimator()
    {
        animator.enabled = true;
        animator.runtimeAnimatorController = animatorController;
    }

    private void InitAtributes()
    {
        for (int i = 0; i < attributes.Length; i++)
        {
            attributes[i].SetParent(this);
        }
        for (int i = 0; i < equipment.GetSlots.Length; i++)
        {
            equipment.GetSlots[i].OnBeforeUpdate += OnBeforeSlotUpdate;
            equipment.GetSlots[i].OnAfterUpdate += OnAfterSlotUpdate;
        }
        equipment.GetSlots[0].OnBeforeUpdate += SetWeapon;
        equipment.GetSlots[0].OnAfterUpdate += SetWeapon;
        equipment.GetSlots[0].OnBeforeUpdate += CallSetWeaponRPC;
        equipment.GetSlots[0].OnAfterUpdate += CallSetWeaponRPC;
    }

    private void OnDestroy()
    {
        HealthPoints.OnDeath -= CheckForGameOver;

        for (int i = 0; i < equipment.GetSlots.Length; i++)
        {
            equipment.GetSlots[i].OnBeforeUpdate -= OnBeforeSlotUpdate;
            equipment.GetSlots[i].OnAfterUpdate -= OnAfterSlotUpdate;
        }
        equipment.GetSlots[0].OnBeforeUpdate -= SetWeapon;
        equipment.GetSlots[0].OnAfterUpdate -= SetWeapon;
        equipment.GetSlots[0].OnBeforeUpdate -= CallSetWeaponRPC;
        equipment.GetSlots[0].OnAfterUpdate -= CallSetWeaponRPC;
    }

    public void SetPlayerClassAndVisual()
    {
        if (photonView.IsMine)
            playerClass = CharacterSelect.selectedClass;
        else
        {
            playerClass = (CharacterClass)photonView.InstantiationData[0];
            print(string.Concat("Networked player instatiated with class: ", playerClass));
        }

        var classObject = GameManager.Instance.WarriorObject;
        switch (playerClass)
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
        var sprite = Instantiate(classObject.visualPrefab, transform.position + new Vector3(0f, -0.3f, 0f), Quaternion.identity, transform);
        sprite.name = "Sprite";

        animatorController = classObject.animatorController;

        if (photonView.IsMine)
        {
            equipment.Container.Slots[0].SetAllowedItems(classObject.weaponTypes);

            foreach (var baseAttribute in classObject.baseAttributes)
            {
                foreach (var charAttribute in attributes)
                {
                    if (baseAttribute.type == charAttribute.type)
                    {
                        charAttribute.value.BaseValue = baseAttribute.value.BaseValue;
                        break;
                    }
                }
            }

            //nastaveni equipu
            foreach (var item in equipment.GetSlots)
            {
                if (item.ItemObject != null)
                    item.UpdateSlot(item.item, 1);
            }

            if (equipment.GetSlots[0].ItemObject == null)
                equipment.GetSlots[0].UpdateSlot(new Item(classObject.defaultWeapon), 1);

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
           //     print(string.Concat("Removed ", slot.ItemObject, " on", slot.parent.inventory.type, ", Allowed Items: ", string.Join(", ", slot.AllowedItems)));
                for (int i = 0; i < slot.item.buffs.Length; i++)
                {
                    for (int j = 0; j < attributes.Length; j++)
                    {
                        if (attributes[j].type == slot.item.buffs[i].attribute)
                            attributes[j].value.RemoveModifier(slot.item.buffs[i]);
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
           //     print(string.Concat("Placed ", slot.ItemObject, " on", slot.parent.inventory.type, ", Allowed Items: ", string.Join(", ", slot.AllowedItems)));

                for (int i = 0; i < slot.item.buffs.Length; i++)
                {
                    for (int j = 0; j < attributes.Length; j++)
                    {
                        if (attributes[j].type == slot.item.buffs[i].attribute)
                            attributes[j].value.AddModifier(slot.item.buffs[i]);
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
            var itemObject = equipment.database.ItemObjects[id];

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
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            if (player.activeInHierarchy)
                return;
        }
        Debug.Log("Game Over, everyone is dead.");
        GameManager.Instance.LeaveRoom();
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
                    if (inventory.AddItem(drop.generatedItem, 1))
                        Destroy(collision.gameObject);
                }
                else
                {
                    if (inventory.AddItem(drop.Item.CreateItem(), 1))
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
     //   Debug.Log(string.Concat(attribute.type, " was updated! Value is now ", attribute.value.ModifiedValue));

        if (attribute.type == Attributes.Health)
        {
            HealthPoints.ChangeMaxHP(attribute.value.ModifiedValue);
        }

        PlayerUI.Instance.AttribureModified(attribute);
    }

    public int GetAttributeValue(Attributes attribute)
    {
        for (int j = 0; j < attributes.Length; j++)
        {
            if (attributes[j].type == attribute)
                return attributes[j].value.ModifiedValue;
        }
        return 0;
    }

    public void AddAtributeModifier(Attributes attribute, IModifier modifier)
    {
        for (int j = 0; j < attributes.Length; j++)
        {
            if (attributes[j].type == attribute)
            {
                attributes[j].value.AddModifier(modifier);
                return;
            }
        }
    }
    public void RemoveAtributeModifier(Attributes attribute, IModifier modifier)
    {
        for (int j = 0; j < attributes.Length; j++)
        {
            if (attributes[j].type == attribute)
            {
                attributes[j].value.RemoveModifier(modifier);
                return;
            }
        }
    }
}