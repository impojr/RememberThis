using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    private bool recieveInput = false;
    private int numberOfButtonsToPress = 0;
    private List<KeyCode> buttonsPressed;

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
        } else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            buttonsPressed.Add(KeyCode.RightArrow);
        } else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            buttonsPressed.Add(KeyCode.UpArrow);
        } else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            buttonsPressed.Add(KeyCode.DownArrow);
        }
    }

    public void RecievePlayerInput(int buttons)
    {
        if (buttons < 0 || buttons > arrows.Length)
            throw new ArgumentOutOfRangeException(nameof(buttons) + " must be within length of " + nameof(arrows));

        numberOfButtonsToPress = 4;
        buttonsPressed = new List<KeyCode>();
        SetChalkboardText("Press buttons");
        chalkboardText.gameObject.SetActive(true);
        timerImage.gameObject.SetActive(true);

        StartCoroutine(StartTimer(10f));
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
        Debug.Log(buttonsPressed.Count);

        for (int i = 0; i < numberOfButtonsToPress; i++)
        {
            Debug.Log("Pressed: " + (arrows[i].arrowKey == buttonsPressed[i]));
        }
    }

    public void SetChalkboardText(string text)
    {
        chalkboardText.text = text;
    }
}
