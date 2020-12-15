using JetBrains.Annotations;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.AI;

public class AbilityEffector : MonoBehaviour
{
    [HideInInspector]
    public Ability ability;

    int damage;

    public virtual void StartSkill(int damageMod)
    {
        if (ability.baseDamage != 0)
            damage = ability.baseDamage + damageMod;

        Invoke("DealDamage", ability.startDelay);

        if (ability.repeatable)
            InvokeRepeating("DealDamage", ability.startDelay + ability.tickEvery, ability.tickEvery);

        Destroy(gameObject, ability.duration);
    }

    void DealDamage()
    {
        Collider2D[] collider2Ds = Physics2D.OverlapCircleAll(transform.position, ability.radius);
        int targetCount = Mathf.Min(ability.maxTargets, collider2Ds.Length);
        for (int i = 0; i < targetCount; i++)
        {
            Collider2D collider = collider2Ds[i];
            if (ability.friendly && collider.CompareTag("Player"))
            {
                Debug.Log("spell targeted player " + collider.name);

                var hp = collider.GetComponent<HealthPoints>();
                hp.Damage(-damage);
                Debug.Log("healed " + damage);

                var target = collider.GetComponent<StatusEffects>();
                if (target != null)
                {
                    Debug.Log("buffing " + collider.name);

                    foreach (var effect in ability.effects)
                    {
                        var tempEfect = new Effect(effect);
                        tempEfect.Apply(target);
                    }
                }
            }
            else if (!ability.friendly && !collider.CompareTag("Player"))
            {
                var hp = collider.GetComponent<HealthPoints>();
                if (hp != null)
                    hp.Damage(damage, PlayerManager.localPlayer.gameObject.transform);

                var target = collider.GetComponent<StatusEffects>();
                if (target != null)
                {
                    foreach (var effect in ability.effects)
                    {
                        var tempEfect = new Effect(effect);
                        tempEfect.Apply(target);
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (ability.channeled)
        {
            Ability.abilityInUse = false;
        }
    }
}
