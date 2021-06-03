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

        SetupPlayer player = conn.identity.gameObject.GetComponent<SetupPlayer>();
        int colorIdx = player.GetUi().GetCar();
        if (colorIdx == 0)
        {
            Color color = Color.red;
            player.CmdSetCarColor(color);
        }
        if (colorIdx == 1)
        {
            Color color = Color.green;
            player.CmdSetCarColor(color);
        }
        if (colorIdx == 2)
        {
            Color color = Color.yellow;
            player.CmdSetCarColor(color);

        }
        if (colorIdx == 3)
        {
            Color color = Color.white;
            player.CmdSetCarColor(color);
        }
        //Debug.Log(player.GetCarColor());
        //GameObject player;
        //player = Instantiate(spawnPrefabs[1], Vector3.zero, Quaternion.identity) as GameObject;
        //NetworkServer.AddPlayerForConnection(conn, player);


    }

    #endregion server


}
