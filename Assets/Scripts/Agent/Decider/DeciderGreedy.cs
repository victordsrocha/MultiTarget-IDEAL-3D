using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class DeciderGreedy : Decider
{
    [SerializeField] private int threshold;
    public override Interaction EnactedInteraction { get; set; }
    public override string EnactedInteractionText { get; set; }

    private Interaction _superInteraction;
    private Interaction _higherLevelSuperInteraction1;
    private Interaction _higherLevelSuperInteraction2;

    public Memory memory;

    private void Start()
    {
        EnactedInteraction = null;
        _superInteraction = null;
    }

    /// <summary>
    /// The current enacted interaction activates previously learned composite interactions as it matches
    /// their pre-interaction, then the agent gets the activated composited interaction set.
    /// </summary>
    List<Interaction> GetActivatedInteractions()
    {
        var contextInteractions = new List<Interaction>();
        if (EnactedInteraction != null)
        {
            contextInteractions.Add(EnactedInteraction);
            if (!EnactedInteraction.IsPrimitive())
            {
                contextInteractions.Add(EnactedInteraction.PostInteraction);
            }

            if (_superInteraction != null)
            {
                contextInteractions.Add(_superInteraction);
            }

            if (_higherLevelSuperInteraction1 != null)
            {
                contextInteractions.Add(_higherLevelSuperInteraction1);
            }

            if (_higherLevelSuperInteraction2 != null)
            {
                contextInteractions.Add(_higherLevelSuperInteraction2);
            }
        }

        var activatedInteractions = new List<Interaction>();
        foreach (var knowInteraction in memory.KnownInteractions.Values)
        {
            if (knowInteraction.IsPrimitive()) continue;
            if (contextInteractions.Contains(knowInteraction.PreInteraction))
            {
                activatedInteractions.Add(knowInteraction);
            }
        }

        return activatedInteractions;
    }

    private HashSet<Anticipation> Anticipate()
    {
        var activatedInteractions = GetActivatedInteractions();

        var proposedAnticipationsSet = new HashSet<Anticipation>();
        foreach (var activatedInteraction in activatedInteractions)
        {
            float proclivity = activatedInteraction.Weight * activatedInteraction.PostInteraction.Valence;
            var anticipation = new Anticipation(activatedInteraction.PostInteraction.Experiment, proclivity);

            if (!proposedAnticipationsSet.Contains(anticipation))
            {
                proposedAnticipationsSet.Add(anticipation);
            }
        }

        foreach (var anticipation in proposedAnticipationsSet)
        {
            foreach (var experimentEnactedInteraction in anticipation.Experiment.EnactedInteractions)
            {
                foreach (var activatedInteraction in activatedInteractions)
                {
                    if (experimentEnactedInteraction == activatedInteraction.PostInteraction)
                    {
                        var proclivity = activatedInteraction.Weight * experimentEnactedInteraction.Valence;
                        anticipation.AddProclivity(proclivity);
                    }
                }
            }
        }

        return proposedAnticipationsSet;
    }

    private Interaction GetRandomNeutralPrimitiveInteraction()
    {
        return memory.NeutralInteractions[Random.Range(0, memory.NeutralInteractions.Count())];
    }

    public override Interaction SelectInteraction()
    {
        EnactedInteractionText = "";
        HashSet<Anticipation> proposedAnticipationsSet = Anticipate();
        Anticipation selectedAnticipation = proposedAnticipationsSet.Max();

        if (selectedAnticipation == null)
        {
            return GetRandomNeutralPrimitiveInteraction();
        }

        return selectedAnticipation.IntendedInteraction;
    }

    public override void LearnCompositeInteraction(Interaction newEnactedInteraction)
    {
        var previousInteraction = EnactedInteraction;
        var lastInteraction = newEnactedInteraction;
        var previousSuperInteraction = _superInteraction;
        Interaction lastSuperInteraction = null;

        // learn [previous current] called the super interaction
        if (previousInteraction != null)
        {
            lastSuperInteraction =
                memory.AddOrGetAndReinforceCompositeInteraction(previousInteraction, lastInteraction);
        }

        // Learn higher-level interactions
        if (previousSuperInteraction != null)
        {
            // learn [penultimate [previous current]]
            _higherLevelSuperInteraction1 = memory.AddOrGetAndReinforceCompositeInteraction(
                previousSuperInteraction.PreInteraction,
                lastSuperInteraction);

            // learn [[penultimate previous] current]
            _higherLevelSuperInteraction2 =
                memory.AddOrGetAndReinforceCompositeInteraction(previousSuperInteraction, lastInteraction);
        }

        this._superInteraction = lastSuperInteraction;

        if (EnactedInteractionText != "*** Random Pick ***")
        {
            EnactedInteractionText = newEnactedInteraction.Label;
        }
    }
}