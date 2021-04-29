using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITest : MonoBehaviour
{
    public Text rightEyeText;
    public Text leftEyeText;
    public Text edgeEyeText;
    public Text EnergyText;

    public FieldOfView rightEyeFov;
    public FieldOfView leftEyeFov;
    public FieldOfViewAdaptedToEdge edgeEyeFov;
    public Energy energy;

    private void LateUpdate()
    {
        if (energy.currentEnergy != 0)
        {
            EnergyText.text = "Energy: " + energy.currentEnergy;
        }
        else
        {
            EnergyText.text = string.Empty;
        }


        leftEyeText.text = "Left:\t\t " + leftEyeFov.closestFoodDstNormalized.ToString("F") + ", " +
                           leftEyeFov.closestPoisonDstNormalized.ToString("F");


        rightEyeText.text = "Left:\t\t " + rightEyeFov.closestFoodDstNormalized.ToString("F") + ", " +
                            rightEyeFov.closestPoisonDstNormalized.ToString("F");


        if (edgeEyeFov.isTargetVisible)
        {
            edgeEyeText.text = "Wall:\t " + edgeEyeFov.closestTargetDstNormalized.ToString("F");
        }
        else
        {
            edgeEyeText.text = "Wall:\t -";
        }
    }
}