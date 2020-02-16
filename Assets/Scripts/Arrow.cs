using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Arrow : MonoBehaviour
{
    public KeyCode arrowKey;

    public void SetArrowKeyAndUpdateSprite(Tuple<KeyCode, Sprite> arrow)
    {
        arrowKey = arrow.Item1;
        GetComponent<Image>().sprite = arrow.Item2;
    }
}
