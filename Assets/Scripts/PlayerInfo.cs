using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerInfo : NetworkBehaviour
{

    private UIManager _uiManager;
    [SyncVar] public string Name;

    public int ID { get; set; }

    public int CurrentPosition { get; set; }

    public int CurrentLap { get; set; }
    public int LastCheckPoint;
    public int MaxCheckPoints;

    public PlayerController controller;

    [SyncVar] public bool isReady = false;
    [SyncVar(hook = nameof(onHostAuth))] public bool isAdmin = false;
    public override string ToString()
    {
        return Name;
    }

    private void Awake()
    {
        controller = gameObject.GetComponent<PlayerController>();
        if (_uiManager == null) _uiManager = FindObjectOfType<UIManager>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Finish")
        {
            if (LastCheckPoint >= MaxCheckPoints)
            {
                LastCheckPoint = 0;
                CurrentLap++;
                Debug.Log(name + " vuelta " + CurrentLap);
            }
        }

        if (collision.gameObject.tag == "CheckPoint")
        {
            int id = collision.gameObject.GetComponent<Checkpoint>().id;
            if (id - LastCheckPoint == 1)
            {
                LastCheckPoint = id;
            }
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        //Si el jugador es el host directamente esta listo
        if (isServer)
        {
            isReady = true;
        }
  
    }

    [Command]
    public void CmdSetReady(bool isReady)
    {
        this.isReady = isReady;
    }


    [Client]
    void onHostAuth(bool oldvalue, bool newvalue)
    {
        if (newvalue && isLocalPlayer)
        {
            Debug.Log("onHostAuth");
 
            _uiManager.setRoomHUDButtons(this);
            _uiManager.ActivateRoomHUD();
        }
    }
}
