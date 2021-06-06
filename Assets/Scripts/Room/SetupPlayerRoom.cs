using System;
using Mirror;
using UnityEngine;
using Random = System.Random;

/*
	Documentation: https://mirror-networking.com/docs/Guides/NetworkBehaviour.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

/// <summary>
/// Es el primer componente qeu se ejecuta y se encarga de añadir el resto de componentes al jugador
/// </summary>
public class SetupPlayerRoom : NetworkRoomPlayer
{
    /// <summary>
    /// Id del jugador
    /// </summary>
    [SyncVar] private int _id;
    
    /// <summary>
    /// GO del coche
    /// </summary>
    [SerializeField] private GameObject _carBody;

    //Color del coche
    [SyncVar(hook = nameof(ColorUpdate))] private Color _carColor = Color.black;

    //nombre del jugador
    [SyncVar(hook = nameof(ChangeName))] private string _name;

    private UIManager _uiManager;
    private MyNetworkManager _networkManager;
    private PlayerController _playerController;
    private PlayerInfo _playerInfo;
    private PolePositionManager _polePositionManager;

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

       /* string nameFromUI = FindObjectOfType<LobbyUIManager>().GetPlayerName();
        if(nameFromUI == "") { nameFromUI = "Player_" + UnityEngine.Random.Range(0, 100); }
        CmdChangeName(nameFromUI);*/

        _playerInfo.CurrentLapSegments = 0;
        _polePositionManager.AddPlayer(_playerInfo);
    }

    /// <summary>
    /// Called when the local player object has been set up.
    /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    /*public override void OnStartLocalPlayer()
    {
        //int colorId = FindObjectOfType<LobbyUIManager>().GetCarSelected();
        if (isClient)
        {
            Debug.Log(colorId);
            CmdChangeColor(colorId);
        }
    }*/

    #endregion


    //referencias
    private void Awake()
    {

        _playerInfo = GetComponent<PlayerInfo>();
        _playerController = GetComponent<PlayerController>();
        _networkManager = FindObjectOfType<MyNetworkManager>();
        _polePositionManager = FindObjectOfType<PolePositionManager>();
        _uiManager = FindObjectOfType<UIManager>();

    }

    private new void Start()
    {
        if (isLocalPlayer)
        {
            _playerController.enabled = true;
            ConfigureCamera();
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        LobbyPlayer[] players = FindObjectsOfType<LobbyPlayer>();

        foreach (LobbyPlayer p in players)
        {
            if (p.isLocalPlayer) {
                if (isClientOnly)
                {
                    CmdChangeColorInServer(p.color);
                    CmdChangeNameInServer(p.name);
                }
                else
                {
                    _carColor = p.color;
                    _playerInfo.Name = p.name;
                }
            }
        }
    }

    /// <summary>
    /// Cambia el color del coche del cliente en el servidor
    /// </summary>
    /// <param name="c">color del coche</param>
    [Command]
    private void CmdChangeColorInServer(Color c)
    {
        _carColor = c;
    }

    /// <summary>
    /// Cambia el nombre del cliente en el servidor
    /// </summary>
    /// <param name="n">Nombre del jugador</param>
    [Command]
    private void CmdChangeNameInServer(string n)
    {
        _name = n;
    }

   
    void ConfigureCamera()
    {
        if (Camera.main != null) Camera.main.gameObject.GetComponent<CameraController>().m_Focus = this.gameObject;
    }

    //change name hook function
    void ChangeName(string oldName, string newName)
    {
        _playerInfo.Name = newName;
    }

    public void SetCarColor(Color newColor)
    {
        _carBody.GetComponent<MeshRenderer>().materials[1].color = newColor;
    }


    //updates car color in client
    public void ColorUpdate(Color old, Color newC)
    {
        SetCarColor(newC);
    }

    /// <summary>
    /// Al desconectarse un cliente del servidor se destruye este componente, en ese momento se borra de la lista de jugadores
    /// </summary>
    private void OnDestroy()
    {
        _polePositionManager.RemovePlayer(_playerInfo);
    }
}
