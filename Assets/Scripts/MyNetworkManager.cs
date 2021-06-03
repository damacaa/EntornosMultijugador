using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MyNetworkManager : NetworkManager
{

    public new void Start()
    {

       
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
        Debug.Log("Jugador Conectado al servidor");
    }

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
        //base.OnServerAddPlayer(conn);
        GameObject player;
        player = Instantiate(spawnPrefabs[1], Vector3.zero, Quaternion.identity) as GameObject;
        NetworkServer.AddPlayerForConnection(conn, player);
    }

    #endregion server


}
