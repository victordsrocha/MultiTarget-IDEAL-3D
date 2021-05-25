using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionAnticipation : IComparable<ActionAnticipation>
{
    public string Action;
    public Interaction NeutralInteraction;
    public float Proclivity;
    public HashSet<Anticipation> AnticipationsSet;

    public ActionAnticipation(string action, Interaction neutralInteraction)
    {
        Action = action;
        NeutralInteraction = neutralInteraction;
        AnticipationsSet = new HashSet<Anticipation>();
        this.Proclivity = 0;
    }

    public void AddProclivity(float anticipationProclivity)
    {
        this.Proclivity += anticipationProclivity;
    }

    public int CompareTo(ActionAnticipation other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return Proclivity.CompareTo(other.Proclivity);
    }
}