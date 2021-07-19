using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anticipation : IComparable<Anticipation>, IEquatable<Anticipation>
{
    public Interaction IntendedInteraction;
    public Experiment Experiment;
    public float Proclivity;
    public HashSet<Anticipation> AnticipationsSet;
    public Interaction ActivationInteraction;

    public Anticipation(Experiment experiment, float proclivity)
    {
        Experiment = experiment;
        Proclivity = proclivity;
        IntendedInteraction = experiment.IntendedInteraction;
    }

    public void AddProclivity(float incProclivity)
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

    public string GetFirstAction()
    {
        Interaction firstPrimitiveInteraction = GetFirstPrimitiveInteraction();

        string moveActionCod = firstPrimitiveInteraction.Label[0].ToString();
        string rotateActionCod = firstPrimitiveInteraction.Label[1].ToString();

        string firstAction = moveActionCod + rotateActionCod;
        return firstAction;
    }

    public bool Equals(Anticipation other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Equals(Experiment, other.Experiment);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Anticipation) obj);
    }

    public override int GetHashCode()
    {
        return (Experiment != null ? Experiment.GetHashCode() : 0);
    }

    public int CompareTo(Anticipation other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return Proclivity.CompareTo(other.Proclivity);
    }
}