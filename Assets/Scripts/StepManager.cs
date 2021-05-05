using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StepManager : MonoBehaviour
{
    public Text intendedInteractionText;
    public Text enactedInteractionText;
    
    public bool stepByStep;
    public bool frozen;

    public Button stepButton;

    public void StepButton()
    {
        frozen = false;
    }

    public void StepByStepButton()
    {
        stepByStep = !stepByStep;
        frozen = stepByStep;

        stepButton.gameObject.SetActive(stepByStep);
    }
}