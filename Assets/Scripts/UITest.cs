using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class UITest : MonoBehaviour
{
    public Text rightEyeText;
    public Text leftEyeText;
    public Text edgeEyeText;

    public FieldOfView rightEyeFov;
    public FieldOfView leftEyeFov;
    public FieldOfViewAdaptedToEdge edgeEyeFov;

    private void LateUpdate()
    {
        leftEyeText.text = "Left:\t\t" +
                           leftEyeFov.closestFoodDstNormalized.ToString("F",
                               CultureInfo.CreateSpecificCulture("en-US")) + ", " +
                           leftEyeFov.closestPoisonDstNormalized.ToString("F",
                               CultureInfo.CreateSpecificCulture("en-US"));


        rightEyeText.text = "Right:\t" +
                            rightEyeFov.closestFoodDstNormalized.ToString("F",
                                CultureInfo.CreateSpecificCulture("en-US")) + ", " +
                            rightEyeFov.closestPoisonDstNormalized.ToString("F",
                                CultureInfo.CreateSpecificCulture("en-US"));


        if (edgeEyeFov.isTargetVisible)
        {
            edgeEyeText.text = "Wall:\t " +
                               edgeEyeFov.closestTargetDstNormalized.ToString("F",
                                   CultureInfo.CreateSpecificCulture("en-US"));
        }
        else
        {
            edgeEyeText.text = "Wall:\t -";
        }
    }
}