using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gizmos = Popcron.Gizmos;


public class GizmoDrawVisionState : MonoBehaviour
{
    public Material material = null;
    public VisionState visionState;
    private FieldOfView _leftFOV;
    private FieldOfView _rightFOV;
    private FieldOfViewAdaptedToEdge _edgeFOV;

    private void Awake()
    {
        _leftFOV = visionState.leftFOV;
        _rightFOV = visionState.rightFOV;
        _edgeFOV = visionState.edgeFOV;
    }

    private void LateUpdate()
    {
        //use custom material, if null it uses a default line material
        Gizmos.Material = material;

        //toggle gizmo drawing
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Gizmos.Enabled = !Gizmos.Enabled;
        }


        if (visionState.isLeftFoodVisible && visionState.leftFood != null)
        {
            Gizmos.Line(_leftFOV.transform.position, visionState.leftFood.position, Color.magenta);
        }

        if (visionState.isRightFoodVisible && visionState.rightFood != null)
        {
            Gizmos.Line(_rightFOV.transform.position, visionState.rightFood.position, Color.magenta);
        }

        if (visionState.isLeftPoisonVisible && visionState.leftPoison != null)
        {
            Gizmos.Line(_leftFOV.transform.position, visionState.leftPoison.position, Color.magenta);
        }

        if (visionState.isRightPoisonVisible && visionState.rightPoison != null)
        {
            Gizmos.Line(_rightFOV.transform.position, visionState.rightPoison.position, Color.magenta);
        }

        if (visionState.isWallVisible)
        {
            Gizmos.Line(_edgeFOV.transform.position, visionState.closestWallPoint, Color.magenta);
        }
    }
}