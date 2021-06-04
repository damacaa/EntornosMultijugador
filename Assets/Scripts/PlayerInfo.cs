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

    private int segment;
    public int Segment
    {
        get { return segment; }
        set { segment = value; }
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
            if (id == 0)
            {
                //Meta
                if (LastCheckPoint == MaxCheckPoints - 1)
                {
                    LastCheckPoint = 0;
                    CurrentLap++;
                    Debug.Log(name + " vuelta " + CurrentLap);
                }
                else if (CurrentLap == 0)
                {
                        LastCheckPoint = 0;
                        CurrentLap++;
                }else if (controller.goingBackwards)
                {
                    CurrentLap--;
                    LastCheckPoint = MaxCheckPoints - 1;
                }
            }
            else if (id - LastCheckPoint == 1)
            {
                LastCheckPoint = id;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Handles.Label(transform.position + transform.right, controller.DistToFinish.ToString());
        Handles.Label(transform.position + transform.right + Vector3.up, CurrentLap.ToString());
    }
}
