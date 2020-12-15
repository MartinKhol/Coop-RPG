using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Photon.Pun;
using System.Linq;
using JetBrains.Annotations;

[RequireComponent(typeof(EnemyMovement))]
public class EnemyChasePlayer : MonoBehaviourPun
{
    //kontroluj vzdalenost hrace a pripadne ho napadne
    EnemyMovement enemyMovement;
    public float aggroRange = 2f;
    IAstarAI ai;
    public LayerMask playerLayerMask;

    public Dictionary<Transform, int> aggroTable;

    void Start()
    {
        ai = GetComponent<IAstarAI>();
        enemyMovement = GetComponent<EnemyMovement>();

        if (photonView.IsMine)
        {
            StartCoroutine(SelectTarget());
        }
        else
        {
            ai.isStopped = true;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            var hp = GetComponent<HealthPoints>();
            hp.OnThreat += ChangeThreat;

            hp.ChangeMaxHP(Mathf.Max(hp.maxHP, hp.maxHP * PhotonNetwork.CurrentRoom.PlayerCount));
        }
    }

    IEnumerator SelectTarget()
    {
        //wait till local player loads in
        yield return new WaitUntil(() => PlayerManager.localPlayer != null);
        aggroTable = new Dictionary<Transform, int>();


        //while not dead
        while (gameObject.activeInHierarchy)
        {

            Transform target = null;

            //vyber cil podle vzdalenosti
            target = CheckPlayerDistance();
            enemyMovement.ChangeDestination(target);
            // Debug.LogFormat("{0} is chasing {1}", name, target.name);

            yield return new WaitForSeconds(1f);

            // yield return new WaitUntil(() => (target == null || target.gameObject.activeInHierarchy == false));

        }
    }

    void ChangeThreat(Transform gameObject, int amount)
    {
        if (aggroTable.ContainsKey(gameObject))
        {
            aggroTable[gameObject] += amount;
        }
        else
        {
            aggroTable.Add(gameObject, amount);
        }

        //utoc na cil s nejvetsim agro
        var target = enemyMovement.Target;
        Transform max = target;
        foreach (var item in aggroTable)
        {
            int i = item.Value;
            if (i > aggroTable[max])
            {
                max = item.Key;
            }
        }
        if (max != target)
        {
            target = max;
            Debug.LogFormat("{0} is chasing {1}", name, target.name);
            enemyMovement.ChangeDestination(target);
        }
    }

    Transform CheckPlayerDistance()
    {
        Collider2D[] collider2Ds = Physics2D.OverlapCircleAll(transform.position, aggroRange, playerLayerMask);
        if (collider2Ds.Length == 0)
        {
            Debug.Log("all players are dead");

            gameObject.SetActive(false);
            return null;
        }
        Transform target = collider2Ds[0].transform;
        float min = float.NegativeInfinity;
        foreach (var item in collider2Ds)
        {
            var distance = Vector2.Distance(transform.position, item.transform.position);
            if (distance < min)
            {
                min = distance;
                target = item.transform;
            }
        }
        return target;
    }
}
