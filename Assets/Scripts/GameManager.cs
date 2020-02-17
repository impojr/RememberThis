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

    public PlayableDirector wave1Arrows;
    public PlayableDirector wave2Arrows;
    public PlayableDirector wave3Arrows;
    public PlayableDirector wave4Arrows;

    private bool recieveInput = false;
    private int numberOfButtonsToPress = 0;
    private List<KeyCode> buttonsPressed;
    private int currentWave = 1;

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

        while (time > 0)
        {
            yield return new WaitForSeconds(0.01f); // change to variable
            time -= 0.01f;
            timerImage.fillAmount = time / seconds;
        }
        recieveInput = false;
        HidePressedIndicators();
        chalkboardText.gameObject.SetActive(false);
        timerImage.gameObject.SetActive(false);
        Debug.Log(buttonsPressed.Count);

        for (int i = 0; i < numberOfButtonsToPress; i++)
        {
            Debug.Log("Pressed: " + (arrows[i].arrowKey == buttonsPressed[i]));
        }

        //end timeline start
    }

    public void ShowArrows()
    {
        if (currentWave == 1)
        {
            wave1Arrows.Play();
        } else if (currentWave == 2)
        {
            wave2Arrows.Play();
        } else if (currentWave == 3)
        {
            wave3Arrows.Play();
        } else if (currentWave == 4)
        {
            wave4Arrows.Play();
        }

        currentWave++;
    }
}
