using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class MonsterSpawner : MonoBehaviour
{    
    public LevelDatabase levels; 

    int levelID = 0;

    Transform bottomleftBorder;
    Transform toprightBorder;
    private const string tavernSceneName = "Tavern";

    void Start()
    {
        bottomleftBorder = transform.GetChild(0);
        toprightBorder = transform.GetChild(1);
        
        //spawnuje vlny jen pokud je master
        if (PhotonNetwork.IsMasterClient)
        {
            levelID = PlayerManager.LocalPlayer.CurrentLevel;
            StartCoroutine(SpawnCoroutine());
        }
    }

    IEnumerator SpawnCoroutine()
    {
        yield return new WaitForSeconds(Settings.levelStartDelay);

        Level currentLevel = levels.GetLevel(levelID);

        foreach (var wave in currentLevel.waves)
        {
            for (int i = 0; i < wave.mobs.Length; i++)
            {
                for (int k = 0; k < wave.mobs[i].count; k++)
                {
                    PhotonNetwork.Instantiate(wave.mobs[i].mob, GetRandomPosition(), Quaternion.identity);
                }
            }
            yield return new WaitForSeconds(wave.delay);
        }

        yield return new WaitUntil(() => EnemyMovement.enemyCount < 1);
        yield return new WaitForSeconds(Settings.levelStartDelay);

        //player finished the level
        PlayerManager.LocalPlayer.CurrentLevel++;

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
        Vector3 pos = bottomleftBorder.position;
        pos.x = UnityEngine.Random.Range(pos.x, toprightBorder.position.x);
        pos.y = UnityEngine.Random.Range(pos.y, toprightBorder.position.y);
        return pos;
    }
}
