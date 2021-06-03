using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MyNetworkManager : NetworkManager
{
    [SerializeField] private Transform [] StartingPositions;
    [SerializeField] private Material[] coloresCoches;

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

        SetupPlayer _playerInfo = conn.identity.GetComponent<SetupPlayer>();
        UIManager hudInicial = _playerInfo.GetUi();

        //get car color
        GameObject car = spawnPrefabs[0];
        var color = hudInicial.GetCarColor();
        if (color == "Verde")
        {
            car = spawnPrefabs[1];
        }
        else if (color == "Amarillo")
        {
            car = spawnPrefabs[2];
        }
        else if (color == "Blanco")
        {
            car = spawnPrefabs[3];
        }
        playerPrefab = car;
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
        base.OnServerAddPlayer(conn);

        //GameObject player;
        //player = Instantiate(spawnPrefabs[1], Vector3.zero, Quaternion.identity) as GameObject;
        //NetworkServer.AddPlayerForConnection(conn, player);


    }

    #endregion server


}
