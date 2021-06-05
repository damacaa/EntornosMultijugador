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
        if (_uiManager == null) _uiManager = FindObjectOfType<UIManager>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "CheckPoint")
        {
            int id = collision.gameObject.GetComponent<Checkpoint>().id;
            if (id - LastCheckPoint == 1) { LastCheckPoint = id; }
            
        }
    }

    public void SetPlayerGO(GameObject car)
    {
        playerGO = car;
    }

    private void OnDrawGizmos()
    {
        Handles.Label(transform.position + transform.right, controller.DistToFinish.ToString());
        Handles.Label(transform.position + transform.right + Vector3.up, CurrentLap.ToString());
        Handles.Label(transform.position + transform.right + 2 * Vector3.up, segment.ToString());
        Handles.Label(transform.position + transform.right + 3 * Vector3.up, LastCheckPoint.ToString());
    }
}
