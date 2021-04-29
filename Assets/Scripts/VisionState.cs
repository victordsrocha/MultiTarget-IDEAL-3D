using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class VisionState : MonoBehaviour
{
    public FieldOfView leftFOV;
    public FieldOfView rightFOV;
    public FieldOfViewAdaptedToEdge edgeFOV;

    public bool isRightFoodVisible;
    public Transform rightFood;
    public float rightFoodDstNormalized;

    public bool isRightPoisonVisible;
    public Transform rightPoison;
    public float rightPoisonDstNormalized;
    
    public bool isLeftFoodVisible;
    public Transform leftFood;
    public float leftFoodDstNormalized;

    public bool isLeftPoisonVisible;
    public Transform leftPoison;
    public float leftPoisonDstNormalized;

    public bool isWallVisible;
    public Vector3 closestWallPoint;
    public float wallDstNormalized;

    private void Update()
    {
        UpdateVisionState(); // No futuro deverá ser chamada somente ao fim de cada interação primitiva
    }

    private void UpdateVisionState()
    {
        isRightFoodVisible = rightFOV.isFoodVisible;
        rightFood = rightFOV.closestFood;
        rightFoodDstNormalized = rightFOV.closestFoodDstNormalized;

        isRightPoisonVisible = rightFOV.isPoisonVisible;
        rightPoison = rightFOV.closestPoison;
        rightPoisonDstNormalized = rightFOV.closestPoisonDstNormalized;

        isLeftFoodVisible = leftFOV.isFoodVisible;
        leftFood = leftFOV.closestFood;
        leftFoodDstNormalized = leftFOV.closestFoodDstNormalized;

        isLeftPoisonVisible = leftFOV.isPoisonVisible;
        leftPoison = leftFOV.closestPoison;
        leftPoisonDstNormalized = leftFOV.closestPoisonDstNormalized;

        isWallVisible = edgeFOV.isTargetVisible;
        closestWallPoint = edgeFOV.targetPosition;
        wallDstNormalized = edgeFOV.closestTargetDstNormalized;
    }
}