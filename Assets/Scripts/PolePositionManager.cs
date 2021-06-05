using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Mirror;
using UnityEngine;

public class PolePositionManager : NetworkBehaviour
{
    private MyNetworkManager _networkManager;
    private UIManager _uiManager;

    private CircuitController _circuitController;
    private GameObject[] _debuggingSpheres;

    [Header("RaceStartingPositions")]
    public Transform[] startingPoints;

    [SyncVar] public int numPlayers = 4;
    [SyncVar] public int laps = 3;
    int currentPlayers;

    private readonly List<PlayerInfo> _players = new List<PlayerInfo>();

    public bool isTrainingRace = true;
    public bool openRoom = true;

    [Header("RaceProgress")]
    string myRaceOrder = "";
    //Boolean que indica si ha empezado la carrera
    [SyncVar] public bool racing = false;


    #region Variables de Tiempo
    //Tiempo de la vuelta actual
    [SyncVar] private float lapTime = 0;
    //Tiempo Total de la carrera
    [SyncVar] private float totalTime = 0;

    #endregion

    #region Comprobaciones
    //Variable que indica si el localPlayer esta yendo en direccion contraria
    private bool goingBackwards = false;


    //Boolean que indica si ha acabado la carrera
    private bool hasRaceEnded = false;
    #endregion

    #region Eventos

    public delegate void OnRaceStartDelegate(bool newVal);
    public event OnRaceStartDelegate OnRaceStartEvent;

    public delegate void OnRaceEndDelegate(bool newVal);
    public event OnRaceEndDelegate OnRaceEndEvent;




    #endregion

    private void Awake()
    {
        if (_networkManager == null) _networkManager = FindObjectOfType<MyNetworkManager>();
        if (_circuitController == null) _circuitController = FindObjectOfType<CircuitController>();
        if (_uiManager == null) _uiManager = FindObjectOfType<UIManager>();
        //ELIMINAMOS LA GENERACION DE ESFERAS INNECESARIAS

        _debuggingSpheres = new GameObject[_networkManager.maxConnections]; //deberia ser solo 1 la del jugador y se pasan todas al server para que calcule quien va primero
        for (int i = 0; i < _networkManager.maxConnections; ++i)
        {
            _debuggingSpheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _debuggingSpheres[i].GetComponent<SphereCollider>().enabled = false;
        }

        racing = true;
    }


    public bool waiting = true;
    private void Update()
    {
        if (isServer)
        {
            if (_players.Count == 0)
                return;

            if (racing)
            {
                totalTime += Time.deltaTime;
                UpdateRaceProgress();
                if (CheckFinish())
                {
                    racing = false;
                    Finish();
                    ResetPlayers();
                }
            }
            else if (waiting)
            {
                waiting = false;
                ResetPlayers();
            }
        }
    }

    [Server]
    public void StartRace()
    {
        bool everyOneIsReady = true;
        foreach (PlayerInfo player in _players)
        {
            everyOneIsReady = player.isReady && everyOneIsReady;
        }
        if (everyOneIsReady)
        {
            numPlayers = _players.Count;
            racing = true;
            RpcChangeFromRoomToGameHUD();
        }
    }

    private bool CheckFinish()
    {
        for (int i = 0; i < _players.Count; ++i)
        {
            if (_players[i].CurrentLapCountingFromFinishLine == laps + 1)
            {
                Debug.Log("Vencedor: " + _players[i].name + totalTime);
                totalTime = 0;
                return true;
            }
        }
        return false;
    }

    [Server]
    private void ResetPlayers()
    {
        for (int i = 0; i < _players.Count; ++i)
        {
            _players[i].CurrentLapSegments = 1;
            _players[i].CurrentLapCountingFromFinishLine = 1;
            _players[i].LastCheckPoint = _circuitController.checkpoints.Count - 1; ;
            _players[i].controller.ResetToStart(startingPoints[i]);
            _players[i].controller.DistToFinish = ComputeCarArcLength(i);
            _players[i].controller.InitialDistToFinish = _players[i].controller.DistToFinish;

            StartCoroutine(DelayStart(3f));
        }
    }

    IEnumerator DelayStart(float t)
    {
        yield return new WaitForSeconds(t / 3);
        Debug.Log("Preparados...");
        yield return new WaitForSeconds(t / 3);
        Debug.Log("Listos...");
        yield return new WaitForSeconds(t / 3);
        Debug.Log("Ya!");
        racing = true;
        yield return null;
    }

    [ClientRpc]
    private void Finish()
    {
        Debug.Log("Fin");
    }

    public void AddPlayer(PlayerInfo player)
    {
        //AQUI FALTA UN COMENTARIO
        player.MaxCheckPoints = _circuitController.checkpoints.Count;
        currentPlayers++;
        _players.Add(player);

        if (isServer)
        {
            player.isAdmin = true;
            player.transform.position = startingPoints[_players.Count - 1].position;
            player.transform.rotation = startingPoints[_players.Count - 1].rotation;
            _uiManager.AddPlayerToRoom(player, _players.Count - 1);

        }

        isTrainingRace = _players.Count < 2;

        _uiManager.TrainingOrRacing(isTrainingRace);

    }

    private class PlayerInfoComparer : Comparer<PlayerInfo>
    {
        public override int Compare(PlayerInfo x, PlayerInfo y)
        {
            if (x.controller.DistToFinish < y.controller.DistToFinish)
                return 1;
            else return -1;
        }
    }

    [Server]
    public void UpdateRaceProgress()
    {
        // Update car arc-lengths

        for (int i = 0; i < _players.Count; ++i)
        {
            _players[i].controller.DistToFinish = ComputeCarArcLength(i); //Distancia restante hasta la meta

        }

        _players.Sort(new PlayerInfoComparer());

        myRaceOrder = "";

        for (int i = 0; i < _players.Count; i++)
        {
            myRaceOrder += _players[i].Name;
            if (i < _players.Count - 1)
            {
                myRaceOrder += '\n';
            }
        }

        RpcUpdateUIRaceProgress(myRaceOrder);
    }

    [ClientRpc]
    void RpcUpdateUIRaceProgress(string newRanking)
    {
        _uiManager.UpdateRanking(newRanking);
        //Debug.Log("El orden de carrera es: " + myRaceOrder);
    }

    [ClientRpc]
    void RpcChangeFromRoomToGameHUD()
    {
        _uiManager.ActivateInGameHUD();
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

        float minArcL = this._circuitController.ComputeClosestPointArcLength(carPos, out segIdx, out carProj, out carDist);

        //Esto no hace falta cuando quitemos las bolas
        this._debuggingSpheres[id].transform.position = carProj;

        _players[id].Segment = segIdx;
        minArcL -= _circuitController.CircuitLength * (laps - _players[id].CurrentLapSegments + 1);

        return minArcL;
    }

    [ClientRpc]
    void showCosa(float s)
    {
        Debug.Log(s);
    }
}
