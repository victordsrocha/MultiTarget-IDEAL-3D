using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    public Toggle speedToggle;
    private bool _fast = false;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0; 
        //Time.captureFramerate = 50;
    }

    private void Update()
    {
        if (speedToggle.isOn != _fast)
        {
            _fast = !_fast;
            Time.captureFramerate = Time.captureFramerate == 0 ? 50 : 0;
        }
    }
}