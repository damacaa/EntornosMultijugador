using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEditor;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public string Name { get; set; }

    public int ID { get; set; }

    public int CurrentPosition { get; set; }

    public int CurrentLap { get; set; }
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
                    CurrentLap++;
                }
                else if (a > 1)
                {
                    LastCheckPoint = MaxCheckPoints - 1;
                    CurrentLap--;
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
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "CheckPoint")
        {
            int id = collision.gameObject.GetComponent<Checkpoint>().id;
            if (id - LastCheckPoint == 1) { LastCheckPoint = id; }
            //
        }

        /*if (collision.gameObject.tag == "Finish")
        {
            //Meta
            if (LastCheckPoint == MaxCheckPoints - 1)
            {
                LastCheckPoint = 0;
                CurrentLap++;
                Debug.Log(name + " vuelta " + CurrentLap);
            }
            else if (LastCheckPoint == 0 && controller.goingBackwards)
            {
                CurrentLap--;
            }
        }*/
    }

    private void OnDrawGizmos()
    {
        Handles.Label(transform.position + transform.right, controller.DistToFinish.ToString());
        Handles.Label(transform.position + transform.right + Vector3.up, CurrentLap.ToString());
        Handles.Label(transform.position + transform.right + 2 * Vector3.up, segment.ToString());
        Handles.Label(transform.position + transform.right + 3 * Vector3.up, LastCheckPoint.ToString());
    }
}
