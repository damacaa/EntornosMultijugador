using System;
using Mirror;
using UnityEngine;
using Random = System.Random;

/*
	Documentation: https://mirror-networking.com/docs/Guides/NetworkBehaviour.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

public class SetupPlayer : NetworkBehaviour
{
    [SyncVar] private int _id;
    [SerializeField] private GameObject _carBody;
    [SyncVar(hook = nameof(ColorUpdate))] private Color _carColor = Color.black;
    [SyncVar(hook = nameof(ChangeName))] private string _name;

    private UIManager _uiManager;
    private MyNetworkManager _networkManager;
    private PlayerController _playerController;
    private PlayerInfo _playerInfo;
    private PolePositionManager _polePositionManager;
    private MyRoomManager m_RoomManager;
    #region Start & Stop Callbacks

    /// <summary>
    /// This is invoked for NetworkBehaviour objects when they become active on the server.
    /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
    /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();
        _id = NetworkServer.connections.Count - 1;
    }

    /// <summary>
    /// Called on every NetworkBehaviour when it is activated on a client.
    /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
    /// </summary>
    public override void OnStartClient()
    {
        base.OnStartClient();
        _playerInfo.ID = _id;
      
        _playerInfo.CurrentLap = 0;
        _polePositionManager.AddPlayer(_playerInfo);
        
        //_uiManager.SetPlayerThatControls(this.gameObject);
    }

    /// <summary>
    /// Called when the local player object has been set up.
    /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        int colorId = _uiManager.GetCarSelected();
        if (isClient)
        {
            CmdChangeColor(colorId);
            _playerInfo.SetPlayerGO(this.gameObject);
        }
        string nameFromUI = _uiManager.GetPlayerName();
        if (nameFromUI == "") { nameFromUI = "Player_" + UnityEngine.Random.Range(0, 1000); }
        CmdChangeName(nameFromUI);

    }

    #endregion

    private void Awake()
    {
        _playerInfo = GetComponent<PlayerInfo>();
        _playerController = GetComponent<PlayerController>();
        _networkManager = FindObjectOfType<MyNetworkManager>();
        _polePositionManager = FindObjectOfType<PolePositionManager>();
        _uiManager = FindObjectOfType<UIManager>();
        m_RoomManager = FindObjectOfType<MyRoomManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (isLocalPlayer)
        {
            _playerController.enabled = true;
            _playerController.OnSpeedChangeEvent += OnSpeedChangeEventHandler;
            _playerController.OnLapChangeEvent += OnLapChangeEventHandler;
            ConfigureCamera();
        }
    }

    private void Update()
    {

    }

    void OnSpeedChangeEventHandler(float speed)
    {
        _uiManager.UpdateSpeed((int)speed * 5); // 5 for visualization purpose (km/h)
    }

    void OnLapChangeEventHandler(int lap)
    {
        _uiManager.UpdateLap(lap);
    }


    void ConfigureCamera()
    {
        if (Camera.main != null) Camera.main.gameObject.GetComponent<CameraController>().m_Focus = this.gameObject;
    }

    public UIManager GetUi()
    {
        return _uiManager;
    }

    //change name function 

    void ChangeName(string oldName, string newName)
    {
        _playerInfo.Name = newName;
    }



    public void SetCarColor(Color newColor)
    {
        _carBody.GetComponent<MeshRenderer>().materials[1].color = newColor;
    }

    public Material GetActualCarColor()
    {
        return _carBody.GetComponent<MeshRenderer>().materials[1];
    }

    [Command]
    public void CmdChangeName(string newName)
    {
        _name = newName;
    }


    //changes sync var _carColor in server in order to later replicate this change to all clients
    [Command]
    public void CmdChangeColor(int id)
    {
        int colorIdx = id;
        if (colorIdx == 0)
        {
            _carColor = Color.red;

        }
        if (colorIdx == 1)
        {
            _carColor = Color.green;

        }
        if (colorIdx == 2)
        {
            _carColor = Color.yellow;

        }
        if (colorIdx == 3)
        {
            _carColor = Color.white;
        }
    }

    //updates car color in client
    public void ColorUpdate(Color old, Color newC)
    {
        SetCarColor(newC);
    }

    public void ChangeReadyName(int player, bool isReady)
    {
        CmdChangeReadyName(player, isReady);
    }

    [Command]
    public void CmdChangeReadyName(int player, bool isReady)
    {
        m_RoomManager.changeReadyName(player,isReady);
    }


}