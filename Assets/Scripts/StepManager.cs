using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StepManager : MonoBehaviour
{
    public Text intendedInteractionText;
    public Text enactedInteractionText;

    public Text stepNumberText;

    public bool stepByStep;
    public bool frozen;

    public Button stepButton;
    public Button runButton;

    private void Awake()
    {
        stepButton.onClick.AddListener(StepButton);
        runButton.onClick.AddListener(RunButton);
    }

    private void RunButton()
    {
        stepByStep = false;
        frozen = !frozen;
    }

    private void StepButton()
    {
        stepByStep = true;
        frozen = !frozen;
    }
}