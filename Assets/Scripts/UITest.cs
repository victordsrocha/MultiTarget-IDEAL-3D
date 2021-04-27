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

    public FieldOfView rightEyeFov;
    public FieldOfView leftEyeFov;
    public FieldOfViewAdaptedToEdge edgeEyeFov;

    private void LateUpdate()
    {
        if (leftEyeFov.isTargetVisible)
        {
            if (leftEyeFov.isTargetFood)
            {
                leftEyeText.text = "Left:\t\t " + leftEyeFov.closestTargetDstNormalized.ToString("F") + ", food";
            }
            else
            {
                leftEyeText.text = "Left:\t\t " + leftEyeFov.closestTargetDstNormalized.ToString("F") +
                                   ", poison";
            }
        }
        else
        {
            leftEyeText.text = "Left:\t\t -";
        }

        if (rightEyeFov.isTargetVisible)
        {
            if (rightEyeFov.isTargetFood)
            {
                rightEyeText.text = "Right:\t " + rightEyeFov.closestTargetDstNormalized.ToString("F") + ", food";
            }
            else
            {
                rightEyeText.text = "Right:\t " + rightEyeFov.closestTargetDstNormalized.ToString("F") +
                                    ", poison";
            }
        }
        else
        {
            rightEyeText.text = "Right:\t -";
        }

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