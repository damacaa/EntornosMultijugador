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
    [SerializeField] private Button notReadyButton;
    [SerializeField] private Text carColorClient;
    [SerializeField] private Text playerNameClient;
    [SerializeField] private InputField inputFieldIP_Wait;

    private int circuitLaps;

    private void Awake()
    {
        m_NetworkManager = FindObjectOfType<MyNetworkManager>();
        if (!_polePositionManager) _polePositionManager = FindObjectOfType<PolePositionManager>();
        circuitLaps = _polePositionManager.laps;
    }
    private void Start()
    {
        buttonHost.onClick.AddListener(() => StartHost());
        buttonClient.onClick.AddListener(() => StartClient());
        buttonServer.onClick.AddListener(() => StartServer());

        playButton.onClick.AddListener(() => ButtonPlay());
        practiceButton.onClick.AddListener(() => ButtonPractise());

        ActivateMainMenu();
    }

    

    public void UpdateSpeed(float speed)
    {
            textSpeed.text = "Speed " + (int) (speed * 5f)+ " Km/h";
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

    /*[ClientRpc]
    public void UpdateTime(PlayerInfo player)
    {
        if (player.isLocalPlayer)
        {
                textTime.text = "TIME: " + player.CurrentLapTime + "/" + player.TotalLapTime;
        }
    }*/

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

    public void ActivateMainMenu()
    {
        mainMenu.SetActive(true);
        inGameHUD.SetActive(false);
        roomHUD.SetActive(false);
    }

    public void ActivateInGameHUD()
    {
        mainMenu.SetActive(false);
        inGameHUD.SetActive(true);
        roomHUD.SetActive(false);
    }

    public void ActivateRoomHUD()
    {
        roomHUD.SetActive(true);
        playerNameClient.text = playerName.textComponent.text;
        carColorClient.text = carColor.text;
        inputFieldIP_Wait = inputFieldIP;
        mainMenu.SetActive(false);
        inGameHUD.SetActive(false);
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

    public void setRoomHUDButtons(PlayerInfo localPlayer)
    {
        readyButton.onClick.RemoveAllListeners();
        notReadyButton.onClick.RemoveAllListeners();
        playButton.onClick.RemoveAllListeners();

        if (localPlayer.isServer)
        {
            readyButton.gameObject.SetActive(false);
            notReadyButton.gameObject.SetActive(false);
            playButton.gameObject.SetActive(true);
            playButton.onClick.AddListener(() => _polePositionManager.StartRace());
        }
        else
        {
            playButton.gameObject.SetActive(false);
            readyButton.gameObject.SetActive(true);
            notReadyButton.gameObject.SetActive(localPlayer.isReady);
            readyButton.onClick.AddListener(() => ButtonReady(localPlayer));
            notReadyButton.onClick.AddListener(() => ButtonNotReady(localPlayer));
        }

    }

    public void TrainingOrRacing(bool isTraining)
    {
        practiceButton.gameObject.SetActive(isTraining);
        playButton.gameObject.SetActive(!isTraining);
    }

    private void StartHost()
    {
        m_NetworkManager.StartHost();
        //m_NetworkManager.OnStartHost();
        //ActivateInGameHUD();
        ActivateRoomHUD();
    }

    private void StartClient()
    {
        m_NetworkManager.StartClient();
        m_NetworkManager.networkAddress = inputFieldIP.text;
        //m_NetworkManager.OnStartClient();
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
        //m_NetworkManager.networkAddress = inputFieldIP_Wait.text;
        roomHUD.SetActive(false);
        ActivateInGameHUD();
        _polePositionManager.StartRace();
    }

    private void ButtonReady(PlayerInfo player)
    {
        player.CmdSetReady(true);
        readyButton.gameObject.SetActive(false);
        notReadyButton.gameObject.SetActive(true);
    }
    private void ButtonNotReady(PlayerInfo player)
    {
        player.CmdSetReady(false);
        readyButton.gameObject.SetActive(true);
        notReadyButton.gameObject.SetActive(false);
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
        playerNames[pos].text = player.Name;
        readyMarkers[pos].gameObject.SetActive(true);
    }
}