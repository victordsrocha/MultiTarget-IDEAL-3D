using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoveVelocity
{
    void Enable();
    void Disable();
    void SetMotorsValues(int move, int rotate);
    IEnumerator EnactAction(Action getObservation, ref bool observationComplete);
}