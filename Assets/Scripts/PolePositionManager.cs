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

    public bool isPractice = true;

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

        NetworkStartPosition[] sp = GameObject.FindObjectsOfType<NetworkStartPosition>();
        startingPoints = new Transform[sp.Length];
        for (int i = 0; i < sp.Length; i++)
        {
            startingPoints[i] = sp[i].gameObject.transform;
        }

        racing = true;
    }

    private void Start()
    {
        if (isServer)
        {
            racing = false;
            StartCoroutine(DelayStart(3f));
        }
    }

    private void Update()
    {
        if (_players.Count == laps)
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
    }

    private bool CheckFinish()
    {
        for (int i = 0; i < _players.Count; ++i)
        {
            if (_players[i].CurrentLap == laps)
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

        if (isServer)
        {
            player.transform.position = startingPoints[_players.Count - 1].position;
            player.transform.rotation = startingPoints[_players.Count - 1].rotation;
            _uiManager.AddPlayerToRoom(player, _players.Count - 1);
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

        //Esto no hace falta cuando quitemos las bolas
        this._debuggingSpheres[id].transform.position = carProj;
  

        minArcL += _circuitController.CircuitLength *(_players[id].CurrentLap);


        return minArcL;
    }

    [ClientRpc]
    void showCosa(float s)
    {
        Debug.Log(s);
    }
}
