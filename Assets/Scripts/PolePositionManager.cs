using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Mirror;
using UnityEngine;

public class PolePositionManager : NetworkBehaviour
{
    public int numPlayers;
    private MyNetworkManager _networkManager;
    private UIManager _uiManager;

    private readonly List<PlayerInfo> _players = new List<PlayerInfo>(4);
    private CircuitController _circuitController;
    private GameObject[] _debuggingSpheres;






    #region Variables de Tiempo
    //Tiempo de la vuelta actual
    [SyncVar] private float lapTime = 0;
    //Tiempo Total de la carrera
    [SyncVar] private float totalTime = 0;

    #endregion

    #region Comprobaciones
    //Variable que indica si el localPlayer esta yendo en direccion contraria
    private bool goingBackwards = false;

    public bool GoingBackwards
    {
        get { return goingBackwards; }
        set
        {
            if (OnGoingBackwardsEvent != null && goingBackwards != value)
                OnGoingBackwardsEvent(value);

            goingBackwards = value;
        }
    }

    //Variable que indica si el localPlayer se ha chocado
    private bool hasCrashed = false;

    public bool HasCrashed
    {
        get { return hasCrashed; }
        set
        {
            if (OnHasCrashedEvent != null && hasCrashed != value)
                OnHasCrashedEvent(value);

            hasCrashed = value;
        }
    }

    //Boolean que indica si ha empezado la carrera
    private bool hasRaceStarted = false;
    //Boolean que indica si ha acabado la carrera
    private bool hasRaceEnded = false;



    #endregion

    #region Eventos

    public delegate void OnRaceStartDelegate(bool newVal);
    public event OnRaceStartDelegate OnRaceStartEvent;

    public delegate void OnRaceEndDelegate(bool newVal);
    public event OnRaceEndDelegate OnRaceEndEvent;

    public delegate void OnGoingBackwardsDelegate(bool newVal);
    public event OnGoingBackwardsDelegate OnGoingBackwardsEvent;

    public delegate void OnHasCrashedDelegate(bool newVal);
    public event OnHasCrashedDelegate OnHasCrashedEvent;

    public delegate void OnRankingChangeDelegate(string newVal);
    public event OnRankingChangeDelegate OnRankingChangeEvent;


    #endregion

    private void Awake()
    {
        if (_networkManager == null) _networkManager = FindObjectOfType<MyNetworkManager>();
        if (_circuitController == null) _circuitController = FindObjectOfType<CircuitController>();
        if (_uiManager==null) _uiManager = FindObjectOfType<UIManager>();
        //ELIMINAMOS LA GENERACION DE ESFERAS INNECESARIAS

        _debuggingSpheres = new GameObject[_networkManager.maxConnections]; //deberia ser solo 1 la del jugador y se pasan todas al server para que calcule quien va primero
        for (int i = 0; i < _networkManager.maxConnections; ++i)
        {
            _debuggingSpheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _debuggingSpheres[i].GetComponent<SphereCollider>().enabled = false;
        }

    }

    private void Start()
    {
       /* if (isLocalPlayer)
        {
        }*/
        this.OnRankingChangeEvent += OnRankingChangeEventHandler;
        this.OnHasCrashedEvent += OnHasCrashedEventHandler;
        this.OnGoingBackwardsEvent += OnGoingBackwardsEventHandler;
    }

    private void Update()
    {
        if (isServer)
        {
            if (_players.Count == 0)
                return;
            UpdateRaceProgress();
        }
    }

    public void AddPlayer(PlayerInfo player)
    {
        _players.Add(player);
    }

    private class PlayerInfoComparer : Comparer<PlayerInfo>
    {
        float[] _arcLengths;

        public PlayerInfoComparer(float[] arcLengths)
        {
            _arcLengths = arcLengths;
        }

        public override int Compare(PlayerInfo x, PlayerInfo y)
        {
            if (_arcLengths[x.ID] > _arcLengths[y.ID])
                return 1;
            else return -1;
        }
    }

    [Server]
    public void UpdateRaceProgress()
    {
        // Update car arc-lengths
        float[] arcLengths = new float[_players.Count];

        for (int i = 0; i < _players.Count; ++i)
        {
            float l = ComputeCarArcLength(i); //Distancia restante hasta la meta

            _players[i].controller.ArcLength = l;
            arcLengths[i] = l;
        }

        _players.Sort(new PlayerInfoComparer(arcLengths));

        string myRaceOrder = "";
        foreach (var player in _players)
        {
            myRaceOrder += player.Name + " ";
        }
    }

    void OnRankingChangeEventHandler(string ranking)
    {
        _uiManager.UpdateRanking(ranking);
        //Debug.Log("El orden de carrera es: " + myRaceOrder);
    }

    void OnHasCrashedEventHandler(bool hasCrashed)
    {
        _uiManager.ShowCrashedWarning(hasCrashed);
    }

    void OnGoingBackwardsEventHandler(bool goingBackwards)
    {
        _uiManager.ShowBackwardsWarning(goingBackwards);
    }

    float ComputeCarArcLength(int id)
    {
        // Compute the projection of the car position to the closest circuit 
        // path segment and accumulate the arc-length along of the car along
        // the circuit.
        Vector3 carPos = this._players[id].transform.position;

        int segIdx;
        float carDist;
        Vector3 carProj;

        float minArcL =
            this._circuitController.ComputeClosestPointArcLength(carPos, out segIdx, out carProj, out carDist);

        this._debuggingSpheres[id].transform.position = carProj;

        if (this._players[id].CurrentLap == 0)
        {
            minArcL -= _circuitController.CircuitLength;
        }
        else
        {
            minArcL += _circuitController.CircuitLength *
                       (_players[id].CurrentLap - 1);
        }

        return minArcL;
    }
}