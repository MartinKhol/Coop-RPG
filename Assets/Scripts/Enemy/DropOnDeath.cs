using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inventory;
using Photon.Pun;
using Launcher;

[System.Serializable]
public class Drop
{
    public ItemObject item;
    [Range(0, 100)]
    public float dropChance;
}

public class DropOnDeath : MonoBehaviourPun
{
    public GameObject dropPrefab;

    public Drop[] dropTable;

    public int minMoney = 0;
    public int maxMoney = 5;

    private void Start()
    {
        var hp = GetComponent<HealthPoints>();

        //if (PhotonNetwork.IsMasterClient)
           hp.OnDeath += OnDeath;
    }

    private void OnDeath()
    {
        GetComponent<HealthPoints>().OnDeath -= OnDeath;

        foreach (var item in dropTable)
        {
            int rndVal = UnityEngine.Random.Range(1, 100);
            if (item.dropChance >= rndVal)
            {
                PlayerManager.LocalPlayer.CallDropRPC(item.item.data.Id, transform.position);
                /*
                var obj = Instantiate(dropPrefab, transform.position, Quaternion.identity);
                var droppedItem = obj.GetComponent<DroppedItem>();
                droppedItem.Item = item.item;*/
            }
        }

        var moneyDrop = UnityEngine.Random.Range(minMoney, maxMoney +1);
        photonView.RPC("DropCoins", RpcTarget.All, moneyDrop);

    }

    [PunRPC]
    private void DropCoins(int amount)
    {
        PlayerManager.LocalPlayer.Wallet.AddCoins(amount);
        print(string.Concat(name, " dropped ", amount, " coins"));
    }
}
