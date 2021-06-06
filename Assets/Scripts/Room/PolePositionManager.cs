using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Mirror;
using UnityEngine;

/// <summary>
/// Manager que gestiona el juego
/// </summary>
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
    
    //Es vuelta de entrenamiento
    public bool isTrainingRace = true;

    [Header("RaceProgress")]
    //String de ordenacion 
    string myRaceOrder = "";
    //Boolean que indica si se esta corriendo
    [SyncVar] public bool racing = false;
    //Boolean que indica si puede empezar al carrera
    public bool raceStart = false;
    //Boolean que indica si ha empezado la carrera
    public bool hasStarted = false;


    #region Variables de Tiempo
    //Tiempo Total de la carrera
    [SyncVar] private float totalTime = 0;

    //Segundos de cuenta atras
    [SyncVar] private int countdownTimer = 3;

    #endregion


    //referencias
    private void Awake()
    {
        if (_networkManager == null) _networkManager = FindObjectOfType<MyNetworkManager>();
        if (_circuitController == null) _circuitController = FindObjectOfType<CircuitController>();
        if (_uiManager == null) _uiManager = FindObjectOfType<UIManager>();
 
        //ESFERAS DE DEBUG
        /*_debuggingSpheres = new GameObject[_networkManager.maxConnections]; //deberia ser solo 1 la del jugador y se pasan todas al server para que calcule quien va primero
        for (int i = 0; i < _networkManager.maxConnections; ++i)
        {
            _debuggingSpheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _debuggingSpheres[i].GetComponent<SphereCollider>().enabled = false;
        }*/
    }
    /// <summary>
    /// En el servidor se calcula el orden y resetean las posiciones al empezar y acabar la carrera
    /// </summary>
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

    /// <summary>
    /// Inicia el cominzo de la carrera
    /// </summary>
    [Server]
    public void StartRace()
    {
        numPlayers = _players.Count;
        raceStart = true;
        StopCoroutine(DecreaseCountdownCoroutine());
        //StartCoroutine(DecreaseCountdownCoroutine());
        RpcUpdateCountdownUI(countdownTimer);
    }

    /// <summary>
    /// Resetea la carreara a sus valores iniciales
    /// </summary>
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

    /// <summary>
    /// Resetea el HUD
    /// </summary>
    public void ResetHUD()
    {
        RpcResetHUD();
    }

    /// <summary>
    /// Comprueba si ha acbado la carrera
    /// </summary>
    /// <returns>Devuelve si ha acabado la carrera</returns>
    private bool CheckFinish()
    {
        for (int i = 0; i < _players.Count; ++i)
        {
            if (_players[i].CurrentLapCountingFromFinishLine == laps + 1)
            {
                //El server manda a los clientes mostrar el EndGameHUD
                RpcShowWinner(_players[i].Name, totalTime);
                totalTime = 0;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// El server manda un mensaje a los clientes para que actualicen al EndGameHUD
    /// </summary>
    /// <param name="name">Nombre del jugador ganador</param>
    /// <param name="time">Tiempo en acabar el circuito</param>
    [ClientRpc]
    private void RpcShowWinner(string name, float time)
    {
        _uiManager.UpdateWinner(name, time);
    }

    /// <summary>
    /// Inicia el final de la carrera
    /// </summary>
    private void Finish()
    {
        Debug.Log("Fin");
        RpcChangeFromGameToEndHUD();
        //hasStarted = true;
    }

    /// <summary>
    /// Si un jugador se queda solo gana la carrera
    /// </summary>
    private void VictoryByDefault()
    {
        racing = false;
        RpcChangeFromGameToEndHUD();
        RpcShowWinner(_players[0].Name, totalTime);
    }

    /// <summary>
    /// Añade un jugador a la lista de jugadores
    /// </summary>
    /// <param name="player">Jugador añadido a la lista</param>
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

        //isTrainingRace = _players.Count < 2;

    }

    /// <summary>
    /// Borra a un jugador de la lista de jugadores
    /// </summary>
    /// <param name="player">Jugador eliminado de la lista</param>
    public void RemovePlayer(PlayerInfo player)
    {
        int playerIndex = _players.IndexOf(player);
        if (playerIndex > -1)
        {
            _players.RemoveAt(playerIndex);
        }
    }

    /// <summary>
    /// El servidor actualiza los segundo de cuenta atras hasta el inicio de carrera
    /// </summary>
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


    /// <summary>
    /// El servidor manda a los clientes decrementar su interfaz de cuentaatras
    /// </summary>
    [ClientRpc]
    void RpcDecreaseCountDown()
    {
        StartCoroutine(DecreaseCountdownCoroutine());
    }

    /// <summary>
    /// Corrutina que actualiza la interfaz de cuenta atras de un cliente
    /// </summary>
    /// <returns></returns>
    [Client]
    IEnumerator DecreaseCountdownCoroutine()
    {
        while (countdownTimer > 1)
        {
            yield return new WaitForSeconds(1f);
            countdownTimer--;
            Debug.Log(countdownTimer);
            UpdateCountdownUI();
        }
        yield return null;
    }

    /// <summary>
    /// Comparador de Distancias a la meta entre jugadores
    /// </summary>
    private class PlayerInfoComparer : Comparer<PlayerInfo>
    {
        public override int Compare(PlayerInfo x, PlayerInfo y)
        {
            if (x.controller.DistToFinish < y.controller.DistToFinish)
                return 1;
            else return -1;
        }
    }

    /// <summary>
    /// El servidor calcula el orden de carrera
    /// </summary>
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

    /// <summary>
    /// Mensaje del servidor a los clientes para actualizar su interfaz de ceutna atras
    /// </summary>
    /// <param name="seconds">segundos restantes hasta el final de la cuetna atras</param>
    [ClientRpc]
    void RpcUpdateCountdownUI(int seconds)
    {
        _uiManager.UpdateCountdown(seconds);
    }

    /// <summary>
    /// Mensaje del servidor a los clientes para actualizar el ranking
    /// </summary>
    /// <param name="newRanking">Nuevo orden de carrera</param>
    [ClientRpc]
    void RpcUpdateUIRaceProgress(string newRanking)
    {
        _uiManager.UpdateRanking(newRanking);
        //Debug.Log("El orden de carrera es: " + myRaceOrder);
    }

    /// <summary>
    /// Mensaje de Servidor a los clientes para activar el HUD de carrera
    /// </summary>
    [ClientRpc]
    void RpcResetHUD()
    {
        _uiManager.ActivateInGameHUD();
    }


    /// <summary>
    /// Mensaje de Servidor a los clientes para activar el HUD de final de carrera
    /// </summary>
    [ClientRpc]
    void RpcChangeFromGameToEndHUD()
    {
        _uiManager.ActivateEndRaceHud();
    }

    /// <summary>
    /// Calcula la distancia hasta la meta
    /// </summary>
    /// <param name="id">id del player desde qeu se calcula la distancia.</param>
    /// <returns> distancia hasta la meta</returns>
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
        //this._debuggingSpheres[id].transform.position = carProj;

        _players[id].Segment = segIdx;
        minArcL -= _circuitController.CircuitLength * (laps - _players[id].CurrentLapSegments);

        return minArcL;
    }

}
