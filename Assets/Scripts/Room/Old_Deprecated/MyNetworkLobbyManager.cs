using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Network Manager qeu hereda de NetworkRoomManager, lo que proporciona una base sobre la que hemos organizado las salas.
/// </summary>
public class MyNetworkLobbyManager : NetworkRoomManager
{

    ///referencia al HUD del Menu
    [SerializeField] private LobbyUIManager _uiManager;

    bool showStartButton;
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


    public override void OnRoomStopClient()
    {
        base.OnRoomStopClient();
    }

    public override void OnRoomStopServer()
    {
        base.OnRoomStopServer();
    }



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

