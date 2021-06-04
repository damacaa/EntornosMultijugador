
﻿using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : NetworkBehaviour
{
    public bool showGUI = true;

    private MyNetworkManager m_NetworkManager;

    [Header("Main Menu")] [SerializeField] private GameObject mainMenu;
    [SerializeField] private Button buttonHost;
    [SerializeField] private Button buttonClient;
    [SerializeField] private Button buttonServer;
    [SerializeField] private InputField inputFieldIP;
    [SerializeField] private InputField playerName;
    [SerializeField] private Text carColor;

    [Header("In-Game HUD")]
    [SerializeField]
    private GameObject inGameHUD;

    [SerializeField] private Text textSpeed;
    [SerializeField] private Text textLaps;
    [SerializeField] private Text textPosition;
    [SerializeField] private Text textTime;
    [SerializeField] private Text crashedWarning;
    [SerializeField] private Text backwardWarning;

    [Header("Client Wait HUD")]
    [SerializeField]
    private GameObject clientWait;
    private SyncList<string> playerNamesList;
    [SerializeField] private Text[] playerNames;
    [SerializeField] private Button playButtonClient;
    [SerializeField] private Text carColorClient;
    [SerializeField] private Text playerNameClient;
    private InputField inputFieldIP_Wait;

    private int circuitLaps;


    private void Awake()
    {
        m_NetworkManager = FindObjectOfType<MyNetworkManager>();
        circuitLaps = FindObjectOfType<CircuitController>().circuitLaps;
    }

    private void Start()
    {
        buttonHost.onClick.AddListener(() => StartHost());
        buttonClient.onClick.AddListener(() => ActivateClientWaitHUD());
        buttonServer.onClick.AddListener(() => StartServer());
        playButtonClient.onClick.AddListener(() => ButtonPlay());
        playerNamesList = new SyncList<string>();
        ActivateMainMenu();
    }

    public void UpdateSpeed(int speed)
    {
        textSpeed.text = "Speed " + speed + " Km/h";
    }

    public void UpdateLap(int lap)
    {
        textLaps.text = "Lap " + lap + "/" + circuitLaps;
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

    private void ActivateMainMenu()
    {
        mainMenu.SetActive(true);
        inGameHUD.SetActive(false);
    }

    private void ActivateInGameHUD()
    {
        mainMenu.SetActive(false);
        inGameHUD.SetActive(true);
    }

    private void ActivateClientWaitHUD()
    {
        clientWait.SetActive(true);
        playerNameClient.text = playerName.textComponent.text;
        carColorClient.text = carColor.text;
        inputFieldIP_Wait = inputFieldIP;
        mainMenu.SetActive(false);
    }

    private void StartHost()
    {
        m_NetworkManager.StartHost();
        ActivateInGameHUD();
    }

    private void StartClient()
    {
        m_NetworkManager.StartClient();
        m_NetworkManager.networkAddress = inputFieldIP.text;
        ActivateInGameHUD();
    }

    public void ButtonPlay()
    {
        m_NetworkManager.StartClient();
        m_NetworkManager.networkAddress = inputFieldIP_Wait.text;
        clientWait.SetActive(false);
    }
    private void StartServer()
    {
        m_NetworkManager.StartServer();
        ActivateInGameHUD();
    }

    public string GetPlayerName()
    {
        //return "Patata";
        return playerName.text;
    }

    public string GetCarColor()
    {
        return carColor.text;
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

    public void UpdateNames(string[] old, string[] newList)
    {
        for (int i=0; i < playerNamesList.Count; i++)
        {
            playerNames[i].text = newList[i];
        }
    }
}