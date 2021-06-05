﻿using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MyNetworkManager : NetworkRoomManager
{

    [SerializeField] private UIManager _uiManager;
    public new void Start()
    {
        if (!_uiManager) _uiManager = FindObjectOfType<UIManager>();

#if UNITY_SERVER
            if (autoStartServerBuild)
            {
                StartServer();
            }
#endif
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        Debug.Log("OnClientConnect");
    }

    #region client
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        Debug.Log("Jugador Desconectado del servidor");

    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("OnStartClient");
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        Debug.Log("OnClientError");
        _uiManager.ActivateMainMenu();
    }



    #endregion client

    #region server
    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
        Debug.Log("Jugador Conectado y creado servidor");
    }
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);
        Debug.Log("OnServerDisconnect");
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);
        Debug.Log(conn);
    }


    #endregion server

    public override void OnStartHost()
    {
        base.OnStartHost();
        Debug.Log("OnStartHost");
    }

    public override void OnStopHost()
    {
        base.OnStopHost();
        Debug.Log("OnStopHost");
    }

}
