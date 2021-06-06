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
    private PlayerInfo[] playersAux;
    public bool isTrainingRace = true;
    public bool openRoom = true;

    [Header("RaceProgress")]
    string myRaceOrder = "";
    //Boolean que indica si se esta corriendo
    [SyncVar] public bool racing = false;
    public bool raceStart = false;
    public bool hasStarted = false;


    #region Variables de Tiempo
    //Tiempo Total de la carrera
    [SyncVar] private float totalTime = 0;

    private int countdownTimer = 3;

    #endregion



    private void Awake()
    {
        if (_networkManager == null) _networkManager = FindObjectOfType<MyNetworkManager>();
        if (_circuitController == null) _circuitController = FindObjectOfType<CircuitController>();
        if (_uiManager == null) _uiManager = FindObjectOfType<UIManager>();
        if (playersAux == null) playersAux = FindObjectsOfType<PlayerInfo>();

        //ELIMINAMOS LA GENERACION DE ESFERAS INNECESARIAS

        _debuggingSpheres = new GameObject[_networkManager.maxConnections]; //deberia ser solo 1 la del jugador y se pasan todas al server para que calcule quien va primero
        for (int i = 0; i < _networkManager.maxConnections; ++i)
        {
            _debuggingSpheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _debuggingSpheres[i].GetComponent<SphereCollider>().enabled = false;
        }
    }

    private void Update()
    {
        if (isServer)
        {
            if (_players.Count == 0)
                return;

            if (racing)
            {

                if (_players.Count == 1 && !isTrainingRace)
                {
                    VictoryByDefault();
                }
                totalTime += Time.deltaTime;
                UpdateRaceProgress();
                if (CheckFinish())
                {
                    racing = false;
                    raceStart = false;
                    Finish();
                    ResetRace();
                }
            }
            else if (!hasStarted)
            {
                hasStarted = true;
                ResetRace();
            }
        }
    }

    [Server]
    public void StartRace()
    {
        numPlayers = _players.Count;
        raceStart = true;
        StopCoroutine(DecreaseCountdownCoroutine());
        StartCoroutine(DecreaseCountdownCoroutine());
        RpcUpdateCountdownUI(countdownTimer);

    }


    [Server]
    public void ResetRace()
    {
        countdownTimer = 3;
        Debug.Log(countdownTimer);
        for (int i = 0; i < _players.Count; ++i)
        {
            _players[i].Segment = 0;
            _players[i].CurrentLapSegments = 0;
            _players[i].CurrentLapCountingFromFinishLine = 1;

            _players[i].CurrentLapTime = 0;
            _players[i].TotalLapTime = 0;


            _players[i].LastCheckPoint = _circuitController.checkpoints.Count - 1;

            _players[i].controller.ResetToStart(startingPoints[i]);

        }

        RpcDecreaseCountDown();
    }

    public void ResetHUD()
    {
        RpcResetHUD();
    }

    private bool CheckFinish()
    {
        for (int i = 0; i < _players.Count; ++i)
        {
            if (_players[i].CurrentLapCountingFromFinishLine == laps + 1)
            {
                RpcShowWinner(_players[i].Name, totalTime);
                totalTime = 0;
                return true;
            }
        }
        return false;
    }

    [ClientRpc]
    private void RpcShowWinner(string name, float time)
    {
        _uiManager.UpdateWinner(name, time);
    }

    private void Finish()
    {
        Debug.Log("Fin");
        RpcChangeFromGameToEndHUD();
        //hasStarted = true;
    }

    private void VictoryByDefault()
    {
        racing = false;
        RpcChangeFromGameToEndHUD();
        RpcShowWinner(_players[0].Name, totalTime);
    }

    public void AddPlayer(PlayerInfo player)
    {
        //AQUI FALTA UN COMENTARIO
        player.MaxCheckPoints = _circuitController.checkpoints.Count;
        currentPlayers++;
        _players.Add(player);

        if (player.isServer)
        {
            player.isAdmin = true;
            player.transform.position = startingPoints[_players.Count - 1].position;
            player.transform.rotation = startingPoints[_players.Count - 1].rotation;

        }

        isTrainingRace = _players.Count < 2;

        //_uiManager.TrainingOrRacing(isTrainingRace);

    }

    public void RemovePlayer(PlayerInfo player)
    {
        int playerIndex = _players.IndexOf(player);
        if (playerIndex > -1)
        {
            _players.RemoveAt(playerIndex);
        }
    }

    [Server]
    void UpdateCountdownUI()
    {
        RpcUpdateCountdownUI(countdownTimer);
        if (countdownTimer == 0)
        {
            Debug.Log("Fin de la cuenta atras");
            racing = true;
        }
    }



    [ClientRpc]
    void RpcDecreaseCountDown()
    {
        StartCoroutine(DecreaseCountdownCoroutine());
    }

    [Client]
    IEnumerator DecreaseCountdownCoroutine()
    {
        while (countdownTimer > 0)
        {
            yield return new WaitForSeconds(2);
            countdownTimer--;
            Debug.Log(countdownTimer);
            UpdateCountdownUI();
        }
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
            _players[i].TotalLapTime += Time.deltaTime;
            _players[i].CurrentLapTime += Time.deltaTime;
            //_players[i].UpdateTime();
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
    void RpcUpdateCountdownUI(int seconds)
    {
        _uiManager.UpdateCountdown(seconds);
    }

    [ClientRpc]
    void RpcUpdateUIRaceProgress(string newRanking)
    {
        _uiManager.UpdateRanking(newRanking);
        //Debug.Log("El orden de carrera es: " + myRaceOrder);
    }

    [ClientRpc]
    void RpcResetHUD()
    {
        _uiManager.ActivateInGameHUD();
    }

    [ClientRpc]
    void RpcChangeFromGameToEndHUD()
    {
        _uiManager.ActivateEndRaceHud();
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
        minArcL -= _circuitController.CircuitLength * (laps - _players[id].CurrentLapSegments);

        return minArcL;
    }

}
