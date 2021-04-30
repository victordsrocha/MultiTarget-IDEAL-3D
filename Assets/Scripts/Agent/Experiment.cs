using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Experiment
{
    private string _label;
    private HashSet<Interaction> _enactedInteractions;
    private Interaction _intendedInteraction;

    public Experiment(string label, Interaction intendedInteraction)
    {
        this._label = label;
        _intendedInteraction = intendedInteraction;
        _enactedInteractions = new HashSet<Interaction>();
    }
}