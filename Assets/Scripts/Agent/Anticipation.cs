using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anticipation : IComparable<Anticipation>
{
    public Interaction IntendedInteraction;
    public Experiment Experiment;
    public int Proclivity;
    public HashSet<Anticipation> AnticipationsSet;

    public Anticipation(Experiment experiment, int proclivity)
    {
        Experiment = experiment;
        Proclivity = proclivity;
        IntendedInteraction = experiment.IntendedInteraction;
    }

    public void AddProclivity(int incProclivity)
    {
        this.Proclivity += incProclivity;
    }

    public Interaction GetFirstPrimitiveInteraction()
    {
        Interaction firstPrimitiveInteraction = IntendedInteraction;
        while (!firstPrimitiveInteraction.IsPrimitive())
        {
            firstPrimitiveInteraction = firstPrimitiveInteraction.PreInteraction;
        }

        return firstPrimitiveInteraction;
    }

    public int CompareTo(Anticipation other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return Proclivity.CompareTo(other.Proclivity);
    }
}