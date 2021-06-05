using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEditor;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{

    private UIManager _uiManager;
    public string Name;

    public GameObject playerGO;
    public int ID { get; set; }

    public int CurrentPosition { get; set; }
    public float CurrentLapTime { get; set; }
    public float TotalTime { get; set; }

    public int CurrentLapSegments;
    public int CurrentLapCountingFromFinishLine;

    public int LastCheckPoint;
    public int MaxCheckPoints;

    public PlayerController controller;
    public float InitialDistToFinish = 0;

    private bool dirty = false;
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

    public void SetPlayerGO(GameObject car)
    {
        playerGO = car;
    }

    /*private void OnDrawGizmos()
    {
        Handles.Label(transform.position + transform.right, controller.DistToFinish.ToString());

        Handles.Label(transform.position - transform.right + Vector3.up, CurrentLapSegments.ToString());
        Handles.Label(transform.position - transform.right + 2 * Vector3.up, CurrentLapCountingFromFinishLine.ToString());

        Handles.Label(transform.position + transform.right + Vector3.up, CurrentLapTime.ToString());
        Handles.Label(transform.position + transform.right + 2 * Vector3.up, TotalTime.ToString());
    }


    /*
    [Client]
    public void UpdateLapUI(int oldValue, int newValue)
    {
        _uiManager.UpdateLap(this, newValue);
    }

    [ClientRpc]
    public void UpdateTimeUI(float oldValue, float newValue)
    {
        _uiManager.UpdateTime(this);
    }*/
}
