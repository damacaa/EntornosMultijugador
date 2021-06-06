using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Manager qeu se encarga de gestionar el HUD mientras se compite en la carrera y al acabar la misma
/// </summary>
public class UIManager : NetworkBehaviour
{
    public bool showGUI = true;

    private MyNetworkManager m_NetworkManager;
    [SerializeField] private PolePositionManager _polePositionManager;

    [Header("In-Game HUD")]
    [SerializeField]
    private GameObject inGameHUD;

    [SerializeField] private Text textSpeed;
    [SerializeField] private Text textLaps;
    [SerializeField] private Text textPosition;
    [SerializeField] private Text textTime;
    [SerializeField] private Text crashedWarning;
    [SerializeField] private Text backwardWarning;

    [Header("End Race HUD")]
    [SerializeField] private GameObject endRaceHUD;
    [SerializeField] private Text playerNameWinner;
    [SerializeField] private Text winnerTime;
    [SerializeField] private Text countdown;
    [SerializeField] private Button rematchButton;
    [SerializeField] private Button exitButton;

    //Vueltas del circuito
    private int circuitLaps;

    //Referencias
    private void Awake()
    {
        m_NetworkManager = FindObjectOfType<MyNetworkManager>();
        if (!_polePositionManager) _polePositionManager = FindObjectOfType<PolePositionManager>();
        circuitLaps = _polePositionManager.laps;
    }

    /// <summary>
    /// FUncionalidad del boton Rematch, llama para que se resetee la carrera y muestra el su HUD 
    /// </summary>
    public void ButtonRematch()
    {
        _polePositionManager.ResetHUD();
        _polePositionManager.ResetRace();
        countdown.gameObject.SetActive(true);
    }

    /// <summary>
    /// Activa el InGameHUD en el cliente
    /// </summary>
    public void ActivateInGameHUD()
    {
        endRaceHUD.SetActive(false);
        inGameHUD.SetActive(true);
    }


    /// <summary>
    /// Activa el EndGameHUD en el cliente
    /// </summary>
    public void ActivateEndRaceHud()
    {
        inGameHUD.SetActive(false);
        endRaceHUD.SetActive(true);
    }

    /// <summary>
    /// Actualiza el HUD con la Velocidad
    /// </summary>
    /// <param name="speed">Velocidad del cliente</param>
    public void UpdateSpeed(float speed)
    {
        textSpeed.text = "Speed " + (int)(speed * 5f) + " Km/h";
    }


    /// <summary>
    /// Actualiza el HUD con la la vuelta actual solo si 
    /// </summary>
    /// <param name="lap">Vuelta actual del cliente</param>
    public void UpdateLap(int lap)
    {

        if (lap >= 0)
        {
            textLaps.text = "Lap " + lap + "/" + circuitLaps;
        }

    }

    /// <summary>
    /// Actualiza el HUD con el tiempo de la vuelta actual y total
    /// </summary>
    /// <param name="time">Velocidad del cliente</param>
    public void UpdateTime(string time)
    {
        textTime.text = time;
    }

    /// <summary>
    /// Actualiza el HUD con orden actual de los participantes de la carrera
    /// </summary>
    /// <param name="ranking">String con los jugadores en orden de la </param>
    public void UpdateRanking(string ranking)
    {
        textPosition.text = ranking;
    }

    /// <summary>
    /// Actualiza la cuenta atras de la salida
    /// </summary>
    /// <param name="countDownSeconds">Segundos restantes hasta el final de la cuenta atras</param>
    [Client]
    public void UpdateCountdown(int countDownSeconds)
    {
        if (countdown != null && countdown.gameObject.activeSelf)
        {
            if (countDownSeconds == 0)
            {
                countdown.text = "START!";
                StartCoroutine(HideCountdown());
            }
            else
            {
                countdown.text = countDownSeconds.ToString();
            }
        }
    }

    /// <summary>
    /// Actualiza el HUD de la carrera cuando esta acaba con el jugador que ha ganado
    /// </summary>
    /// <param name="name">Nombre del ganador</param>
    /// <param name="time">Tiempo total en acbar el circuito</param>
    public void UpdateWinner(string name, float time)
    {
        playerNameWinner.text = "WINNER: " + name;
        winnerTime.text = "TIME: " +
                Math.Truncate(time / 60) + ":" + Math.Round(time % 60, 2);
    }

    /// <summary>
    /// Corrutina que esconde el GO de la cuenta atras
    /// </summary>
    IEnumerator HideCountdown()
    {
        yield return new WaitForSeconds(1f);
        countdown.gameObject.SetActive(false);
    }

    /// <summary>
    /// Observador que reacciona al evento de chocarse y muestra un cartel avisando
    /// </summary>
    /// <param name="hasCrashed">Si el jugador se ha chocado</param>
    public void ShowCrashedWarning(bool hasCrashed)
    {
        crashedWarning.transform.parent.gameObject.SetActive(hasCrashed);
    }

    /// <summary>
    /// Observador que reacciona al evento ir marcha atras y muestra un cartel avisando
    /// </summary>
    /// <param name="goingBackwards"></param>
    public void ShowBackwardsWarning(bool goingBackwards)
    {
        backwardWarning.transform.parent.gameObject.SetActive(goingBackwards);
    }

    /// <summary>
    /// Añade la funcionalidad a los botones del EndGameHUD dependeiendo si el jugador es Host o no
    /// </summary>
    /// <param name="p">Cliente al que se le añadiran la funcionalidad a los botones</param>
    public void setEndRaceHUDButtons(PlayerInfo p)
    {
        if (p.isLocalPlayer)
        {
            rematchButton.onClick.AddListener(() => ButtonRematch());
            exitButton.onClick.AddListener(() => m_NetworkManager.StopHost());
        }
        else
        {
            rematchButton.gameObject.SetActive(false);
            exitButton.onClick.AddListener(() => m_NetworkManager.StopClient());
        }
    }


}