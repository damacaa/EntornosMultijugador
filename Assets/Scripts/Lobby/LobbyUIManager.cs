using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
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

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
      
        m_NetworkManager = FindObjectOfType<MyNetworkManager>();
    }

    private void Start()
    {
        buttonHost.onClick.AddListener(() => StartHost());
        buttonClient.onClick.AddListener(() => StartClient());
        buttonServer.onClick.AddListener(() => StartServer());
    }

    private void StartHost()
    {
        m_NetworkManager.StartHost();
    }

    private void StartClient()
    {
        m_NetworkManager.StartClient();
        m_NetworkManager.networkAddress = inputFieldIP.text;
    }

    public void ButtonPlay()
    {
        m_NetworkManager.StartClient();
    }

    private void StartServer()
    {
        m_NetworkManager.StartServer();
    }

    public string GetPlayerName()
    {
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

}