using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public bool showGUI = true;

    private MyNetworkManager m_NetworkManager;
    [SerializeField] private PolePositionManager _polePositionManager;

    [Header("In-Game HUD")]
    [SerializeField]
    private GameObject inGameHUD;

    [SerializeField] private Text textSpeed;
    [SerializeField] private Text textLaps;
    [SerializeField] private Text textPosition;
    [SerializeField] private Text textTime;
    [SerializeField] private Text crashedWarning;
    [SerializeField] private Text backwardWarning;


    private int circuitLaps;

    private void Awake()
    {
        m_NetworkManager = FindObjectOfType<MyNetworkManager>();
        if (!_polePositionManager) _polePositionManager = FindObjectOfType<PolePositionManager>();
        circuitLaps = FindObjectOfType<CircuitController>().circuitLaps;
    }

    public void UpdateSpeed(float speed)
    {
        textSpeed.text = "Speed " + (int)(speed * 5f) + " Km/h";
    }

    public void UpdateLap(PlayerInfo player, int lap)
    {
        if (player.isLocalPlayer)
        {
            if (lap >= 0)
            {
                textLaps.text = "Lap " + lap + "/" + circuitLaps;
            }
        }
    }

    public void UpdateTime(string time)
    {
        textTime.text = time;
    }

    public void UpdateRanking(string ranking)
    {
        textPosition.text = ranking;
    }

    public void ShowCrashedWarning(bool hasCrashed)
    {
        crashedWarning.transform.parent.gameObject.SetActive(hasCrashed);
    }

    public void ShowBackwardsWarning(bool goingBackwards)
    {
        backwardWarning.transform.parent.gameObject.SetActive(goingBackwards);
    }

    public string GetPlayerName()
    {
        return "Paco";// playerName.text;
    }

    public string GetCarColor()
    {
        return "Rojo";// carColor.text;
    }

    //gets car's color selected from client UI
    public int GetCarSelected()
    {
        int car = 0;
        var color = GetCarColor();
        if (color == "Verde")
        {
            car = 1;
        }
        else if (color == "Amarillo")
        {
            car = 2;
        }
        else if (color == "Blanco")
        {
            car = 3;
        }
        return car;
    }
}