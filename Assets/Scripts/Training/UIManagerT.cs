using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManagerT : MonoBehaviour
{
    public bool showGUI = true;

    [SerializeField] private PolePositionManagerT _polePositionManager;

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


    private int circuitLaps;

    private void Awake()
    {
        circuitLaps = FindObjectOfType<CircuitControllerT>().circuitLaps;
    }


    public void ButtonRematch()
    {
        _polePositionManager.ResetHUD();
        _polePositionManager.ResetRace();
        _polePositionManager.StartRace();
    }

    public void ActivateInGameHUD()
    {
        endRaceHUD.SetActive(false);
        inGameHUD.SetActive(true);
    }

    public void ActivateEndRaceHud()
    {
        inGameHUD.SetActive(false);
        endRaceHUD.SetActive(true);
    }

    public void UpdateSpeed(float speed)
    {
        textSpeed.text = "Speed " + (int)(speed * 5f) + " Km/h";
    }

    public void UpdateLap(PlayerInfoT player, int lap)
    {
  
            if (lap >= 0)
            {
                textLaps.text = "Lap " + lap + "/" + circuitLaps;
            }
        
    }

    public void UpdateTime(string time)
    {
        textTime.text = time;
    }

    public void UpdateRanking(string ranking)
    {
        textPosition.text = ranking;
    }

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
                Debug.Log(countDownSeconds);
            }
        }
    }
    IEnumerator HideCountdown()
    {
        yield return new WaitForSeconds(1f);
        countdown.gameObject.SetActive(false);
    }

    public void ShowCrashedWarning(bool hasCrashed)
    {
        crashedWarning.transform.parent.gameObject.SetActive(hasCrashed);
    }

    public void ShowBackwardsWarning(bool goingBackwards)
    {
        backwardWarning.transform.parent.gameObject.SetActive(goingBackwards);
    }

    public string GetPlayerName()
    {
        return FindObjectOfType<InputField>().text;
    }

    public string GetCarColor()
    {
        return "Rojo";// carColor.text;
    }

    //gets car's color selected from client UI
    public int GetCarSelected()
    {
        int car = 0;
        var color = GetCarColor();
        if (color == "Verde")
        {
            car = 1;
        }
        else if (color == "Amarillo")
        {
            car = 2;
        }
        else if (color == "Blanco")
        {
            car = 3;
        }
        return car;
    }

    public void setEndRaceHUDButtons(PlayerInfo localPlayer)
    {
        if (localPlayer.isServer)
        {
            rematchButton.onClick.AddListener(() => ButtonRematch());
        }
        else
        {
            rematchButton.gameObject.SetActive(false);
        }
    }

    public void UpdateWinner(string name, float time)
    {
        playerNameWinner.text = "WINNER: " + name;
        winnerTime.text = "TIME: " +
                Math.Truncate(time / 60) + ":" + Math.Round(time % 60, 2);
    }
}