using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderScript : MonoBehaviour
{
    public FruitsManager fruitsManager;
    
    [SerializeField] private Slider slider;
    [SerializeField] private Text text;

    private void Start()
    {
        slider.value = (int)20;
        text.text = "# targets: " + slider.value.ToString("00");
        slider.onValueChanged.AddListener((v) => {text.text = "# targets: " + v.ToString("00");});
        slider.onValueChanged.AddListener(v => { fruitsManager.fruitMaxQuantity=(int)v;});
    }
}