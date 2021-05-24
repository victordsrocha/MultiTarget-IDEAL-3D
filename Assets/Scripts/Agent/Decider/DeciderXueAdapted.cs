using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class DeciderXueAdapted : Decider
{
    [SerializeField] private int threshold;
    public override Interaction EnactedInteraction { get; set; }
    public override string EnactedInteractionText { get; set; }

    private Interaction _superInteraction;
    private Interaction _higherLevelSuperInteraction1;
    private Interaction _higherLevelSuperInteraction2;

    public Memory memory;

    private int _angry;

    private void Start()
    {
        EnactedInteraction = null;
        _superInteraction = null;

        _angry = 0;
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
        /*
        foreach (var activatedInteraction in activatedInteractions)
        {
            int proclivity = activatedInteraction.Weight * activatedInteraction.PostInteraction.Valence;
            var anticipation = new Anticipation(activatedInteraction.PostInteraction.Experiment, proclivity);

            if (!proposedAnticipationsSet.Contains(anticipation))
            {
                proposedAnticipationsSet.Add(anticipation);
            }
        }
        */

        // test
        foreach (var activatedInteraction in activatedInteractions)
        {
            int proclivity;

            if (EnactedInteraction.Valence < 0)
            {
                proclivity = activatedInteraction.Weight *
                             (activatedInteraction.PostInteraction.Valence - EnactedInteraction.Valence);
            }
            else
            {
                proclivity = activatedInteraction.Weight * activatedInteraction.PostInteraction.Valence;
            }

            var anticipation = new Anticipation(activatedInteraction.PostInteraction.Experiment, proclivity);

            if (!proposedAnticipationsSet.Contains(anticipation))
            {
                proposedAnticipationsSet.Add(anticipation);
            }
        }

        /*
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
        */

        // test
        foreach (var anticipation in proposedAnticipationsSet)
        {
            foreach (var experimentEnactedInteraction in anticipation.Experiment.EnactedInteractions)
            {
                foreach (var activatedInteraction in activatedInteractions)
                {
                    if (experimentEnactedInteraction == activatedInteraction.PostInteraction)
                    {
                        int proclivity;
                        if (EnactedInteraction.Valence < 0)
                        {
                            proclivity = activatedInteraction.Weight *
                                         (experimentEnactedInteraction.Valence - EnactedInteraction.Valence);
                        }
                        else
                        {
                            proclivity = activatedInteraction.Weight * experimentEnactedInteraction.Valence;
                        }

                        anticipation.AddProclivity(proclivity);
                    }
                }
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

    private Interaction GetRandomNeutralPrimitiveInteraction(HashSet<Anticipation> defaultSet)
    {
        /*
        List<Anticipation> defaultNeutralList = new List<Anticipation>();
        foreach (var anticipation in defaultSet)
        {
            if (!anticipation.AnticipationsSet.Any())
            {
                defaultNeutralList.Add(anticipation);
            }
        }

        int randomPos = Random.Range(0, defaultNeutralList.Count() - 1);

        return defaultNeutralList[randomPos].IntendedInteraction;
        */

        return memory.NeutralInteractions[Random.Range(0, memory.NeutralInteractions.Count)];

        //return defaultSet.Where(x => x.AnticipationsSet.Count() > 0).OrderByDescending(x => x.Proclivity);

        /*
        Anticipation selectedDefaultAnticipation = defaultSet.Where(x => x.AnticipationsSet.Any()).Max();
        Interaction selectedInteraction;

        if (selectedDefaultAnticipation == null)
        {
            selectedInteraction = memory.NeutralInteractions[Random.Range(0, memory.NeutralInteractions.Count)];
        }
        else
        {
            selectedInteraction = selectedDefaultAnticipation.GetFirstPrimitiveInteraction();
            
            _angry++;
            if (_angry > 4)
            {
                _angry = 0;
                return memory.NeutralInteractions[Random.Range(0, memory.NeutralInteractions.Count)];
            }
            
        }
        
        return selectedInteraction;
        */
    }

    public override Interaction SelectInteraction()
    {
        EnactedInteractionText = "";
        HashSet<Anticipation> defaultSet = Anticipate();
        Anticipation selectedDefaultAnticipation = defaultSet.Max();
        if (!selectedDefaultAnticipation.AnticipationsSet.Any())
        {
            //return selectedDefaultAnticipation.IntendedInteraction;
            //Debug.Log("*Random Pick");
            EnactedInteractionText = "*** Random Pick ***";
            return GetRandomNeutralPrimitiveInteraction(defaultSet);
        }

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