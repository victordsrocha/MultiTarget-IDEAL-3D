using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeciderChicken : Decider
{
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
        var defaultSet = new HashSet<Anticipation>();
        foreach (var interaction in memory.KnownInteractions.Values)
        {
            if (interaction.IsPrimitive())
            {
                var defaultAnticipation = new Anticipation(interaction.Experiment, 0)
                {
                    AnticipationsSet = new HashSet<Anticipation>()
                };
                defaultSet.Add(defaultAnticipation);
            }
        }

        var activatedInteractions = GetActivatedInteractions();

        var proposedAnticipationsSet = new HashSet<Anticipation>();
        foreach (var activatedInteraction in activatedInteractions)
        {
            int proclivity = activatedInteraction.Weight * activatedInteraction.PostInteraction.Valence;
            var anticipation = new Anticipation(activatedInteraction.PostInteraction.Experiment, proclivity);

            if (!proposedAnticipationsSet.Contains(anticipation))
            {
                proposedAnticipationsSet.Add(anticipation);
            }
        }

        foreach (var defaultAnticipation in defaultSet)
        {
            foreach (var anticipation in proposedAnticipationsSet)
            {
                var firstPrimitiveInteraction = anticipation.GetFirstPrimitiveInteraction();
                if (firstPrimitiveInteraction.Experiment == defaultAnticipation.Experiment)
                {
                    defaultAnticipation.AnticipationsSet.Add(anticipation);
                    defaultAnticipation.AddProclivity(anticipation.Proclivity);
                }
            }
        }

        return defaultSet;
    }
    
    public override Interaction SelectInteraction()
    {
        throw new System.NotImplementedException();
    }

    public override void LearnCompositeInteraction(Interaction newEnactedInteraction)
    {
        throw new System.NotImplementedException();
    }
}
