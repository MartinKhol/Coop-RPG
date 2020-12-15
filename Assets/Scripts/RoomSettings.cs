using Inventory;
using Launcher;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Versioning;
using UnityEngine;

[RequireComponent(typeof(GameManager))]
public class RoomSettings : MonoBehaviour
{
    public bool openRoom;

    private void Awake()
    {
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.CurrentRoom.IsOpen = openRoom;
    }
}
