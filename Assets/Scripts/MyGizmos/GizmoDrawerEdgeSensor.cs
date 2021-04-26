using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gizmos = Popcron.Gizmos;

public class GizmoDrawerEdgeSensor : MonoBehaviour
{
    public Material material = null;

    private FieldOfViewAdaptedToEdge fov;

    private void Start()
    {
        fov = transform.GetComponent<FieldOfViewAdaptedToEdge>();
    }

    private void Update()
    {
        //use custom material, if null it uses a default line material
        Gizmos.Material = material;

        //toggle gizmo drawing
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Gizmos.Enabled = !Gizmos.Enabled;
        }

        if (fov.isTargetVisible)
        {
            Gizmos.Line(fov.transform.position, fov.targetPosition, Color.magenta);
        }
    }
}
