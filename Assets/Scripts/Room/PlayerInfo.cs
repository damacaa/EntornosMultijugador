using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEditor;
using UnityEngine;


/// <summary>
/// Infarmacion sobre un player
/// </summary>
public class PlayerInfo : NetworkBehaviour
{
    //referencias
    private UIManager _uiManager;
    public PlayerController controller;

    /// <summary>
    /// Nombre del jugador
    /// </summary>
    [SyncVar] public string Name;

    /// <summary>
    /// ID del jugador
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// Vueltas basdas en los segmentos en lso qeu se divide el circuito
    /// Esto se hace debido al desfase qeu hay entre el punto final qen el qeu se "acaba" el circuito y el lugar en el que se encuentra la meta
    /// </summary>
    public int CurrentLapSegments;

    /// <summary>
    /// Vueltas basdas la posicion fisica de la meta.
    /// Esto se hace debido al desfase qeu hay entre el punto final qen el qeu se "acaba" el circuito y el lugar en el que se encuentra la meta
    /// </summary>
    [SyncVar(hook = nameof(UpdateLapUI))] public int CurrentLapCountingFromFinishLine;

    //Ultimo Checkpoint por el qeu ha pasado el jugador
    public int LastCheckPoint;
    //Numero total de checkpoints
    public int MaxCheckPoints;

    /// <summary>
    /// Tiempo actual de la vuelta que esta realizando un jugador
    /// </summary>
    [SyncVar(hook = nameof(UpdateTime))] public float CurrentLapTime = 0;

    /// <summary>
    /// Tiempo total en acabar todas las vueltas del circuito
    /// </summary>
    [SyncVar] public float TotalLapTime = 0;

    /// <summary>
    /// Segmento del circuito en el que se encuentra actualmente el jugador
    /// </summary>
    private int segment;
    public int Segment
    {
        get { return segment; }
        set
        {
            if (value != segment)
            {
                int a = value - segment;
                if (a < -1 && LastCheckPoint == MaxCheckPoints - 1)
                {
                    LastCheckPoint = 0;
                    CurrentLapSegments++;
                }
                else if (a > 1)
                {
                    LastCheckPoint = MaxCheckPoints - 1;
                    CurrentLapSegments--;
                }
                segment = value;

            }
        }
    }

    [SyncVar(hook = nameof(isHost))] public bool isAdmin = false;

    //Referencias
    private void Awake()
    {
        controller = gameObject.GetComponent<PlayerController>();
        if (_uiManager == null) _uiManager = FindObjectOfType<UIManager>();
    }

    //Al detectar un trigger
    private void OnTriggerEnter(Collider collision)
    {   
        //Si es un checkpoint y es el siguiente al ultimo que pasó esta recorriendo el circuito correctamente
        if (collision.gameObject.tag == "CheckPoint")
        {
            int id = collision.gameObject.GetComponent<Checkpoint>().id;
            if (id - LastCheckPoint == 1) { LastCheckPoint = id; }
        }

        //Si es la meta y ha pasado por todos los checkpoints anteriores se añade una vuelta
        if (collision.gameObject.tag == "Finish")
        {
            //Meta
            if (LastCheckPoint == MaxCheckPoints - 1 && CurrentLapSegments >= 0)
            {
                if (CurrentLapCountingFromFinishLine <= CurrentLapSegments + 1)
                {
                    CurrentLapTime = 0;
                    CurrentLapCountingFromFinishLine = CurrentLapSegments + 2;
                }
            }
            else if (CurrentLapSegments < 0)
            {
                CurrentLapCountingFromFinishLine = 1;
            }
        }
    }

    //Gizmos
    private void OnDrawGizmos()
    {
        Handles.Label(transform.position + transform.right, controller.DistToFinish.ToString());

        Handles.Label(transform.position - transform.right + Vector3.up, CurrentLapSegments.ToString());
        Handles.Label(transform.position - transform.right + 2 * Vector3.up, CurrentLapCountingFromFinishLine.ToString());

        Handles.Label(transform.position + transform.right + Vector3.up, segment.ToString());
        Handles.Label(transform.position + transform.right + 2 * Vector3.up, LastCheckPoint.ToString());
    }

    [Client]
    void isHost(bool oldvalue, bool newvalue)
    {
        if (newvalue && isLocalPlayer)
        {
            Debug.Log("onHostAuth");

            _uiManager.setEndRaceHUDButtons(this);
            //_uiManager.ActivateRoomHUD();
        }
    }


    /// <summary>
    /// Hook que se llama cada vez que CurrentLapCountingFromFinishLine cambia de valor y muetra la nueva vuelta por pantalla.
    /// </summary>
    /// <param name="oldValue">Vuelta que acaba de terminar</param>
    /// <param name="newValue">Vuelta que caba de empezar</param>
    public void UpdateLapUI(int oldValue, int newValue)
    {
        _uiManager.UpdateLap( newValue);
    }

    /// <summary>
    /// Hook que se llama cada vez que CurrentLapTime cambia de valor y muetra lel tiempode esta vuelta por pantalla.
    /// </summary>
    /// <param name="oldValue">Valor anterior</param>
    /// <param name="newValue">Valor Nuevo</param>
    public void UpdateTime(float old, float time)
    {
        if (isLocalPlayer)
        {
            _uiManager.UpdateTime("TIME: " +
                Math.Truncate(CurrentLapTime / 60) + ":" + Math.Round(CurrentLapTime % 60, 2)
            + "/" + Math.Truncate(TotalLapTime / 60) + ":" + Math.Round(TotalLapTime % 60, 2));
        }
    }
}
