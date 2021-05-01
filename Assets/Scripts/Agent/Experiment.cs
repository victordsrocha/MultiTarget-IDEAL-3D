using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Experiment
{
    private string _label;
    public HashSet<Interaction> EnactedInteractions { get; private set; }
    public Interaction IntendedInteraction { get; private set; }

    public Experiment(string label, Interaction intendedInteraction)
    {
        this._label = label;
        IntendedInteraction = intendedInteraction;
        EnactedInteractions = new HashSet<Interaction>();
    }
}