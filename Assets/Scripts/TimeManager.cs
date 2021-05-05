using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public int captureFramerate = 0;
    public int timeScale = 1;

    private void Update()
    {
        Time.captureFramerate = captureFramerate;
        Time.timeScale = timeScale;
    }
}