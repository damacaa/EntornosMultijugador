using System.Collections;
using System.Collections.Generic;
using Mirror;
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
}
