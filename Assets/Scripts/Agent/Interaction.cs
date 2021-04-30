using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction
{
    public string Label { get; private set; }

    public Experiment Experiment { get; set; }

    public int Valence { get; set; }

    public Interaction PreInteraction { get; set; }

    public Interaction PostInteraction { get; set; }

    public int Weight { get; set; }

    public Interaction(string label)
    {
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
}