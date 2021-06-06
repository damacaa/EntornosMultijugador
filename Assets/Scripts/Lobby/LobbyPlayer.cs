using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LobbyPlayer : NetworkRoomPlayer
{
    [SerializeField]public string _carColor ="";
    [SerializeField] public string _name;


    public override void OnStartClient()
    {
        // Debug.LogFormat(LogType.Log, "OnStartClient {0}", SceneManager.GetActiveScene().path);

        base.OnStartClient();
    }

    public override void OnClientEnterRoom()
    {
        // Debug.LogFormat(LogType.Log, "OnClientEnterRoom {0}", SceneManager.GetActiveScene().path);
    }

    public override void OnClientExitRoom()
    {
        // Debug.LogFormat(LogType.Log, "OnClientExitRoom {0}", SceneManager.GetActiveScene().path);
    }

    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
    {
        // Debug.LogFormat(LogType.Log, "ReadyStateChanged {0}", newReadyState);
    }


}

