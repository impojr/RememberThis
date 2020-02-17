using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public Text chalkboardText;
    public Image timerImage;

    public Sprite upArrowSprite;
    public Sprite downArrowSprite;
    public Sprite leftArrowSprite;
    public Sprite rightArrowSprite;

    public Arrow[] arrows;
    public Image[] pressedIndicators;

    public Color unpressedColor;
    public Color pressedColor;

    public PlayableDirector introTimeline;
    public PlayableDirector[] waveTimelines;
    public PlayableDirector[] outroTimelines;

    public Image[] healthUIs;

    private bool recieveInput = false;
    private int numberOfButtonsToPress = 0;
    private List<KeyCode> buttonsPressed;
    private int currentWave = 1;
    private int health = 5;
    private int movementNumber = 0;

    void Start()
    {
        SetArrowsToPress();
    }

    public void SetArrowsToPress()
    {
        Tuple<KeyCode, Sprite>[] keys =
        {
            Tuple.Create(KeyCode.UpArrow, upArrowSprite),
            Tuple.Create(KeyCode.DownArrow, downArrowSprite),
            Tuple.Create(KeyCode.LeftArrow, leftArrowSprite),
            Tuple.Create(KeyCode.RightArrow, rightArrowSprite)
        };

        foreach (Arrow arrow in arrows)
        {
            arrow.SetArrowKeyAndUpdateSprite(keys[Random.Range(0, keys.Length)]);
        }
    }

    void Update()
    {
        if (!recieveInput)
            return;

        if (buttonsPressed.Count >= numberOfButtonsToPress)
            return;

        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            buttonsPressed.Add(KeyCode.LeftArrow);
            ChangeColorOfPressedIndicators(buttonsPressed.Count);
        } else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            buttonsPressed.Add(KeyCode.RightArrow);
            ChangeColorOfPressedIndicators(buttonsPressed.Count);
        } else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            buttonsPressed.Add(KeyCode.UpArrow);
            ChangeColorOfPressedIndicators(buttonsPressed.Count);
        } else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            buttonsPressed.Add(KeyCode.DownArrow);
            ChangeColorOfPressedIndicators(buttonsPressed.Count);
        }
    }

    public void RecievePlayerInput(int buttons)
    {
        if (buttons < 0 || buttons > arrows.Length)
            throw new ArgumentOutOfRangeException(nameof(buttons) + " must be within length of " + nameof(arrows));

        numberOfButtonsToPress = buttons;
        buttonsPressed = new List<KeyCode>();
        chalkboardText.gameObject.SetActive(true);
        timerImage.gameObject.SetActive(true);

        ShowPressedIndicators(buttons);

        StartCoroutine(StartTimer(10f));
    }

    private void ShowPressedIndicators(int buttonsToPress)
    {
        for (int i = 0; i < buttonsToPress; i++)
        {
            pressedIndicators[i].color = unpressedColor;
            pressedIndicators[i].gameObject.SetActive(true);
        }
    }

    private void HidePressedIndicators()
    {
        for (int i = 0; i < pressedIndicators.Length; i++)
        {
            pressedIndicators[i].color = unpressedColor;
            pressedIndicators[i].gameObject.SetActive(false);
        }
    }

    private void ChangeColorOfPressedIndicators(int buttonsPressed)
    {
        pressedIndicators[buttonsPressed - 1].color = pressedColor;
    }

    IEnumerator StartTimer(float seconds)
    {
        recieveInput = true;
        float time = seconds;
        float timeIntervalForTimerFillAmount = 0.01f;

        while (time > 0)
        {
            yield return new WaitForSeconds(timeIntervalForTimerFillAmount); 
            time -= timeIntervalForTimerFillAmount;
            timerImage.fillAmount = time / seconds;
        }
        recieveInput = false;
        HidePressedIndicators();
        chalkboardText.gameObject.SetActive(false);
        timerImage.gameObject.SetActive(false);
        ShowOutroTimeline();
    }

    public void ShowArrows()
    {
        waveTimelines[currentWave - 1].Play();
    }

    public void ShowOutroTimeline()
    {
        outroTimelines[currentWave - 1].Play();
        currentWave++;
    }

    public void TriggerMovementAnimation()
    {
        //todo trigger animation

        //todo make sure you cater for no button presses

        if (arrows[movementNumber].arrowKey != buttonsPressed[movementNumber])
        {
            ReduceHealth();
        }

        movementNumber++;
    }

    private void ReduceHealth()
    {
        if (health <= 0)
            return;

        health--;
        healthUIs[health].color = new Color(255,255,255,0);
    }

    public void ResetMovementNumberAndStartNextWave()
    {
        //if (health <= 0)
        //{
            //todo lose screen
        /*} else */ if (currentWave > 4)
        {
            //todo win screen
        } else
        {
            movementNumber = 0;
            introTimeline.Play();
        }
    }
}
