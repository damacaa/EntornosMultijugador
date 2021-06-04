﻿using System;
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
    private Transform[] startingPoints;

    [SyncVar] public int numPlayers = 4;
    [SyncVar] public int laps = 3;
    int currentPlayers;

    private readonly List<PlayerInfo> _players = new List<PlayerInfo>();
    private readonly List<PlayerInfo> _order = new List<PlayerInfo>();

    public bool isPractice = true;

    [Header("RaceProgress")]
    [SyncVar] string myRaceOrder = "";
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

    public delegate void OnRankingChangeDelegate(string newVal);
    public event OnRankingChangeDelegate OnRankingChangeEvent;


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

        NetworkStartPosition[] sp = GameObject.FindObjectsOfType<NetworkStartPosition>();
        startingPoints = new Transform[sp.Length];
        for (int i = 0; i < sp.Length; i++)
        {
            startingPoints[i] = sp[i].gameObject.transform;
        }

        racing = false;
    }


    public bool waiting = true;
    private void Update()
    {
        if (_players.Count == 0)
            return;

        if (racing)
        {
            UpdateRaceProgress();
            if (isServer)
            {
                if (CheckFinish())
                {
                    racing = false;
                    Finish();
                    ResetPlayers();
                }

                totalTime += Time.deltaTime;
            }
        }
        else if (waiting)
        {
            waiting = false;
            ResetPlayers();
        }
    }

    private bool CheckFinish()
    {
        for (int i = 0; i < _players.Count; ++i)
        {
            if (_players[i].CurrentLap == laps + 1)
            {
                Debug.Log("Vencedor: " + _players[i].name);
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
            _players[i].CurrentLap = 0;
            _players[i].LastCheckPoint = 0;
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
        currentPlayers++;
        player.MaxCheckPoints = _circuitController.checkpoints.Count;
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
            if (_arcLengths[x.ID] < _arcLengths[y.ID])
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

            _players[i].controller.DistToFinish = l;
            arcLengths[i] = l;
        }

        _players.Sort(new PlayerInfoComparer(arcLengths));

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

        /*if (carDist > 15)
        {
            this._players[id].transform.position = carProj;
            minArcL = this._circuitController.ComputeClosestPointArcLength(carPos, out segIdx, out carProj, out carDist);
        }*/

        this._players[id].Segment = segIdx;

        //Esto no hace falta cuando quitemos las bolas
        this._debuggingSpheres[id].transform.position = carProj;

        if (_players[id].CurrentLap == 0)
        {
            minArcL -= _circuitController.CircuitLength * (laps + 1);
        }
        else
        {
            minArcL -= _circuitController.CircuitLength * (laps - _players[id].CurrentLap + 1);
        }
        //minArcL -= _circuitController.CircuitLength * (laps - _players[id].CurrentLap);
        //Debug.Log(_circuitController.CircuitLength * (_players[id].CurrentLap- laps));



        return minArcL;
    }
}
