using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "Ability")]
public class Ability : ScriptableObject
{
    public static bool abilityInUse;
    public GameObject effectorPrefab;
    public Sprite uiDisplay;
    [Space]
    [Tooltip("spawns effector on mouse cursor instead of player")]
    public bool onMouse;
    [Tooltip("block player movement and attack")]
    public bool channeled;
    [Tooltip("affects only friendly units")]
    public bool friendly;
    [Tooltip("effect spawns facing mouse cursor")]
    public bool faceMouse;
    public bool repeatable = true;
    public int maxTargets = 999;
    public Attributes damageStat = Attributes.PhysicalDamage;
    public int baseDamage = 0;
    public float damageMult = 1f;
    public float duration = 10f;
    public float cooldown = 1f;
    public float startDelay = 0f;
    [Tooltip("Only when repeatable")]
    public float tickEvery = 1f;
    public float radius = 2f;
    public Effect[] effects;
    protected float lastAttack = -60f;

    public event Action<float> StartCooldown;

    public bool Use(Vector2 mouseWorldPosition, Transform attackPivot, Vector3 direction)
    {
        if ((Time.time - lastAttack) < cooldown) return false;
        lastAttack = Time.time;

        Debug.Log("using skill" + name);

        Vector3 position;
        if (onMouse)
        {
            position = mouseWorldPosition;
        }
        else
        {
            position = attackPivot.position;
        }

        var obj = Instantiate(effectorPrefab, position, Quaternion.identity);
        if (faceMouse)
        {
            obj.transform.up = direction;
        }
        var effector = obj.GetComponent<AbilityEffector>();
        effector.ability = this;
        effector.StartSkill((int)(PlayerManager.localPlayer.GetAttributeValue(damageStat) * damageMult));
        if (StartCooldown != null)
        {
            StartCooldown.Invoke(cooldown);
        }

        if (channeled)
        {
            abilityInUse = true;
        }

        return true;
    }

    private void OnEnable()
    {
        lastAttack = -180f;
    }
}
