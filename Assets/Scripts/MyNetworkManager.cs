using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class MyNetworkManager : NetworkManager
{
    #region client
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        Debug.Log("Jugador Conectado al servidor");
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

        //Set name choosen as the players name
        SetupPlayer player = conn.identity.GetComponent<SetupPlayer>();
        player.CmdChangeName();
        Debug.Log(player.GetName()); //coge el nombre del espacio creado en el HUD inicial para meter el nombre del jugador
    }

    #endregion server


}
