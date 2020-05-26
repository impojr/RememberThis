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

    private static string PLAYER_IDLE_ANIM = "Idle";
    private static string PLAYER_THINKING_ANIM = "Thinking";
    private static string PLAYER_UP_ANIM = "Up";
    private static string PLAYER_DOWN_ANIM = "Down";
    private static string PLAYER_LEFT_ANIM = "Left";
    private static string PLAYER_RIGHT_ANIM = "Right";
    private static string PLAYER_WIN_ANIM = "Win";
    private static string PLAYER_LOSE_ANIM = "Lose";

    private static string TEACHER_IDLE_ANIM = "Idle";
    private static string TEACHER_THINKING_ANIM = "Thinking";
    private static string TEACHER_WIN_ANIM = "Win";
    private static string TEACHER_LOSE_ANIM = "Lose";

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

        teacherAnimator.SetTrigger(TEACHER_THINKING_ANIM);
        playerAnimator.SetTrigger(PLAYER_THINKING_ANIM);

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

        playerAnimator.SetTrigger(PLAYER_IDLE_ANIM);
        teacherAnimator.SetTrigger(TEACHER_IDLE_ANIM);
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
            resultText.text = "YOU LOSE!";
            resultScreen.SetActive(true);
            playerAnimator.SetTrigger(PLAYER_LOSE_ANIM);
            teacherAnimator.SetTrigger(TEACHER_LOSE_ANIM);
            resultAudioSource.PlayOneShot(loseSound);
        } else if (currentWave > outroTimelines.Length)
        {
            resultText.text = "YOU WIN!";
            resultScreen.SetActive(true);
            playerAnimator.SetTrigger(PLAYER_WIN_ANIM);
            teacherAnimator.SetTrigger(TEACHER_WIN_ANIM);
            resultAudioSource.PlayOneShot(winSound);
        } else
        {
            playerAnimator.SetTrigger(PLAYER_IDLE_ANIM);
            movementNumber = 0;
            introTimeline.Play();
        }
    }

    public void TriggerPlayerMovementAnimation()
    {
        if (movementNumber >= buttonsPressed.Count)
        {
            playerAnimator.SetTrigger(PLAYER_THINKING_ANIM);
        } else if (buttonsPressed[movementNumber] == KeyCode.UpArrow)
        {
            playerAnimator.SetTrigger(PLAYER_UP_ANIM);
        } else if (buttonsPressed[movementNumber] == KeyCode.DownArrow)
        {
            playerAnimator.SetTrigger(PLAYER_DOWN_ANIM);
        } else if (buttonsPressed[movementNumber] == KeyCode.RightArrow)
        {
            playerAnimator.SetTrigger(PLAYER_RIGHT_ANIM);
        } else if (buttonsPressed[movementNumber] == KeyCode.LeftArrow)
        {
            playerAnimator.SetTrigger(PLAYER_LEFT_ANIM);
        }
    }
}
