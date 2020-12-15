using System.Collections;
using UnityEngine;

public enum EffectsType
{
    Slow,
    Stun,
    PhysicalAttack,
    MagicalAttack,
}

[System.Serializable]
public class Effect
{
    public EffectsType type;
    public float duration;
    public float value;

    public Effect(Effect effect)
    {
        type = effect.type;
        duration = effect.duration;
        value = effect.value;
    }
    public void Apply(StatusEffects target)
    {
        switch (type)
        {
            case EffectsType.Slow:
                target.Slow(value);
                break;
            case EffectsType.Stun:
              //  Debug.Log("stunning");
                target.Stun();
                break;
            case EffectsType.PhysicalAttack:
                target.ChangePhysicalDamage(value, duration);
                break;
            case EffectsType.MagicalAttack:
                target.ChangeMagDmg(value, duration);
                break;
            default:
                break;
        }

        if (target.isActiveAndEnabled)
            target.StartCoroutine(Unapply(target));
    }

    IEnumerator Unapply(StatusEffects target)
    {
        yield return new WaitForSeconds(duration);
        switch (type)
        {
            case EffectsType.Slow:
                target.ResetSpeed();
                break;
            case EffectsType.Stun:
                Debug.Log("reseting stun");
                target.Unstun();
                break;
            case EffectsType.PhysicalAttack:
                target.ResetPhysDmg();
                break;
            case EffectsType.MagicalAttack:
                target.ResetMagDmg();
                break;
            default:
                break;
        }
    }
}
