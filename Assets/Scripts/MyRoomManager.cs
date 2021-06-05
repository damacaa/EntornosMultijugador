using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyRoomManager : NetworkBehaviour
{
    [SerializeField] public UIManager _ui;
    [SerializeField] string[] playerNames;
    [SyncVar(hook = nameof(UpdateListName))] public string player1;
    [SyncVar(hook = nameof(UpdateListName))] public string player2;
    [SyncVar(hook = nameof(UpdateListName))] public string player3;
    [SyncVar(hook = nameof(UpdateListName))] public string player4;

    // Start is called before the first frame update
    void Awake()
    {
        playerNames = new string[4];
        playerNames[0] = ""; 
        playerNames[1] = "";
        playerNames[2] = "";
        playerNames[3] = "";
        player1 = "";
        player2 = "";
        player3 = "";
        player4 = "";


    }

    public void UpdateListName(string old, string newV)
    {
        playerNames[0] = player1;
        playerNames[1] = player2;
        playerNames[2] = player3;
        playerNames[3] = player4;
        _ui.UpdateNameList(playerNames);
    }
    
    public void changeReadyName(int playerIndex,bool isReady)
    {
        switch (playerIndex)
        {
            case 0:
                if (isReady) player1 = "READY";
                else player1 = "";
                break;
            case 1:
                if (isReady) player2 = "READY";
                else player1 = "";
                break;
            case 2:
                if (isReady) player3 = "READY";
                else player1 = "";
                break;
            case 3:
                if (isReady) player4 = "READY";
                else player1 = "";
                break;
        }
    }
}
