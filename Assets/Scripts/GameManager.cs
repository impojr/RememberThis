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

    public Animator playerAnimator;
    public Animator teacherAnimator;

    public AudioSource correctMoveAudioSource;
    public AudioSource resultAudioSource;

    public AudioClip loseSound;
    public AudioClip winSound;

    public Text resultText;
    public GameObject resultScreen;

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

        teacherAnimator.SetTrigger(Constants.TEACHER_THINKING_ANIM);
        playerAnimator.SetTrigger(Constants.PLAYER_THINKING_ANIM);

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

        while (time > 0 && buttonsPressed.Count < numberOfButtonsToPress)
        {
            yield return new WaitForSeconds(timeIntervalForTimerFillAmount); 
            time -= timeIntervalForTimerFillAmount;
            timerImage.fillAmount = time / seconds;
        }

        if (buttonsPressed.Count < numberOfButtonsToPress)
        {
            yield return new WaitForSeconds(2f);
        }

        playerAnimator.SetTrigger(Constants.PLAYER_IDLE_ANIM);
        teacherAnimator.SetTrigger(Constants.TEACHER_IDLE_ANIM);
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

    public void HideArrows()
    {
        foreach(Arrow arrow in arrows)
        {
            arrow.gameObject.SetActive(false);
        }
    }

    public void TriggerMovementAnimation()
    {
        TriggerPlayerMovementAnimation();

        arrows[movementNumber].gameObject.SetActive(true);

        if (movementNumber >= buttonsPressed.Count || (arrows[movementNumber].arrowKey != buttonsPressed[movementNumber]))
        {
            ReduceHealth();
        } else
        {
            correctMoveAudioSource.Play();
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
        if (health <= 0)
        {
            TriggerResultScreen("YOU LOSE!", Constants.PLAYER_LOSE_ANIM, Constants.TEACHER_LOSE_ANIM, loseSound);
        } else if (currentWave > outroTimelines.Length)
        {
            TriggerResultScreen("YOU WIN!", Constants.PLAYER_WIN_ANIM, Constants.TEACHER_WIN_ANIM, winSound);
        } else
        {
            playerAnimator.SetTrigger(Constants.PLAYER_IDLE_ANIM);
            movementNumber = 0;
            introTimeline.Play();
        }
    }

    public void TriggerResultScreen(string resultString, string playerAnimation, string teacherAnimation, AudioClip sound)
    {
        resultText.text = resultString;
        resultScreen.SetActive(true);
        playerAnimator.SetTrigger(playerAnimation);
        teacherAnimator.SetTrigger(teacherAnimation);
        resultAudioSource.PlayOneShot(sound);
    }

    public void TriggerPlayerMovementAnimation()
    {
        if (movementNumber >= buttonsPressed.Count)
        {
            playerAnimator.SetTrigger(Constants.PLAYER_THINKING_ANIM);
        } else if (buttonsPressed[movementNumber] == KeyCode.UpArrow)
        {
            playerAnimator.SetTrigger(Constants.PLAYER_UP_ANIM);
        } else if (buttonsPressed[movementNumber] == KeyCode.DownArrow)
        {
            playerAnimator.SetTrigger(Constants.PLAYER_DOWN_ANIM);
        } else if (buttonsPressed[movementNumber] == KeyCode.RightArrow)
        {
            playerAnimator.SetTrigger(Constants.PLAYER_RIGHT_ANIM);
        } else if (buttonsPressed[movementNumber] == KeyCode.LeftArrow)
        {
            playerAnimator.SetTrigger(Constants.PLAYER_LEFT_ANIM);
        }
    }
}
