using System;
using UnityEngine;
using Gizmos = Popcron.Gizmos;

[ExecuteAlways]
public class GizmoDrawerSensor : MonoBehaviour
{
    public Material material = null;

    public VisionState visionState;

    private void Update()
    {
        //use custom material, if null it uses a default line material
        Gizmos.Material = material;

        //toggle gizmo drawing
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Gizmos.Enabled = !Gizmos.Enabled;
        }

/*
        if (fov.isTargetVisible)
        {
            Gizmos.Line(fov.transform.position, fov.targetPosition, Color.magenta);
        }
        */
    }
}