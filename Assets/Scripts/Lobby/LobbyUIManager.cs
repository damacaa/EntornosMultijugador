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

    public GameObject nameAndColorPrefab;

    private void Awake()
    { 
        m_NetworkManager = FindObjectOfType<MyNetworkManager>();
    }

    private void Start()
    {
        buttonHost.onClick.AddListener(() => StartHost());
        buttonClient.onClick.AddListener(() => StartClient());
        buttonServer.onClick.AddListener(() => StartServer());
    }

    public void SetColor()
    {
        /*LobbyPlayer[] players = FindObjectsOfType<LobbyPlayer>();

        foreach (LobbyPlayer p in players)
        {
            if (p.isLocalPlayer)
            {
                p.color = GetCarSelected();
            }
        }*/

        GameObject g = GameObject.Instantiate(nameAndColorPrefab);
        ColorAndName c = g.GetComponent<ColorAndName>();

        c.color = GetSelectedColor();
        c.Name = GetPlayerName();

        DontDestroyOnLoad(g);
    }

    private void StartHost()
    {
        SetColor();
        m_NetworkManager.StartHost();
    }

    private void StartClient()
    {
        SetColor();
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
    public Color GetSelectedColor()
    {
        Color car = Color.red;

        var color = GetCarColor();
        if (color == "Verde")
        {
            car = Color.green;
        }
        else if (color == "Amarillo")
        {
            car = Color.yellow;
        }
        else if (color == "Blanco")
        {
            car = Color.white;
        }

        return car;
    }

}