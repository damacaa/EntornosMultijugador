using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MyNetworkManager : NetworkManager
{
    [SerializeField] private Transform[] StartingPositions;
    [SerializeField] private Color[] coloresCoches;

    public new void Start()
    {


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

        //Gets the color selected un initialHUD represented as a number and according to it
        //Changes the player Color
        SetupPlayer player = conn.identity.gameObject.GetComponent<SetupPlayer>();


    }

    #endregion server


}
