 using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Inventory;
using System.IO;

namespace Launcher
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        public static GameManager Instance;
        public GameObject dropPrefab;
        [Space]
        [Tooltip("The prefab to use for representing the player")]
        public string playerPrefab;
        public PlayerClassObject WarriorObject;
        public PlayerClassObject HunterObject;
        public PlayerClassObject WizardObject;
        public PlayerClassObject AlchemystObject;
        public PlayerClassObject PriestObject;

        public InventoryObject[] inventories;

        public Vector3 spawnPosition = new Vector3(-5.5f, -2.5f, 0f);

        public bool saveInvetory = true;

        void Awake()
        {
            //vymazat v konecnem buildu
            if (!PhotonNetwork.IsConnected)
            {
                SceneManager.LoadScene(0);
                return;
            }
            //-------------------------

            Instance = this;
        }

        private void Start()
        {
            if (PlayerManager.localPlayer == null)
            {
                LoadInventories();

                Debug.LogFormat("Instantiating Local Player from scene {0}", SceneManagerHelper.ActiveSceneName);
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                object[] data = new object[1];
                data[0] = CharacterSelect.selectedClass;
                PhotonNetwork.Instantiate(playerPrefab, spawnPosition, Quaternion.identity,0 ,data);
            }
            else
            {
                //revive dead player
                PlayerManager.localPlayer.HealthPoints.ReviveRpc();
                PlayerManager.localPlayer.transform.position = spawnPosition;
                Debug.LogFormat("Carried player object over to scene {0}", SceneManagerHelper.ActiveSceneName);
            }
        }

        #region Photon Callbacks

        public override void OnPlayerEnteredRoom(Player other)
        {
            Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting
        }

        public override void OnPlayerLeftRoom(Player other)
        {
            Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects
        }

        public override void OnLeftRoom()
        {
            foreach (var inventory in inventories)
            {
                inventory.Clear();
            }
            Destroy(PlayerManager.localPlayer.gameObject);
            Debug.Log("eft room");
            PhotonNetwork.Disconnect();
            PhotonNetwork.LoadLevel(0);
        }

        #endregion


        #region Public Methods

        public void SpawnDrop(int itemID, Vector2 position, Item item = null)
        {
            var itemObject = inventories[0].database.ItemObjects[itemID];
            var obj = Instantiate(dropPrefab, position, Quaternion.identity);
            var drop = obj.GetComponent<DroppedItem>();
            drop.Item = itemObject;
            drop.generatedItem = item;
        }

        public void SaveInventories()
        {
            string saveName = CharacterSelect.selectedClass.ToString();

            for (int i = 0; i < inventories.Length; i++)
            {
                inventories[i].savePath = string.Concat("/", saveName, i, ".save");
                inventories[i].Save();
            }

            PlayerManager.localPlayer.wallet.Save();
        }

        public void LoadInventories()
        {
            string saveName = CharacterSelect.selectedClass.ToString();

            for( int i = 0; i < inventories.Length; i++)
            {
                inventories[i].savePath = string.Concat("/", saveName, i,".save");
                inventories[i].Load();
            }
        }

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

        #endregion

        private void OnDestroy()
        {
            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.CurrentRoom.IsOpen = true;
        }
        
        private void OnApplicationQuit()
        {
            if (saveInvetory)
                SaveInventories();

            foreach (var item in inventories)
            {
                item.Clear();
            }
        }
    }
}