using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class DeciderXue : Decider
{
    [SerializeField] private int threshold;
    public override Interaction EnactedInteraction { get; set; }
    public override string EnactedInteractionText { get; set; }

    private Interaction _superInteraction;
    private Interaction _higherLevelSuperInteraction1;
    private Interaction _higherLevelSuperInteraction2;

    public Memory memory;
    public Agent agent;

    public float decrementFailureRate = 0.05f;
    public float decrementExperimentEnactedInteractionsRate = 0.01f;

    private List<Interaction> activatedInteractionsList;

    private Interaction _selectedInteraction;

    private void Start()
    {
        activatedInteractionsList = new List<Interaction>();

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
        foreach (var knowInteraction in memory.KnownCompositeInteractions.Values)
        {
            if (contextInteractions.Contains(knowInteraction.PreInteraction))
            {
                activatedInteractions.Add(knowInteraction);
            }
        }

        return activatedInteractions;
    }

    private HashSet<Anticipation> Anticipate()
    {
        activatedInteractionsList = GetActivatedInteractions();

        var proposedAnticipationsSet = new HashSet<Anticipation>();

        foreach (var activatedInteraction in activatedInteractionsList)
        {
            var deltaValence = DeltaValence(activatedInteraction.PostInteraction.Valence);

            float proclivity = activatedInteraction.Weight * deltaValence;

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
                foreach (var activatedInteraction in activatedInteractionsList)
                {
                    if (experimentEnactedInteraction == activatedInteraction.PostInteraction)
                    {
                        var deltaValence = DeltaValence(experimentEnactedInteraction.Valence);
                        var proclivity = activatedInteraction.Weight * deltaValence;
                        anticipation.AddProclivity(proclivity);
                    }
                }
            }
        }

        return proposedAnticipationsSet;
    }

    private float DeltaValence(int valence)
    {
        if (agent.happiness > 0)
        {
            return valence;
        }
        else
        {
            return valence - (agent.happiness / 1.2f); // a divisão por um valor pequeno evita deltaValence zero
        }
    }

    private HashSet<Anticipation> Map(HashSet<Anticipation> proposedAnticipationsSet)
    {
        var defaultSet = new HashSet<Anticipation>();
        foreach (var interaction in memory.KnownPrimitiveInteractions.Values)
        {
            var defaultAnticipation = new Anticipation(interaction.Experiment, 0)
            {
                AnticipationsSet = new HashSet<Anticipation>()
            };
            defaultSet.Add(defaultAnticipation);
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

    private Interaction GetRandomNeutralPrimitiveInteraction()
    {
        return memory.NeutralInteractions[Random.Range(0, memory.NeutralInteractions.Count)];
    }

    public override Interaction SelectInteraction()
    {
        EnactedInteractionText = "";
        HashSet<Anticipation> proposedAnticipationsSet = Anticipate();
        HashSet<Anticipation> proposedAnticipationsSetMapped = Map(proposedAnticipationsSet);
        Anticipation selectedDefaultAnticipation = proposedAnticipationsSetMapped.Max();
        if (!selectedDefaultAnticipation.AnticipationsSet.Any())
        {
            VSRTrace.random = 1;
            //EnactedInteractionText = "*** Random Pick ***";
            _selectedInteraction = GetRandomNeutralPrimitiveInteraction();
            return _selectedInteraction;
        }

        VSRTrace.random = 0;

        Anticipation selectedAnticipation = selectedDefaultAnticipation.AnticipationsSet.Max();
        Interaction selectedInteraction = selectedAnticipation.IntendedInteraction;

        if (selectedInteraction.Weight >= threshold && selectedAnticipation.Proclivity > 0)
        {
            _selectedInteraction = selectedInteraction;
        }
        else
        {
            //return selectedAnticipation.GetFirstPrimitiveInteraction();
            _selectedInteraction = selectedAnticipation.IntendedInteraction.IsPrimitive()
                ? selectedAnticipation.IntendedInteraction
                : selectedAnticipation.IntendedInteraction.PreInteraction;
        }

        return _selectedInteraction;
    }

    private void DecrementExperimentEnactedInteractions(Interaction realEnactedInteraction)
    {
        /*
         * Toda interaction tem uma lista de enactedInteractions armazenada em seu respectivo experiment
         * Quando uma interaction é executada, este método diminui a crença em toda a lista de enacted interactions
         * menos na interaction que realmente foi feedback do ultimo experimento realizado
         * este decremento de crença é feito reduzindo o peso da activatedInteraction relacionada
         *
         * Na prática este método decrementa todas as propositions com primeira ação igual ao intendedInteraction
         * e feedback diferente do realEnactedInteraction
         */
        foreach (var experimentEnactedInteraction in _selectedInteraction.Experiment.EnactedInteractions)
        {
            foreach (var activatedInteraction in activatedInteractionsList)
            {
                if (experimentEnactedInteraction == activatedInteraction.PostInteraction)
                {
                    if (activatedInteraction.PostInteraction.GetActionsCode() ==
                        realEnactedInteraction.GetActionsCode())
                    {
                        if (activatedInteraction.PostInteraction != realEnactedInteraction)
                        {
                            if (memory.KnownCompositeInteractions.ContainsKey(activatedInteraction.Label))
                            {
                                var dec = Mathf.Max(
                                    activatedInteraction.Weight * decrementExperimentEnactedInteractionsRate,
                                    Mathf.Abs(memory.forgettingRate / activatedInteraction.Valence));
                                activatedInteraction.Weight -= dec;

                                if (activatedInteraction.Weight < 0.01f)
                                {
                                    activatedInteraction.Weight = 0f;
                                    memory.ForgottenCompositeInteraction.Add(activatedInteraction.Label,
                                        activatedInteraction);
                                    memory.KnownCompositeInteractions.Remove(activatedInteraction.Label);
                                }
                            }
                        }
                    }
                }
            }
        }
    }


    private void DecrementFailureInteraction(Interaction realEnactedInteraction)
    {
        /*
         * não faz sentido
         * para mudar as propositions preciso penalizar a crença/peso das anticipaions
         * não faz sentido penalizar a selectedInteraction
         */
        if (_selectedInteraction.IsPrimitive() || _selectedInteraction == realEnactedInteraction) return;


        if (memory.KnownCompositeInteractions.ContainsKey(_selectedInteraction.Label))
        {
            var dec = Mathf.Max(
                _selectedInteraction.Weight * decrementFailureRate,
                memory.forgettingRate / _selectedInteraction.Valence);

            _selectedInteraction.Weight -= dec;

            if (_selectedInteraction.Weight < 0.01f)
            {
                _selectedInteraction.Weight = 0f;
                memory.ForgottenCompositeInteraction.Add(_selectedInteraction.Label, _selectedInteraction);
                memory.KnownCompositeInteractions.Remove(_selectedInteraction.Label);
            }
        }
    }

    public override void LearnCompositeInteraction(Interaction newEnactedInteraction)
    {
        DecrementFailureInteraction(newEnactedInteraction);
        DecrementExperimentEnactedInteractions(newEnactedInteraction);

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

        // When a schema reaches a weight of 0 it is deleted from memory
        memory.DecrementAndForgetSchemas(new List<Interaction>()
        {
            previousInteraction, lastInteraction, previousSuperInteraction, lastSuperInteraction,
            _higherLevelSuperInteraction1, _higherLevelSuperInteraction2
        });

        if (EnactedInteractionText != "*** Random Pick ***")
        {
            EnactedInteractionText = newEnactedInteraction.Label;
        }
    }
}