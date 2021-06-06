using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LobbyPlayer : NetworkRoomPlayer
{
    public Color color;

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

