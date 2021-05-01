using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anticipation
{
    private Interaction _intendedInteraction;
    public Experiment Experiment;
    public int Proclivity;
    public HashSet<Anticipation> AnticipationsSet;

    public Anticipation(Experiment experiment, int proclivity)
    {
        Experiment = experiment;
        Proclivity = proclivity;
        _intendedInteraction = experiment.IntendedInteraction;
    }

    public void AddProclivity(int incProclivity)
    {
        this.Proclivity += incProclivity;
    }

    public Interaction GetFirstPrimitiveInteraction()
    {
        Interaction firstPrimitiveInteraction = _intendedInteraction;
        while (!firstPrimitiveInteraction.IsPrimitive())
        {
            firstPrimitiveInteraction = firstPrimitiveInteraction.PreInteraction;
        }

        return firstPrimitiveInteraction;
    }
}