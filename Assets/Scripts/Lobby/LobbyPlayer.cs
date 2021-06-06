using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase necesaria para el manager de salas, asignado a una prefab del coche
/// </summary>
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

    /// <summary>
    /// Funcion que se llama al cambiar el estado de la variable ready
    /// </summary>
    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
    {
        base.ReadyStateChanged(oldReadyState, newReadyState);

        ColorAndName colorAndName = FindObjectOfType<ColorAndName>();
        if (colorAndName == null) { Debug.Log("Color no encontrado"); }
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

        }
    }

    /*public override void OnGUI()
    {
        
        if (!showRoomGUI)
            return;

        NetworkRoomManager room = NetworkManager.singleton as NetworkRoomManager;
        if (room)
        {
            if (!room.showRoomGUI)
                return;

            if (!NetworkManager.IsSceneActive(room.RoomScene))
                return;

            ///
             GUILayout.BeginArea(new Rect(20f + (index * 100), 200f, 90f, 130f));

            GUILayout.Label(name);

            if (readyToBegin)
                GUILayout.Label("Ready");
            else
                GUILayout.Label("Not Ready");

            if (((isServer && index > 0) || isServerOnly) && GUILayout.Button("REMOVE"))
            {
                // This button only shows on the Host for all players other than the Host
                // Host and Players can't remove themselves (stop the client instead)
                // Host can kick a Player this way.
                GetComponent<NetworkIdentity>().connectionToClient.Disconnect();
            }

            GUILayout.EndArea();
            ///
            if (NetworkClient.active && isLocalPlayer)
            {
                GUILayout.BeginArea(new Rect(20f, 300f, 120f, 20f));

                if (readyToBegin)
                {
                    if (GUILayout.Button("Cancel"))
                        CmdChangeReadyState(false);
                }
                else
                {
                    if (GUILayout.Button("Ready"))
                        CmdChangeReadyState(true);
                }

                GUILayout.EndArea();
            }
        }
    }*/


}

