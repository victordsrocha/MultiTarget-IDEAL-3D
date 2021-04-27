using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderScript : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Text text;
    [SerializeField] private FieldOfViewAdaptedToEdge fov;

    private void Start()
    {
        slider.value = 120;
        text.text = slider.value.ToString("0.00");
        slider.onValueChanged.AddListener((v) => { text.text = v.ToString("0.00"); });
        slider.onValueChanged.AddListener(v => { fov.AdjustViewAngle(v);});
    }
}