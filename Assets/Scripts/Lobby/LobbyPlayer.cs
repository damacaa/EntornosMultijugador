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

    /// <summary>
    /// Funcion que se llama al cambiar el estado de la variable ready
    /// </summary>
    /// <param name="oldReadyState">Antiguo valor de ready</param>
    /// <param name="newReadyState">Nuevo valor de ready</param>
    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
    {

        ColorAndName colorAndName = FindObjectOfType<ColorAndName>();
        if (colorAndName != null && newReadyState) {

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
}
