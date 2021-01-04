using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(EnemyMovement))]
public class EnemyAttack : MonoBehaviourPun
{
    public int damage;
    public float range;
    public float time;
    public bool ranged;
    public GameObject projectilePrefab;
    public LayerMask projectileLayer;
    
    HealthPoints target;
    Animator animator;
    int attackTrigger;
    int angleFloat;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        attackTrigger = Animator.StringToHash("attack");
        angleFloat = Animator.StringToHash("angle");
    }

    EnemyMovement enemyMovement;

    private void Start()
    {
        if (!photonView.IsMine) return;
        StartCoroutine(Attack());
        enemyMovement = GetComponent<EnemyMovement>();
        enemyMovement.OnDestinationChange += ChangeTarget;
    }

    public bool canAttack = true;

    IEnumerator Attack()
    {
        while (true)
        {
            yield return new WaitUntil(() => canAttack);
            yield return new WaitUntil(() => target != null);
            yield return new WaitUntil(() => Vector2.Distance(target.transform.position, transform.position) < range);
            
            animator.SetTrigger(attackTrigger);

            Vector2 direction = target.transform.position - transform.position;
            
            float angle = Vector2.SignedAngle(Vector2.up, direction);
            if (angle < -157.5f) angle *= -1;
            animator.SetFloat(angleFloat, angle);
            photonView.RPC("SetAttackAnimatorTrigger", RpcTarget.Others, angle);

            if (!ranged)
                target.Damage(damage);
            else
            {
                direction.Normalize();
                photonView.RPC("SpawnProjectile", RpcTarget.Others, direction, damage);
                SpawnProjectile(direction, damage);
            }
            yield return new WaitForSeconds(time);
        }
    }

    public void ChangeTarget(Transform destination)
    {
        var hp = destination.GetComponent<HealthPoints>();
        if (hp != null)
         target = hp;
    }

    const float projectileSpawnDistance = 0.8f;

    [PunRPC]
    private void SpawnProjectile(Vector2 direction, int damage)
    {
        var obj = Instantiate(projectilePrefab, transform.position + (Vector3)direction * projectileSpawnDistance, Quaternion.identity);
        obj.transform.up = direction;
        var projectile = obj.GetComponent<Projectile>();
        projectile.damage = damage;
        projectile.layerMask = projectileLayer;
    }


    [PunRPC]
    protected void SetAttackAnimatorTrigger(float angle, PhotonMessageInfo info)
    {
        // the photonView.RPC() call is the same as without the info parameter.
        // the info.Sender is the player who called the RPC.
        animator.SetTrigger(attackTrigger);
        animator.SetFloat(angleFloat, angle);
    }
}
