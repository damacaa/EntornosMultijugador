using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MyNetworkManager : NetworkManager
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
    }

    #region client
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        Debug.Log("Jugador Desconectado al servidor");
        SceneManager.LoadSceneAsync(0);
    }

    #endregion client

    #region server
    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
        Debug.Log("Jugador Conectado y creado servidor");
    }



    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);
 
    }

    /*public override void OnStartHost()
    {
        base.OnStartHost();
        _uiManager.ActivateHostOptions();
    }*/
    #endregion server


}
