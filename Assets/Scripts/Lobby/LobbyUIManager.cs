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
    [SerializeField] private Dropdown colorSelecter;
    [SerializeField] private Text carColor;

    public GameObject nameAndColorPrefab;
    private ColorAndName colorAndName;

    public Color[] playerColors;


    private void Awake()
    {
        m_NetworkManager = FindObjectOfType<MyNetworkManager>();
    }

    private void Start()
    {
        buttonHost.onClick.AddListener(() => StartHost());
        buttonClient.onClick.AddListener(() => StartClient());
        buttonServer.onClick.AddListener(() => StartServer());

        colorAndName = FindObjectOfType<ColorAndName>();

        if (!colorAndName)
        {
            GameObject g = GameObject.Instantiate(nameAndColorPrefab);
            colorAndName = g.GetComponent<ColorAndName>();
            DontDestroyOnLoad(g);
        }

        if (colorAndName.Name != "") { playerName.text = colorAndName.Name; }
        if (colorAndName.color != Color.clear)
        {
            for (int i = 0; i < colorSelecter.options.Count; i++)
            {
                if (colorAndName.color.Equals(playerColors[i]))
                {
                    colorSelecter.value = i;
                }
            }
        }
    }

    public void SetColor()
    {
        colorAndName.color = GetSelectedColor();
        colorAndName.Name = GetPlayerName();
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

        switch (color)
        {
            case "Verde":
                car = playerColors[1];
                break;
            case "Amarillo":
                car = playerColors[2];
                break;
            case "Blanco":
                car = playerColors[3];
                break;
            default:
                car = playerColors[0];
                break;
        }

        return car;
    }

}