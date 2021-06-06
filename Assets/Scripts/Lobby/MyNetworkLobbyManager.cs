using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MyNetworkLobbyManager : NetworkRoomManager
{

    [SerializeField] private LobbyUIManager _uiManager;
    public override void OnRoomServerConnect(NetworkConnection conn)
    {
        if (!_uiManager) _uiManager = FindObjectOfType<LobbyUIManager>();

#if UNITY_SERVER
            if (autoStartServerBuild)
            {
                StartServer();
            }
#endif
    }

    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnection conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        // PlayerScore playerScore = gamePlayer.GetComponent<PlayerScore>();
        //playerScore.index = roomPlayer.GetComponent<NetworkRoomPlayer>().index;
        Debug.Log(roomPlayer.GetComponent<LobbyPlayer>());
        return true;
    }

    public override void OnRoomServerAddPlayer(NetworkConnection conn)
    {
        base.OnRoomServerAddPlayer(conn);
        string name = _uiManager.GetPlayerName();
        string color = _uiManager.GetCarColor();

        conn.identity.gameObject.GetComponent<LobbyPlayer>()._name = name;
        conn.identity.gameObject.GetComponent<LobbyPlayer>()._carColor = color;
        Debug.Log("COMEDME UN PIE");
    }


    public override void OnRoomStopClient()
    {
        base.OnRoomStopClient();
    }

    public override void OnRoomStopServer()
    {
        base.OnRoomStopServer();
    }

    /*private void Update()
    {
        if (roomSlots.Count > 1)
        {
            foreach (NetworkRoomPlayer roomPlayer in roomSlots)
            {
                if (roomPlayer == null)
                    continue;

                // find the game-player object for this connection, and destroy it
                NetworkIdentity identity = roomPlayer.GetComponent<NetworkIdentity>();

                if (NetworkServer.active)
                {
                    // re-add the room object
                    roomPlayer.GetComponent<NetworkRoomPlayer>().readyToBegin = false;
                    NetworkServer.ReplacePlayerForConnection(identity.connectionToClient, roomPlayer.gameObject);
                }
            }

            allPlayersReady = false;
            base.ServerChangeScene(GameplayScene);
        }
    }*/




    bool showStartButton;

    public override void OnRoomServerPlayersReady()
    {
        // calling the base method calls ServerChangeScene as soon as all players are in Ready state.
#if UNITY_SERVER
            base.OnRoomServerPlayersReady();
#else
        showStartButton = true;
#endif
    }

    public override void OnGUI()
    {
        base.OnGUI();

        if (allPlayersReady && showStartButton && GUI.Button(new Rect(150, 300, 120, 20), "START GAME"))
        {
            // set to false to hide it in the game scene
            showStartButton = false;

            ServerChangeScene(GameplayScene);
        }
    }
}

