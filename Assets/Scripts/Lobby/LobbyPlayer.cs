using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LobbyPlayer : NetworkRoomPlayer
{
    public Color color;

    public override void OnStartLocalPlayer()
    {
        
    }

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
        ColorAndName colorAndName = FindObjectOfType<ColorAndName>();
        if (colorAndName == null) { Debug.Log("Caca"); }
        else
        {

            LobbyPlayer[] players = FindObjectsOfType<LobbyPlayer>();
            foreach (LobbyPlayer p in players)
            {
                if (p.isLocalPlayer)
                {
                    p.color = colorAndName.color;
                    p.name = colorAndName.Name;
                }
            }

            Destroy(colorAndName);
        }
    }
}

