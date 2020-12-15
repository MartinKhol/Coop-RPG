using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;


public class HealthPoints : MonoBehaviourPun
{
    public event Action OnDeath = delegate { };
    public event Action<int> OnHPChange = delegate { }; //posila pocet zivotu
    public event Action<Transform, int> OnThreat = delegate { };

    public bool disableOnDeath = true;

    [SerializeField]
    private int healthPoints = 10;

    public int maxHP { get; private set; }

    public int HP
    {
        get => healthPoints;
        private set
        {
            int previousVal = healthPoints;

            healthPoints = value;

            if (previousVal != healthPoints) //took damage
            {
                OnHPChange(healthPoints);
            }

            if (healthPoints < 1)
            {
                healthPoints = 0;
                Debug.Log(name + " is dead");
                OnDeath();
                if (disableOnDeath)
                {
                    gameObject.SetActive(false);
                }
            }

            if (healthPoints > maxHP)
                healthPoints = maxHP;

            if (previousVal > healthPoints)
            {
                DamageAnimation();
            }

            Debug.Log(string.Concat(name, " health points at: ", healthPoints));

        }
    }

    Animator animator;
    int damageTrig;
    PlayerManager playerManager;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        damageTrig = Animator.StringToHash("damaged");

        maxHP = HP;

        playerManager = GetComponent<PlayerManager>();
    }

    private void DamageAnimation()
    {
        animator.SetTrigger(damageTrig);
    }

    public void Damage(int amount, Transform playerID = null, int additionalThreat = 0)
    {
        if (playerManager != null && amount > 0)
        {
            var armorVal = playerManager.GetAttributeValue(Attributes.Armor);
            amount = Mathf.Max(amount - armorVal, 1);

        }

        HP -= amount;

        photonView.RPC("ChangeHealth", RpcTarget.Others, HP);
    }

    public void ChangeMaxHP(int newValue)
    {
        photonView.RPC("ChangeMaxHPRPC", RpcTarget.All, newValue);
    }
    
    [PunRPC]
    public void ChangeMaxHPRPC(int newValue)
    {
        var change = newValue - maxHP;
        maxHP = newValue;
        if (change > 0) HP += change;
        else
        if (HP > maxHP) HP = maxHP;

        Debug.Log(name + " maxHp set to " + maxHP);
    }

    [PunRPC]
    void ChangeHealth(int newValue, PhotonMessageInfo info)
    {
        int previousVal = healthPoints;

        healthPoints = newValue;

        if (previousVal != healthPoints) //took damage
        {
            OnHPChange(healthPoints);
        }

        if (healthPoints < 1)
        {
            healthPoints = 0;
            Debug.Log(name + " is dead");
            OnDeath();
            if (disableOnDeath)
            {
                gameObject.SetActive(false);
            }
        }
        else if (healthPoints > maxHP)
            healthPoints = maxHP;

        if (previousVal > healthPoints)
        {
            DamageAnimation();
        }
    }

    public void ReviveRpc()
    {
        photonView.RPC("Revive", RpcTarget.All);
    }

    [PunRPC]
    void Revive()
    {
        Debug.Log(string.Concat("Revived ", name));
        gameObject.SetActive(true);
        healthPoints = maxHP;
        OnHPChange(healthPoints);
    }
}