using System;
using UnityEngine;
using Random = System.Random;

public class SetupPlayerEnt : MonoBehaviour
{
    private int _id;
    [SerializeField] private GameObject _carBody;
    private Color _carColor = Color.black;
    private string _name;

    private UIManagerT _uiManager;
    private PlayerControllerT _playerController;
    private PlayerInfoT _playerInfo;
    private PolePositionManagerT _polePositionManager;

    #region Start & Stop Callbacks

    /// <summary>
    /// This is invoked for NetworkBehaviour objects when they become active on the server.
    /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
    /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
    /// </summary>
    //public override void OnStartServer()
    //{
    //    base.OnStartServer();
    //    _id = NetworkServer.connections.Count - 1;
    //}

    /// <summary>
    /// Called on every NetworkBehaviour when it is activated on a client.
    /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
    /// </summary>
    //public override void OnStartClient()
    //{
    //    base.OnStartClient();
    //    _playerInfo.ID = _id;

    /* string nameFromUI = FindObjectOfType<LobbyUIManager>().GetPlayerName();
     if(nameFromUI == "") { nameFromUI = "Player_" + UnityEngine.Random.Range(0, 100); }
     CmdChangeName(nameFromUI);*/

  
    //}

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



    private void Awake()
    {

        _playerInfo = GetComponent<PlayerInfoT>();
        _playerController = GetComponent<PlayerControllerT>();
        _polePositionManager = FindObjectOfType<PolePositionManagerT>();
        _uiManager = FindObjectOfType<UIManagerT>();


    }
    private void Start()
    {

        _playerInfo.ID = _id;
        _playerInfo.CurrentLapSegments = 0;
        _polePositionManager.AddPlayer(_playerInfo);
        _playerController.enabled = true;
        ConfigureCamera();

    }
    void ConfigureCamera()
    {
        if (Camera.main != null) Camera.main.gameObject.GetComponent<CameraControllerT>().m_Focus = this.gameObject;
    }

    public UIManagerT GetUi()
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


    public void CmdChangeName(string newName)
    {
        _name = newName;
    }


    //updates car color in client
    public void ColorUpdate(Color old, Color newC)
    {
        SetCarColor(newC);
    }

    private void OnDestroy()
    {
        _polePositionManager.RemovePlayer(_playerInfo);
    }
}
