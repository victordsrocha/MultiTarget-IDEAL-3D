using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gizmos = Popcron.Gizmos;

[ExecuteAlways]
public class NewGizmoFocus : MonoBehaviour
{
    public Material material = null;
    public Observation observation;
    public VisionState visionState;
    private Transform focus;

    private FieldOfView _leftFOV;
    private FieldOfView _rightFOV;


    private void Awake()
    {
        //use custom material, if null it uses a default line material
        Gizmos.Material = material;

        focus = null;
        
        _leftFOV = visionState.leftFOV;
        _rightFOV = visionState.rightFOV;
    }

    private void LateUpdate()
    {
        //toggle gizmo drawing
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Gizmos.Enabled = !Gizmos.Enabled;
        }

        focus = observation.focusTransform;
        if (observation.IsFocusSeen() && focus != null)
        {
            var focusPosition = focus.position;
            if (observation.GetFocusEyes() == 2)
            {
                Gizmos.Line(_leftFOV.transform.position, focusPosition, Color.magenta);
                Gizmos.Line(_rightFOV.transform.position, focusPosition, Color.red);
            }
            else if (observation.GetFocusEyes() == 1)
            {
                Gizmos.Line(_leftFOV.transform.position, focusPosition, Color.magenta);
            }
            else
            {
                Gizmos.Line(_rightFOV.transform.position, focusPosition, Color.red);
            }
        }
    }
}