using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileAbilityEffector : AbilityEffector
{
    public Projectile[] projectiles;

    public override void StartSkill(int damage)
    {
        foreach (var projectile in projectiles)
        {
            projectile.damage = damage;
        }
    }
}
