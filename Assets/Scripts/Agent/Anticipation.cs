using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anticipation : IComparable<Anticipation>, IEquatable<Anticipation>
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
        if (Proclivity == other.Proclivity)
        {
            return 0;
        }

        if (Proclivity > other.Proclivity)
        {
            return 1;
        }

        return -1;
    }

    public bool Equals(Anticipation other)
    {
        return other != null && Experiment == other.Experiment;
    }
}