using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PolePositionManagerT : MonoBehaviour
{
    private UIManagerT _uiManager;

    private CircuitControllerT _circuitController;
    private GameObject[] _debuggingSpheres;

    [Header("RaceStartingPositions")]
    public Transform[] startingPoints;

    public int numPlayers = 1;
    public int laps = 3;
    int currentPlayers;

    private readonly List<PlayerInfoT> _players = new List<PlayerInfoT>();
    private PlayerInfoT[] playersAux;
    public bool isTrainingRace = true;
    public bool openRoom = true;

    [Header("RaceProgress")]
    string myRaceOrder = "";
    //Boolean que indica si se esta corriendo
    public bool racing = false;
    public bool raceStart = false;


    #region Variables de Tiempo
    //Tiempo Total de la carrera
    private float totalTime = 0;

    private int countdownTimer = 3;

    #endregion



    private void Awake()
    {
        if (_circuitController == null) _circuitController = FindObjectOfType<CircuitControllerT>();
        if (_uiManager == null) _uiManager = FindObjectOfType<UIManagerT>();
        if (playersAux == null) playersAux = FindObjectsOfType<PlayerInfoT>();
    }

    private void Update()
    {
        if (racing)
        {
            totalTime += Time.deltaTime;
            UpdateRaceProgress();
        }
        else if (!raceStart)
        {
            raceStart = true;
            ResetRace();
        }

    }

    public void StartRace()
    {
        numPlayers = _players.Count;
        raceStart = true;
        StopCoroutine(DecreaseCountdownCoroutine());
        StartCoroutine(DecreaseCountdownCoroutine());
        RpcUpdateCountdownUI(countdownTimer);

    }


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

    private void RpcShowWinner(string name, float time)
    {
        _uiManager.UpdateWinner(name, time);
    }

    private void Finish()
    {
        racing = false;
        Debug.Log("Fin");
        RpcChangeFromGameToEndHUD();
        SceneManager.LoadScene(0);
        //hasStarted = true;
    }

    private void VictoryByDefault()
    {
        racing = false;
        RpcChangeFromGameToEndHUD();
        RpcShowWinner(_players[0].Name, totalTime);
    }

    public void AddPlayer(PlayerInfoT player)
    {

        player.MaxCheckPoints = _circuitController.checkpoints.Count;
        currentPlayers++;
        _players.Add(player);

        player.transform.position = startingPoints[0].position;
        player.transform.rotation = startingPoints[0].rotation;

        isTrainingRace = _players.Count < 2;
    }

    public void RemovePlayer(PlayerInfoT player)
    {
        int playerIndex = _players.IndexOf(player);
        if (playerIndex > -1)
        {
            _players.RemoveAt(playerIndex);
        }
    }

    void UpdateCountdownUI()
    {
        RpcUpdateCountdownUI(countdownTimer);
        if (countdownTimer == 0)
        {
            Debug.Log("Fin de la cuenta atras");
            racing = true;
        }
    }

    void RpcDecreaseCountDown()
    {
        StartCoroutine(DecreaseCountdownCoroutine());
    }

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

    private class PlayerInfoComparerT : Comparer<PlayerInfoT>
    {
        public override int Compare(PlayerInfoT x, PlayerInfoT y)
        {
            if (x.controller.DistToFinish < y.controller.DistToFinish)
                return 1;
            else return -1;
        }
    }

    public void UpdateRaceProgress()
    {
        // Update car arc-lengths
        _players[0].controller.DistToFinish = ComputeCarArcLength(0); //Distancia restante hasta la meta
        _players[0].CurrentLapTime += Time.deltaTime;
    }

    void RpcUpdateCountdownUI(int seconds)
    {
        _uiManager.UpdateCountdown(seconds);
    }

    void RpcUpdateUIRaceProgress(string newRanking)
    {
        _uiManager.UpdateRanking(newRanking);
        //Debug.Log("El orden de carrera es: " + myRaceOrder);
    }

    void RpcResetHUD()
    {
        _uiManager.ActivateInGameHUD();
    }

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

        _players[id].Segment = segIdx;
        minArcL -= _circuitController.CircuitLength * (laps - _players[id].CurrentLapSegments);

        return minArcL;
    }

    void OnGUI()
    {
        if (GUILayout.Button("Menu"))
        {
            Finish();
        }
    }

}
