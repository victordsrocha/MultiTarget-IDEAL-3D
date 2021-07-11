using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;

public class Interaction
{
    public string Label { get; private set; }

    public Experiment Experiment { get; set; }

    public int Valence { get; set; }

    public Interaction PreInteraction { get; set; }

    public Interaction PostInteraction { get; set; }

    public float Weight { get; set; }

    public List<Interaction> InteractionsList { get; set; }

    public Interaction(string label)
    {
        InteractionsList = new List<Interaction>();
        this.Label = label;
        Experiment = null;
        Valence = 0;
        PreInteraction = null;
        PostInteraction = null;
        Weight = 0;
    }

    public Interaction(string label, int valence)
    {
        this.Label = label;
        Experiment = null;
        Valence = 0;
        PreInteraction = null;
        PostInteraction = null;
        Weight = 0;
    }

    public bool IsPrimitive()
    {
        return PreInteraction == null;
    }

    public string GetActionsCode()
    {
        var actions = string.Empty;
        foreach (var i in InteractionsList)
        {
            actions += i.Label[0];
            actions += i.Label[1];
        }

        return actions;
    }
}