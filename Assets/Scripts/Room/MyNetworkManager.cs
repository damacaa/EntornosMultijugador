using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// Network Manager qeu hereda de NetworkRoomManager, lo que proporciona una base sobre la que hemos organizado las salas.
/// </summary>
public class MyNetworkManager : NetworkRoomManager
{
    ///referencia al HUD del Menu
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

    #region client
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        Debug.Log("OnClientConnect");
    }


    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        Debug.Log("Jugador Desconectado del servidor");

    }

    public override void OnRoomClientDisconnect(NetworkConnection conn)
    {
        base.OnRoomClientDisconnect(conn);
        Debug.Log("Jugador Desconectado dela sala");
    }

    #endregion

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

    #region host

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
