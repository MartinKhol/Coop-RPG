using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;

public class EnemyMovement : MonoBehaviour
{
    public event Action<Transform> OnDestinationChange = delegate { };

    Animator animator;
    public Transform Target 
    { 
        private set;
        get; 
    }
    IAstarAI ai;
    
    public static int enemyCount = 0;

    void OnEnable()
    {
        ai = GetComponent<IAstarAI>();

        // Update the destination right before searching for a path as well.
        if (ai != null) ai.onSearchPath += UpdateDestination;
        enemyCount++;
    }

    void OnDisable()
    {
        if (ai != null) ai.onSearchPath -= UpdateDestination;
        enemyCount--;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("walk", true);

        UpdateDestination();
    }

    Vector3 scale = Vector3.one;

    void UpdateDestination()
    {
        if (Target != null && ai != null) ai.destination = Target.position;
        else return;
        //face target
        scale.x = Target.position.x < transform.position.x ? -1 : 1;
        transform.localScale = scale;
    }

    IEnumerator WaitTillReachesTarget()
    {
        yield return new WaitUntil(() => ai.reachedEndOfPath);
        animator.SetBool("walk", false);
    }

    public void ChangeDestination(Transform destination)
    {
        if (destination == null)
        {
            Debug.LogWarning("neexistuje destination");
        }

        OnDestinationChange(destination);
        Target = destination;
        ai.destination = Target.position;

        if (ai.reachedEndOfPath)
        {
            animator.SetBool("walk", true);
            ai.destination = Target.position;
            ai.onSearchPath += UpdateDestination;
            StartCoroutine(WaitTillReachesTarget());
        }
    }
}