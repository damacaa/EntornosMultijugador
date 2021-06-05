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
    [SerializeField] private GameObject roomHUD;
    [SerializeField] private Text[] playerNames;
    [SerializeField] private Text[] readyMarkers;
    [SerializeField] private Button playButton;
    [SerializeField] private Button practiceButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Text carColorClient;
    [SerializeField] private Text playerNameClient;
    [SerializeField] private InputField inputFieldIP_Wait;

    private int circuitLaps;

    private void Awake()
    {
        m_NetworkManager = FindObjectOfType<MyNetworkManager>();
        _polePositionManager = FindObjectOfType<PolePositionManager>();
        circuitLaps = FindObjectOfType<CircuitController>().circuitLaps;
    }

    private void Start()
    {
        buttonHost.onClick.AddListener(() => StartHost());
        buttonClient.onClick.AddListener(() => StartClient());
        buttonServer.onClick.AddListener(() => StartServer());
        playButton.onClick.AddListener(() => ButtonPlay());
        practiceButton.onClick.AddListener(() => ButtonPractise());
        readyButton.onClick.AddListener(() => ButtonPlay());//cambiarlo para poner qeu un jugador esta listo
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
        roomHUD.SetActive(false);
    }

    private void ActivateInGameHUD()
    {
        mainMenu.SetActive(false);
        inGameHUD.SetActive(true);
        roomHUD.SetActive(false);
    }

    private void ActivateRoomHUD()
    {
        roomHUD.SetActive(true);
        playerNameClient.text = playerName.textComponent.text;
        carColorClient.text = carColor.text;
        inputFieldIP_Wait = inputFieldIP;
        mainMenu.SetActive(false);
    }

    public void ActivateHostOptions()
    {
        readyButton.gameObject.SetActive(false);
        if (_polePositionManager.numPlayers > 1)
        {
            practiceButton.gameObject.SetActive(false);
            playButton.gameObject.SetActive(true);
        }
        else
        {
            playButton.gameObject.SetActive(false);
            practiceButton.gameObject.SetActive(true);
        }
    }

    public void ActivateClientOptions()
    {
        readyButton.gameObject.SetActive(true);
        practiceButton.gameObject.SetActive(false);
        playButton.gameObject.SetActive(false);

    }

    private void StartHost()
    {
        //m_NetworkManager.StartHost();
        m_NetworkManager.OnStartHost();
        //ActivateInGameHUD();
        ActivateRoomHUD();
    }

    private void StartClient()
    {
        m_NetworkManager.networkAddress = inputFieldIP.text;
       // m_NetworkManager.StartClient();
        m_NetworkManager.OnStartClient();
        ActivateRoomHUD();

    }

    public void ButtonPlay()
    {
        m_NetworkManager.StartClient();
        m_NetworkManager.networkAddress = inputFieldIP_Wait.text;
        roomHUD.SetActive(false);
        ActivateInGameHUD();
    }
    public void ButtonPractise()
    {
        m_NetworkManager.StartHost();
        m_NetworkManager.networkAddress = inputFieldIP_Wait.text;
        roomHUD.SetActive(false);
        ActivateInGameHUD();
    }
    private void StartServer()
    {
        m_NetworkManager.StartServer();
        ActivateInGameHUD();
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
    public int GetCarSelected() {
        int car = 0;
        var color = GetCarColor();
        if (color == "Verde")
        {
            car =1;
        }
        else if (color == "Amarillo")
        {
            car =2;
        }
        else if (color == "Blanco")
        {
            car =3;
        }
        return car;
    }

    public void AddPlayerToRoom(PlayerInfo player, int pos)
    {
        playerNames[pos].text = player.ToString();
    }
}