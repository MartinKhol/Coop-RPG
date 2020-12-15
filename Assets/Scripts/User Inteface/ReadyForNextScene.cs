using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Unity.Collections;
using UnityEngine.UI;
using Photon.Realtime;
using Launcher;

public class ReadyForNextScene : MonoBehaviourPunCallbacks
{
    public KeyCode readinessKey = KeyCode.R;
    public string nextSceneName;
    public Text readinessText;
    private bool myReadiness = false;
    private int totalPlayersReady = 0;

    //server only
    private Dictionary<int, bool> playersReady;

    private void Start()
    {
        UpdateText();
        readinessText.gameObject.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(readinessKey))
        {
            if (myReadiness)
            {
                myReadiness = false;
                PlayerNotReady();
                photonView.RPC("PlayerNotReady", RpcTarget.Others);
            }
            else
            {
                myReadiness = true;
                PlayerReady();
                photonView.RPC("PlayerReady", RpcTarget.Others);
            }
        }
    }

    [PunRPC]
    void PlayerReady()
    {
        totalPlayersReady++;
        UpdateText();
        if (totalPlayersReady >= PhotonNetwork.CurrentRoom.PlayerCount)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("Switching to next scene: {0}", nextSceneName);
                PhotonNetwork.LoadLevel(nextSceneName);
            }
        }
    }

    [PunRPC]
    void PlayerNotReady()
    {
        totalPlayersReady--;
        UpdateText();
    }

    void UpdateText()
    {
      //  if (totalPlayersReady == 0) readinessText.enabled = false;
      //  else readinessText.enabled = true;
        readinessText.text = "[R]eady " + totalPlayersReady + " / " + PhotonNetwork.CurrentRoom.PlayerCount;
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        UpdateText();
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        myReadiness = false;
        totalPlayersReady = 0;
        UpdateText();
    }
}