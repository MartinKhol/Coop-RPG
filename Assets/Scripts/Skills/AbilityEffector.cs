using JetBrains.Annotations;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.AI;

public class AbilityEffector : MonoBehaviour
{
    [HideInInspector]
    public Ability Ability;

    private int damage;

    public virtual void StartSkill(int damageMod)
    {
        if (Ability.damageMult != 0)
            damage = Ability.baseDamage + damageMod;

        Invoke("DealDamage", Ability.startDelay);

        if (Ability.repeatable)
            InvokeRepeating("DealDamage", Ability.startDelay + Ability.tickEvery, Ability.tickEvery);

        Destroy(gameObject, Ability.duration);
    }

    void DealDamage()
    {
        Collider2D[] collider2Ds = Physics2D.OverlapCircleAll(transform.position, Ability.radius);
        int targetCount = Mathf.Min(Ability.maxTargets, collider2Ds.Length);
        for (int i = 0; i < targetCount; i++)
        {
            Collider2D collider = collider2Ds[i];
            if (Ability.friendly && collider.CompareTag("Player"))
            {
                var hp = collider.GetComponent<HealthPoints>();
                hp.Damage(-damage);

                var target = collider.GetComponent<StatusEffects>();
                if (target != null)
                {
                    foreach (var effect in Ability.effects)
                    {
                        var tempEfect = new Effect(effect);
                        tempEfect.Apply(target);
                    }
                }
            }
            else if (!Ability.friendly && !collider.CompareTag("Player"))
            {
                var hp = collider.GetComponent<HealthPoints>();
                if (hp != null)
                    hp.Damage(damage, PlayerManager.LocalPlayer.gameObject.transform);

                var target = collider.GetComponent<StatusEffects>();
                if (target != null)
                {
                    foreach (var effect in Ability.effects)
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
        if (Ability.channeled)
        {
            Ability.AbilityInUse = false;
        }
    }
}
