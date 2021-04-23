// https://github.com/SebLague/Field-of-View
// https://youtu.be/73Dc5JTCmKI
// Adapted

using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(FieldOfViewAdaptedToEdge))]
public class FieldOfViewAdaptedToEdgeEditor : Editor
{
    void OnSceneGUI()
    {
        FieldOfViewAdaptedToEdge fow = (FieldOfViewAdaptedToEdge) target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fow.transform.position, Vector3.up, Vector3.forward, 360, fow.viewRadius);
        Vector3 viewAngleA = fow.DirFromAngle(-fow.viewAngle / 2, false);
        Vector3 viewAngleB = fow.DirFromAngle(fow.viewAngle / 2, false);

        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleA * fow.viewRadius);
        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleB * fow.viewRadius);

        Handles.color = Color.red;
        foreach (Transform visibleTarget in fow.visibleTargets)
        {
            Handles.DrawLine(fow.transform.position, visibleTarget.position);
        }
    }
}