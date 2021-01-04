using Inventory;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Animator))]
public class PlayerAttack : MonoBehaviourPun
{
    public Transform attackPivot;
    public LayerMask layerMask;

    public float radius = 0.4f;
    public float distance = 0.5f;

    public float baseAttackSpeed = 1f;

    private float AttackCooldown
    {
        get
        {
            return ((float)Mathf.Max((100 - playerManager.GetAttributeValue(Attributes.AttackSpeed)), 20) / 100f) * baseAttackSpeed;
        }
    }

    private int PhysicalDamage
    {
        get
        {
            return playerManager.GetAttributeValue(Attributes.PhysicalDamage);
        }
    }

    private int MagicalDamage
    {
        get
        {
            return playerManager.GetAttributeValue(Attributes.MagicDamage);
        }
    }

    public GameObject staffProjectile;
    public GameObject bowProjectile;
    public LineRenderer ankhLineRenderer;
    public float ankhRange = 3f;

    public Ability ability1;
    public Ability ability2;
    public float parryCd = 1f;
    private float lastParry = 0f;
    private PlayerManager playerManager;

    //Animator caches
    protected Animator animator;
    protected int angleFloat;
    protected int attackTrigger;
    protected int parryTrigger;

    ParryModifier parryModifier = new ParryModifier();

    Camera mainCamera;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        angleFloat = Animator.StringToHash("angle");
        attackTrigger = Animator.StringToHash("attack");
        parryTrigger = Animator.StringToHash("parry");
    }

    private void Start()
    {
        playerManager = PlayerManager.LocalPlayer;
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (!Ability.AbilityInUse)
            {
                if (Input.GetKey(Settings.attackKey))
                {
                    if (!EventSystem.current.IsPointerOverGameObject() && MouseData.tempItemBeingDragged == null)
                    {
                        Vector2 direction = mousePos - (Vector2)attackPivot.position;
                        direction.Normalize();
                        Attack(direction);
                    }
                }
                else
                if (Input.GetKey(Settings.ability1Key))
                {
                    Vector2 direction = mousePos - (Vector2)attackPivot.position;
                    direction.Normalize();
                    if (ability1.Use(mousePos, attackPivot, direction))
                    {
                        var position = ability1.onMouse ? mousePos : (Vector2)attackPivot.position;
                        photonView.RPC("SpawnEffect", RpcTarget.Others, 1, position, direction);
                    }
                }
                else
                if (Input.GetKey(Settings.ability2Key))
                {
                    Vector2 direction = mousePos - (Vector2)attackPivot.position;
                    direction.Normalize();
                    if (ability2.Use(mousePos, attackPivot, direction))
                    {
                        var position = ability2.onMouse ? mousePos : (Vector2)attackPivot.position;
                        photonView.RPC("SpawnEffect", RpcTarget.Others, 2, position, direction);
                    }
                }
                else
                if (Input.GetKey(Settings.parryKey) && playerManager.WeaponEquiped)
                {
                    Parry();
                }
            }
            //flip character towards mouse
            Vector3 scale = Vector3.one;
            scale.x = mousePos.x < attackPivot.position.x ? -1 : 1;
            transform.localScale = scale;
        }
    }

    private void Parry()
    {
        if ((Time.time - lastParry) > parryCd)
        {
            lastParry = Time.time;
            animator.SetTrigger(parryTrigger);
            photonView.RPC("ParryAnimation", RpcTarget.Others);

            int armorVal = playerManager.GetAttributeValue(Attributes.Armor);
            if (armorVal == 0)
            {
                parryModifier.parryMod = 1;
            }
            else
            {
                parryModifier.parryMod = (int)(armorVal * 0.5f);
            }
            playerManager.AddAtributeModifier(Attributes.Armor, parryModifier);
        }
    }

    private void EndParry()
    {
        playerManager.RemoveAtributeModifier(Attributes.Armor, parryModifier);
    }

    private void OnDrawGizmos()
    {
        if (photonView.IsMine)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = mousePos - (Vector2)attackPivot.position;
            direction.Normalize();
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(attackPivot.position + (Vector3)direction * distance, radius);
        }
    }

    protected float lastAttack = -1f;

    public virtual void Attack(Vector2 direction)
    {
        if (!playerManager.WeaponEquiped) return;

        if ((Time.time - lastAttack) < AttackCooldown) return;
        lastAttack = Time.time;

        float angle = Vector2.SignedAngle(Vector2.up, direction);
        if (angle < -157.5f) angle *= -1;

        animator.SetFloat(angleFloat, angle);
        animator.SetTrigger(attackTrigger);
        photonView.RPC("SetAttackAnimatorTrigger", RpcTarget.Others, angle);

        switch (playerManager.WeaponType)
        {
            case Inventory.ItemType.Sword:
                photonView.RPC("MeleeAttack", RpcTarget.MasterClient, direction, PhysicalDamage);
               // MeleeAttack(direction);
                break;
            case Inventory.ItemType.Staff:
                SpawnStaffProjectile(direction, MagicalDamage);
                photonView.RPC("SpawnStaffProjectile", RpcTarget.Others, direction, MagicalDamage);
                break;
            case Inventory.ItemType.Bow:
                SpawnBowProjectile(direction, PhysicalDamage);
                photonView.RPC("SpawnBowProjectile", RpcTarget.Others, direction, PhysicalDamage);
                break;
            case Inventory.ItemType.Dagger:
                photonView.RPC("MeleeAttack", RpcTarget.MasterClient, direction, PhysicalDamage);        
                break;
            case Inventory.ItemType.Ankh:
                SpawnAnkhProjectile(direction, MagicalDamage);
                photonView.RPC("SpawnAnkhProjectile", RpcTarget.Others, direction, MagicalDamage);
                break;
            default:
                break;
        }
    }


    #region RPC Calls

    [PunRPC]
    private void MeleeAttack(Vector2 direction, int damage)
    {
        Collider2D[] collider2Ds = Physics2D.OverlapCircleAll(attackPivot.position + (Vector3)direction * (distance + 0.3f), radius, layerMask);

        foreach (var col in collider2Ds)
        {
            HealthPoints healthPoints = col.GetComponent<HealthPoints>();
            if (healthPoints != null)
            {
                healthPoints.Damage(damage, gameObject.transform, 0);
            }
        }
    }

    [PunRPC]
    private void ParryAnimation()
    {
        animator.SetTrigger(parryTrigger);
    }

    const float projectileSpawnDistance = 0.8f;

    [PunRPC]
    private void SpawnStaffProjectile(Vector2 direction, int damage)
    {
        var obj = Instantiate(staffProjectile, attackPivot.position + (Vector3)direction * projectileSpawnDistance, Quaternion.identity);
        obj.transform.up = direction;
        var projectile = obj.GetComponent<Projectile>();
        projectile.damage = damage;
        projectile.SetSourceAndThreat(gameObject, 0);
    }

    [PunRPC]
    private void SpawnBowProjectile(Vector2 direction, int damage)
    {
        var obj = Instantiate(bowProjectile, attackPivot.position + (Vector3)direction * projectileSpawnDistance, Quaternion.identity);
        obj.transform.up = direction;
        var projectile = obj.GetComponent<Projectile>();
        projectile.damage = damage;
        projectile.SetSourceAndThreat(gameObject, 0);
    }

    const float ankhRayOffset = 0.6f;

    [PunRPC]
    private void SpawnAnkhProjectile(Vector2 direction, int damage)
    {
        var rayStart = attackPivot.position + (Vector3)direction * ankhRayOffset;

        ankhLineRenderer.SetPosition(0, rayStart);
        ankhLineRenderer.enabled = true;
        Invoke("DisableAnkhLineRenderer", 0.2f);

        RaycastHit2D hit;
        hit = Physics2D.Raycast(rayStart, direction, ankhRange, layerMask);
        if (hit.collider != null)
        {
          

            if (!hit.collider.CompareTag("Player"))
            {
                var hp = hit.collider.gameObject.GetComponent<HealthPoints>();
                if (hp != null)
                {
                    hp.Damage(damage);
                }
               // ankhLineRenderer.SetPosition(1, hit.point);
            }
            ankhLineRenderer.SetPosition(1, hit.point);
        }
        else 
            ankhLineRenderer.SetPosition(1, rayStart + (Vector3)direction * ankhRange);
    }

    void DisableAnkhLineRenderer()
    {
        ankhLineRenderer.enabled = false;
    }

    [PunRPC]
    protected void SetAttackAnimatorTrigger(float angle)
    {
        animator.SetTrigger(attackTrigger);
        animator.SetFloat(angleFloat, angle);
    }

    [PunRPC]
    public void SpawnEffect(int ability, Vector2 position, Vector2 direction)
    {  
        GameObject prefab = null;
        if (ability == 1)
        {
            prefab = ability1.effectorPrefab;
            if (prefab != null)
            {
                var obj = Instantiate(prefab, position, Quaternion.identity);
                Destroy(obj, ability1.duration);
            }
        }
        else if (ability == 2)
        {
            prefab = ability2.effectorPrefab;
            if (prefab != null)
            {
                var obj = Instantiate(prefab, position, Quaternion.identity);
                Destroy(obj, ability2.duration);
            }
        }
    }
    #endregion
}

public class ParryModifier : IModifier
{
    public int parryMod = 1;
    public void AddValue(ref int baseValue)
    {
        baseValue += parryMod;
    }
}