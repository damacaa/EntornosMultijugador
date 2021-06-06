using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerInfoT : MonoBehaviour
{

    private UIManagerT _uiManager;
    public string Name;

    public int ID { get; set; }

    public int CurrentPosition { get; set; }

    public int CurrentLapSegments;
    public int CurrentLapCountingFromFinishLine;

    public int LastCheckPoint;
    public int MaxCheckPoints;

    public float CurrentLapTime = 0;
    public float TotalLapTime = 0;

    public PlayerControllerT controller;
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

    public override string ToString()
    {
        return Name;
    }

    private void Awake()
    {
        controller = gameObject.GetComponent<PlayerControllerT>();
        if (_uiManager == null) _uiManager = FindObjectOfType<UIManagerT>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "CheckPoint")
        {
            int id = collision.gameObject.GetComponent<CheckpointT>().id;
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

    //private void OnDrawGizmos()
    //{
    //    Handles.Label(transform.position + transform.right, controller.DistToFinish.ToString());

    //    Handles.Label(transform.position - transform.right + Vector3.up, CurrentLapSegments.ToString());
    //    Handles.Label(transform.position - transform.right + 2 * Vector3.up, CurrentLapCountingFromFinishLine.ToString());

    //    Handles.Label(transform.position + transform.right + Vector3.up, segment.ToString());
    //    Handles.Label(transform.position + transform.right + 2 * Vector3.up, LastCheckPoint.ToString());
    //}

    public void UpdateLapUI(int oldValue, int newValue)
    {
        _uiManager.UpdateLap(this, newValue);
    }


    public void UpdateTime(float old, float time)
    {

        _uiManager.UpdateTime("TIME: " +
            Math.Truncate(CurrentLapTime / 60) + ":" + Math.Round(CurrentLapTime % 60, 2)
        + "/" + Math.Truncate(TotalLapTime / 60) + ":" + Math.Round(TotalLapTime % 60, 2));
    }
}
