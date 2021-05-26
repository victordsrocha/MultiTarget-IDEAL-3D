using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeciderChicken : Decider
{
    public override Interaction EnactedInteraction { get; set; }
    public override string EnactedInteractionText { get; set; }

    [SerializeField] private int threshold;

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

    private HashSet<ActionAnticipation> Anticipate()
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

        var defaultSet = new HashSet<ActionAnticipation>
        {
            new ActionAnticipation("→↑", memory.GetPrimitiveInteraction("→↑,uu,uu,u,nn")),
            new ActionAnticipation("→-", memory.GetPrimitiveInteraction("→-,uu,uu,u,nn")),
            new ActionAnticipation("→↓", memory.GetPrimitiveInteraction("→↓,uu,uu,u,nn")),
            new ActionAnticipation("-↑", memory.GetPrimitiveInteraction("-↑,uu,uu,u,nn")),
            //new ActionAnticipation("--", memory.GetPrimitiveInteraction("--,uu,uu,u,nn")),
            new ActionAnticipation("-↓", memory.GetPrimitiveInteraction("-↓,uu,uu,u,nn")),
            //new ActionAnticipation("←↑", memory.GetPrimitiveInteraction("←↑,uu,uu,u,nn")),
            //new ActionAnticipation("←-", memory.GetPrimitiveInteraction("←-,uu,uu,u,nn")),
            //new ActionAnticipation("←↓", memory.GetPrimitiveInteraction("←↓,uu,uu,u,nn"))
        };

        foreach (var actionAnticipation in defaultSet)
        {
            foreach (var anticipation in proposedAnticipationsSet)
            {
                string firstAction = anticipation.GetFirstAction();
                if (firstAction == actionAnticipation.Action)
                {
                    actionAnticipation.AnticipationsSet.Add(anticipation);
                    actionAnticipation.AddProclivity(anticipation.Proclivity);
                }
            }
        }

        return defaultSet;
    }

    private Interaction GetRandomNeutralPrimitiveInteraction(HashSet<ActionAnticipation> defaultSet)
    {
        List<ActionAnticipation> defaultNeutralList = new List<ActionAnticipation>();
        foreach (ActionAnticipation actionAnticipation in defaultSet)
        {
            if (!actionAnticipation.AnticipationsSet.Any())
            {
                defaultNeutralList.Add(actionAnticipation);
            }
        }

        int randomPos = Random.Range(0, defaultNeutralList.Count() - 1);
        return defaultNeutralList[randomPos].NeutralInteraction;
    }

    public override Interaction SelectInteraction()
    {
        EnactedInteractionText = "";
        var defaultSet = Anticipate();

        int c = 0;
        foreach (var actionAnticipation in defaultSet)
        {
            c += actionAnticipation.AnticipationsSet.Count();
        }

        if (c == 0)
        {
            EnactedInteractionText = "*** Random Pick ***";
            return GetRandomNeutralPrimitiveInteraction(defaultSet);
        } 

        ActionAnticipation selectedDefaultAnticipation = defaultSet.Where(x=>x.AnticipationsSet.Any()).Max();

        Anticipation selectedAnticipation = selectedDefaultAnticipation.AnticipationsSet.Max();
        Interaction selectedInteraction = selectedAnticipation.IntendedInteraction;

        if (selectedInteraction.Weight >= threshold && selectedAnticipation.Proclivity > 0)
        {
            return selectedInteraction;
        }
        else
        {
            return selectedAnticipation.GetFirstPrimitiveInteraction();
        }
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