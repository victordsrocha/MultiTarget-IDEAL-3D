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

    public Text fruitsQuantityText;
    public Text poisonsQuantityText;

    public Text hapinessText;

    public FieldOfView rightEyeFov;
    public FieldOfView leftEyeFov;
    public FieldOfViewAdaptedToEdge edgeEyeFov;

    public GameObject fruits;
    public GameObject poisons;

    public Agent agent;

    private void LateUpdate()
    {
        hapinessText.text = "Mood: " + agent.happiness.ToString("F",
            CultureInfo.CreateSpecificCulture("en-US"));

        fruitsQuantityText.text = "Fruits: " + fruits.transform.childCount.ToString();
        poisonsQuantityText.text = "Poisons: " + poisons.transform.childCount.ToString();

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