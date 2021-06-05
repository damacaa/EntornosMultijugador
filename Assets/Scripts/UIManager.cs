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
    private MyRoomManager m_RoomManager;
    [SerializeField] private PolePositionManager _polePositionManager;

    private GameObject player; //from here do commands



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
        m_RoomManager = FindObjectOfType<MyRoomManager>();
        if (!_polePositionManager) _polePositionManager = FindObjectOfType<PolePositionManager>();
        circuitLaps = FindObjectOfType<CircuitController>().circuitLaps;
    }

    private void Start()
    {
        buttonHost.onClick.AddListener(() => ActivateRoomHUDHost());
        buttonClient.onClick.AddListener(() => ActivateRoomHUDClient());
        buttonServer.onClick.AddListener(() => StartServer());
        //playButton.onClick.AddListener(() => ButtonPlay());
        practiceButton.onClick.AddListener(() => ButtonPractise());
        readyButton.onClick.AddListener(() => ButtonReady());
        m_RoomManager.UpdateListName("NO", "HACE NADA");
        ActivateMainMenu();
    }

    private void StartServer()
    {
        m_NetworkManager.StartServer();
        ActivateInGameHUD();
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

    private void ActivateMainMenu()
    {
        mainMenu.SetActive(true);
        inGameHUD.SetActive(false);
        roomHUD.SetActive(false);
    }

    public void ActivateInGameHUD()
    {
        inGameHUD.SetActive(true);
        mainMenu.SetActive(false);
        roomHUD.SetActive(false);
    }

    public void ActivateRoomHUDHost()
    {
        m_NetworkManager.StartHost();
        roomHUD.SetActive(true);
        playerNameClient.text = playerName.textComponent.text;
        carColorClient.text = carColor.text;
        inputFieldIP_Wait = inputFieldIP;
        //_polePositionManager.AddPlayersReady();
        readyButton.gameObject.SetActive(true);

        mainMenu.SetActive(false);
    }
    public void ActivateRoomHUDClient()
    {
        m_NetworkManager.StartClient();
        m_NetworkManager.networkAddress = inputFieldIP.text;
        roomHUD.SetActive(true);
        playerNameClient.text = playerName.textComponent.text;
        carColorClient.text = carColor.text;
        inputFieldIP_Wait = inputFieldIP;

        //_polePositionManager.AddPlayersReady();
        readyButton.gameObject.SetActive(true);
        mainMenu.SetActive(false);
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

    public void UpdateNameList(string[] newNames)
    {
        for (int i = 0; i < 4; i++)
        {
            playerNames[i].text = newNames[i];
        }
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

    public void TrainingOrRacing(bool isTraining)
    {
        practiceButton.gameObject.SetActive(isTraining);
        playButton.gameObject.SetActive(!isTraining);
    }


    public void SetReady(int playerIdx)
    {
        //player.GetComponent<SetupPlayer>().ChangeReadyName(playerIdx,true);
        _polePositionManager.SetPlayerReady(playerIdx,true);
        readyButton.gameObject.SetActive(false);
        notReadyButton.gameObject.SetActive(true);
    }
    public void SetNotReady(int playerIdx)
    {
        _polePositionManager.SetPlayerReady(playerIdx,false);
        //player.GetComponent<SetupPlayer>().ChangeReadyName(playerIdx,false);
        readyButton.gameObject.SetActive(true);
        notReadyButton.gameObject.SetActive(false);
    }

    public void ButtonPractise()
    {
        m_NetworkManager.StartHost();
        m_NetworkManager.networkAddress = inputFieldIP_Wait.text;
        roomHUD.SetActive(false);
        ActivateInGameHUD();
    }

    public void ButtonReady()
    {

    }

    public Button GetPlayButton()
    {
        return playButton;
    }

    public void GetReadyButton(int playerN)
    {
        readyButton.onClick.AddListener(()=>SetReady(playerN));

        notReadyButton.onClick.AddListener(()=>SetNotReady(playerN));
    }

    /*public void SetPlayerThatControls(GameObject p)
    {
        player = p;
    }*/


    #region car
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

    public void AddPlayerToRoom(PlayerInfo player, int pos)
    {
        playerNames[pos].text = player.Name;
        readyMarkers[pos].gameObject.SetActive(true);
    }
    #endregion car
}