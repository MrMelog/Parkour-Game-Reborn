using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputManagerScr : MonoBehaviour
{
    public static UnityEvent MouseLeftDown = new();
    public static UnityEvent MouseRightDown = new();

    public bool canLeft = true;
    public bool canRight = true;

    void Update()
    {
        if (!Input.anyKey)
            return;
        if (Input.GetMouseButtonDown(0) && canLeft)
            MouseLeftDown.Invoke();
        if (Input.GetMouseButtonDown(1) && canRight)
            MouseRightDown.Invoke();
    }
}
