using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class StatusEffects : MonoBehaviour
{
    float defaultSpeed;
    IAstarAI astarAI;
    PlayerMovement playerMovement;
    PlayerManager playerManager;

    float baseSpeed;

    private void Start()
    {
        astarAI = GetComponent<IAstarAI>();
        playerMovement = GetComponent<PlayerMovement>();
        playerManager = GetComponent<PlayerManager>();
        baseSpeed = GetSpeed();
    }

    public void AttackSlow(float time)
    {
        if (slowed) return; //nemuze se spomalit pokud ma na sobe debuff
        SetSpeed(baseSpeed / 3f);
        StartCoroutine(ResetSpeed(time));
    }

    IEnumerator ResetSpeed(float time = 0)
    {
        yield return new WaitForSeconds(time);
        if (!slowed) //nemuze se zrychlit pokud ma na sobe debuff
            SetSpeed(baseSpeed);
    }

    bool slowed = false;
    public void ResetSpeed()
    {
        slowed = false;
        SetSpeed(baseSpeed);
    }

    public bool Slow(float multiplier)
    {
        bool prevSlowed = slowed;
        slowed = true;
        SetSpeed(baseSpeed * multiplier);
        return prevSlowed;
    }

    public void Stun()
    {
        SetSpeed(0);
        var enemyAttack = GetComponent<EnemyAttack>();
        if (enemyAttack != null) enemyAttack.canAttack = false;
    }

    public void Unstun()
    {
        ResetSpeed();
        var enemyAttack = GetComponent<EnemyAttack>();
        if (enemyAttack != null) enemyAttack.canAttack = true;
    }

    BuffModifier physDmgMod = new BuffModifier();

    public void ChangePhysicalDamage(float multiplier, float duration)
    {
        if (playerManager == null) return;

        var playerPhysDmg = playerManager.GetAttributeValue(Attributes.PhysicalDamage);
        physDmgMod.buffMod = (int)(playerPhysDmg * multiplier);
        playerManager.AddAtributeModifier(Attributes.PhysicalDamage, physDmgMod);
    }

    public void ResetPhysDmg()
    {
        playerManager.RemoveAtributeModifier(Attributes.PhysicalDamage, physDmgMod);
    }

    BuffModifier magDmgMod = new BuffModifier();

    public void ChangeMagDmg(float multiplier, float duration)
    {
        if (playerManager == null) return;

        var playerMagDmg = playerManager.GetAttributeValue(Attributes.MagicDamage);
        magDmgMod.buffMod = (int)(playerMagDmg * multiplier);
        playerManager.AddAtributeModifier(Attributes.MagicDamage, magDmgMod);
    }

    public void ResetMagDmg()
    {
        playerManager.RemoveAtributeModifier(Attributes.MagicDamage, magDmgMod);
    }

    float GetSpeed()
    {
        if (astarAI != null)
            return astarAI.maxSpeed;

        if (playerMovement != null)
            return playerMovement.movementSpeed;

        return 0;
    }

    void SetSpeed(float speed)
    {
        if (astarAI != null)
            astarAI.maxSpeed = speed;
        else
        if (playerMovement != null)
            playerMovement.movementSpeed = speed;
    }

}

public class BuffModifier : IModifier
{
    public int buffMod = 1;
    public void AddValue(ref int baseValue)
    {
        baseValue += buffMod;
    }
}