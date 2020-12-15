using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class MonsterSpawner : MonoBehaviour
{    
    public Wave[] waves; 

    public static int currentWave = 0;

    Transform leftBorder;
    Transform rightBorder;
    private const string tavernSceneName = "Tavern";

    void Start()
    {
        leftBorder = transform.GetChild(0);
        rightBorder = transform.GetChild(1);
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(SpawnCoroutine());
    }

    IEnumerator SpawnCoroutine()
    {
        yield return new WaitForSeconds(Settings.levelStartDelay);

        if (currentWave >= waves.Length) currentWave = 0;

        foreach (var group in waves[currentWave].groups)
        {
            for (int i = 0; i < group.count; i++)
            {
                for (int k = 0; k < group.mob.Length; k++)
                {
                    PhotonNetwork.Instantiate(group.mob[k], GetRandomPosition(), Quaternion.identity);
                }
                yield return new WaitForSeconds(group.delay);
            }

        }

        yield return new WaitUntil(() => EnemyMovement.enemyCount < 1);
        yield return new WaitForSeconds(Settings.levelStartDelay);
        currentWave++;
        ReturnToTavern();
    }

    private void ReturnToTavern()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(tavernSceneName);
        }
    }

    private Vector3 GetRandomPosition()
    {
        Vector3 pos = leftBorder.position;
        pos.x = UnityEngine.Random.Range(pos.x, rightBorder.position.x);
        return pos;
    }
}
