using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEditor;
using UnityEngine;

public class PlayerInfo : NetworkBehaviour
{

    private UIManager _uiManager;
    [SyncVar] public string Name;

    public int ID { get; set; }

    public int CurrentPosition { get; set; }

    public int CurrentLapSegments;
    [SyncVar(hook = nameof(UpdateLapUI))] public int CurrentLapCountingFromFinishLine;
    //[SyncVar(hook = nameof(UpdateSpeedUI))] public float Speed = 0;

    public int LastCheckPoint;
    public int MaxCheckPoints;

    [SyncVar(hook = nameof(UpdateTime))] public float CurrentLapTime = 0;
    public float TotalLapTime = 0;

    public PlayerController controller;
    public float InitialDistToFinish = 0;

    private int segment;
    public int Segment
    {
        get { return segment; }
        set
        {
            if (value != segment)
            {
                int a = value - segment;
                if (a < -1 && LastCheckPoint == MaxCheckPoints - 1)
                {
                    LastCheckPoint = 0;
                    CurrentLapSegments++;
                }
                else if (a > 1)
                {
                    LastCheckPoint = MaxCheckPoints - 1;
                    CurrentLapSegments--;
                }
                segment = value;

            }
        }
    }

    //[SyncVar(hook = nameof(UpdateReadyOnUI))] public bool isReady = false;
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
        if (collision.gameObject.tag == "CheckPoint")
        {
            int id = collision.gameObject.GetComponent<Checkpoint>().id;
            if (id - LastCheckPoint == 1) { LastCheckPoint = id; }
            //
        }

        if (collision.gameObject.tag == "Finish")
        {
            //Meta
            if (LastCheckPoint == MaxCheckPoints - 1 && CurrentLapSegments >= 0)
            {
                if (CurrentLapCountingFromFinishLine <= CurrentLapSegments + 1)
                {
                    CurrentLapTime = 0;
                    CurrentLapCountingFromFinishLine = CurrentLapSegments + 2;
                }
            }
            else if (CurrentLapSegments < 0)
            {
                CurrentLapCountingFromFinishLine = 1;
            }
        }
    }

    private void OnDrawGizmos()
    {
       /* Handles.Label(transform.position + transform.right, controller.DistToFinish.ToString());

        Handles.Label(transform.position - transform.right + Vector3.up, CurrentLapSegments.ToString());
        Handles.Label(transform.position - transform.right + 2 * Vector3.up, CurrentLapCountingFromFinishLine.ToString());

        Handles.Label(transform.position + transform.right + Vector3.up, segment.ToString());
        Handles.Label(transform.position + transform.right + 2 * Vector3.up, LastCheckPoint.ToString());*/
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        //Si el jugador es el host directamente esta listo
        if (isServer)
        {
            //isReady = true;
        }

    }

    [Command]
    public void CmdSetReady(bool isReady)
    {
        //this.isReady = isReady;
    }


    [Client]
    void onHostAuth(bool oldvalue, bool newvalue)
    {
        if (newvalue && isLocalPlayer)
        {
            Debug.Log("onHostAuth");

            _uiManager.setEndRaceHUDButtons(this);
            //_uiManager.ActivateRoomHUD();
        }
    }

    /*[Client]
    void UpdateReadyOnUI(bool oldValue, bool newValue)
    {
        _uiManager.readyMarkers[ID].text = (newValue) ? "Ready":"";
    }*/


    public void UpdateLapUI(int oldValue, int newValue)
    {
        _uiManager.UpdateLap(this, newValue);
    }


    /*public void UpdateSpeed(float newValue)
    {
        if (Math.Abs(newValue - Speed) < float.Epsilon) return;
        Speed = newValue;
    }*/

    /*[Client]
    public void UpdateSpeedUI(float oldValue, float newValue)
    {
        _uiManager.UpdateSpeed(this, newValue);
    }*/

    public void UpdateTime(float old, float time)
    {
        if (isLocalPlayer)
        {
            _uiManager.UpdateTime("TIME: " +
                Math.Round(CurrentLapTime / 60) + ":" + Math.Round(CurrentLapTime % 60, 2)
            + "/" + Math.Round(TotalLapTime / 60) + ":" + Math.Round(TotalLapTime % 60, 2));
        }
    }
}
