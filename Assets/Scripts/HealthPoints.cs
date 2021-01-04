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

    [SerializeField]
    private bool disableOnDeath = true;

    [SerializeField]
    private int healthPoints = 10;

    public int maxHP { get; private set; }

    public int HP
    {
        get => healthPoints;
      /*  private set
        {
            int previousVal = healthPoints;

            //nastaveni nove hodnoty zivotu
            healthPoints = value;

            //doslo ke zmene hodnoty
            if (previousVal != healthPoints) 
            {
                OnHPChange(healthPoints);
            }

            //zivoty klesly pod 1
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

            //zivoty presahly povolene maximum
            if (healthPoints > maxHP)
                healthPoints = maxHP;

            //zivoty klesly
            if (previousVal > healthPoints)
            {
                DamageAnimation();
            }

        }*/
    }

    public bool IsDead { get { return healthPoints < 1; } }

    Animator animator; //odkaz na animator postavy pro prehrani animace pri poklesu zivotu
    readonly int damageTrig = Animator.StringToHash("damaged"); //hash parametru animatoru
    PlayerManager playerManager; // odkaz na hrace pro pocitani s armorem

   
    private void Awake()  //inicializace
    {
        animator = GetComponent<Animator>();
        playerManager = GetComponent<PlayerManager>();
        maxHP = HP;
    }

    private void DamageAnimation()
    {
        if (animator.runtimeAnimatorController != null)
            animator.SetTrigger(damageTrig);
    }

    public void Damage(int amount, Transform attackingPlayer = null, int additionalThreat = 0)
    {
        if (!PhotonNetwork.IsMasterClient) // veskere vypocty zivotu probihaji pouze na master clientu aby se predeslo chybam
        {            
            return;
        }

        //odecti hodnotu armoru hrace
        if (playerManager != null && amount > 0)
        {
            var armorVal = playerManager.GetAttributeValue(Attributes.Armor);
            amount = Mathf.Max(amount - armorVal, 1);
        }

        //HP -= amount;

        photonView.RPC("ChangeHealth", RpcTarget.All, HP - amount);

        // threat change
        if (attackingPlayer != null)
            OnThreat(attackingPlayer, additionalThreat);
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
        if (change > 0)
            photonView.RPC("ChangeHealth", RpcTarget.All, HP + change);
        else if (HP > maxHP) 
            photonView.RPC("ChangeHealth", RpcTarget.All, maxHP);
    }

    [PunRPC]
    void ChangeHealth(int newValue)
    {
        int previousVal = healthPoints;
        
        healthPoints = Mathf.Clamp(newValue, 0, maxHP);

        if (healthPoints < 1) //character is dead
        {
            OnDeath();
            if (disableOnDeath)
                gameObject.SetActive(false);
        }
        
        if (previousVal > healthPoints) //character got damaged - play animation
            DamageAnimation();

        if (previousVal != healthPoints) //send hp change event
            OnHPChange(healthPoints);
    }

    public void ReviveRpc()
    {
        photonView.RPC("Revive", RpcTarget.All);
    }

    [PunRPC]
    void Revive()
    {
        gameObject.SetActive(true);
        healthPoints = maxHP;

        OnHPChange(healthPoints); //updatne healthbar
    }
}