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

    public PlayerController controller;

    public override string ToString()
    {
        return Name;
    }

    private void Awake()
    {
        controller = gameObject.GetComponent<PlayerController>();
    }
}