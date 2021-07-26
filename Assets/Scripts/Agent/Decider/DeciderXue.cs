using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// ReSharper disable InconsistentNaming

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
    public float decrementWrongProposition = 0.01f;

    private List<Interaction> activatedInteractionsList;

    private Interaction _selectedInteraction;
    private Anticipation _selectedAnticipation;

    private Interaction _realIntention;

    private float epsilon;

    public Text proclivityText;
    public Text activationWeightText;
    public Text activationText;

    private Queue<Interaction> lastEnactedInteractions;
    private int lastEnactedInteractionsSize;

    private int decisionCycle;
    public bool isRelativeValenceOn;

    private void Start()
    {
        decisionCycle = 0;
        activatedInteractionsList = new List<Interaction>();

        EnactedInteraction = null;
        _superInteraction = null;
        epsilon = 0.95f;

        lastEnactedInteractions = new Queue<Interaction>();
        lastEnactedInteractionsSize = 20;
        for (int i = 0; i < lastEnactedInteractionsSize; i++)
        {
            lastEnactedInteractions.Enqueue(GetRandomNeutralPrimitiveInteraction());
        }
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
            var deltaValence = DeltaValence(activatedInteraction.PostInteraction);

            float proclivity = activatedInteraction.Weight * deltaValence;

            var anticipation = new Anticipation(activatedInteraction.PostInteraction.Experiment, proclivity);
            anticipation.ActivationInteraction = activatedInteraction;

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
                        var deltaValence = DeltaValence(experimentEnactedInteraction);
                        var proclivity = activatedInteraction.Weight * deltaValence;
                        anticipation.AddProclivity(proclivity);
                    }
                }
            }
        }


        return proposedAnticipationsSet;
    }

    private float DeltaValence(Interaction suggestedInteraction)
    {
        /*
        int valence = suggestedInteraction.Valence;
        bool frustrated = true;
        for (int i = 0; i < lastEnactedInteractionsSize; i++)
        {
            if (lastEnactedInteractions.ElementAt(i).Valence > 0)
            {
                frustrated = false;
            }
        }

        if (frustrated)
        {
            valence = suggestedInteraction.GetFinalResult().Valence;
        }

        if (agent.happiness > 0)
        {
            return valence;
        }
        else
        {
            return valence -
                   (agent.happiness / 1.2f); // a divisão por um valor pequeno evita deltaValence zero
        }
        */

        if (isRelativeValenceOn)
        {
            if (!suggestedInteraction.IsPrimitive())
            {
                return suggestedInteraction.GetCompositeDeltaValence(agent.happiness);
            }

            if (agent.happiness > 0)
            {
                return suggestedInteraction.Valence;
            }
            else
            {
                return suggestedInteraction.Valence -
                       (agent.happiness); // a divisão por um valor pequeno evita deltaValence zero
            }
        }
        else
        {
            return suggestedInteraction.Valence;
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
        _selectedAnticipation = null;
        EnactedInteractionText = "";
        HashSet<Anticipation> proposedAnticipationsSet = Anticipate();
        HashSet<Anticipation> proposedAnticipationsSetMapped = Map(proposedAnticipationsSet);
        Anticipation selectedDefaultAnticipation = proposedAnticipationsSetMapped.Max();
        if (!selectedDefaultAnticipation.AnticipationsSet.Any())
        {
            VSRTrace.random = 1;
            //EnactedInteractionText = "*** Random Pick ***";
            _selectedInteraction = GetRandomNeutralPrimitiveInteraction();

            proclivityText.text = "Proclivity = -";
            activationWeightText.text = "Activation weight = -";
            activationText.text = "Activation = -";

            return _selectedInteraction;
        }

        VSRTrace.random = 0;

        Anticipation selectedAnticipation = selectedDefaultAnticipation.AnticipationsSet.Max();
        Interaction selectedInteraction = selectedAnticipation.IntendedInteraction;

        proclivityText.text = "Proclivity = " + selectedAnticipation.Proclivity.ToString(CultureInfo.InvariantCulture);
        activationWeightText.text = "Activation weight = " +
                                    selectedAnticipation.ActivationInteraction.Weight
                                        .ToString(CultureInfo.InvariantCulture);
        activationText.text = "Activation = [" + selectedAnticipation.ActivationInteraction.PreInteraction.Label +
                              "] <color=lime> [" + selectedAnticipation.ActivationInteraction.PostInteraction.Label +
                              "]</color>";

        _selectedAnticipation = selectedAnticipation; // quando não for possível rastrear e penalizar as anticipations
        // em contradição, vou penalizar ao menos a ativação armazenada na antecipação principal

        // com esta mudança a antecipação principal pode acabar sendo penalziada duas vezes!


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

    private void DecrementFailureActivations(Interaction realEnactedInteraction)
    {
        /*
        List<Interaction> usedActivations = new List<Interaction>();
        foreach (Interaction activatedInteraction in activatedInteractionsList)
        {
            if (activatedInteraction.PostInteraction.GetActionsCode() == _selectedInteraction.GetActionsCode())
            {
                usedActivations.Add(activatedInteraction);
            }
        }

        foreach (Interaction usedActivation in usedActivations)
        {
            // TODO otimizar utilizando uma flag para sucesso ou falha
            if (usedActivation.PostInteraction != realEnactedInteraction)
            {
                if (memory.KnownCompositeInteractions.ContainsKey(usedActivation.Label))
                {
                    var dec = Mathf.Max(usedActivation.Weight * decrementWrongProposition, 0.1f);
                    usedActivation.Weight -= dec;

                    if (usedActivation.Weight < 0.01f)
                    {
                        usedActivation.Weight = 0f;
                        memory.ForgottenCompositeInteraction.Add(usedActivation.Label,
                            usedActivation);
                        memory.KnownCompositeInteractions.Remove(usedActivation.Label);
                    }
                }
            }
        }
        */

        // main activation
        if (_selectedInteraction != realEnactedInteraction && _selectedAnticipation != null)
        {
            // independente de filtro: penalizar antecipação principal em caso de falha
            var mainActivation = _selectedAnticipation.ActivationInteraction;
            if (memory.KnownCompositeInteractions.ContainsKey(mainActivation.Label))
            {
                var dec = Mathf.Max(mainActivation.Weight * decrementWrongProposition, 0.1f);
                mainActivation.Weight -= dec;

                if (mainActivation.Weight < 0.01f)
                {
                    mainActivation.Weight = 0f;
                    memory.ForgottenCompositeInteraction.Add(mainActivation.Label,
                        mainActivation);
                    memory.KnownCompositeInteractions.Remove(mainActivation.Label);
                }
            }
        }
    }

    private void DecrementExperimentEnactedInteractions(Interaction realEnactedInteraction)
    {
        /*
         * este método decrementa todas as activatedInteraction com primeira ação igual ao intendedInteraction
         * e feedback diferente do realEnactedInteraction
         *
         * quando aactivatedInteraction tem ação igual a realizada
         */

        foreach (var activatedInteraction in activatedInteractionsList)
        {
            if (activatedInteraction.PostInteraction.GetActionsCode() ==
                realEnactedInteraction.GetActionsCode())
            {
                if (activatedInteraction.PostInteraction != realEnactedInteraction)
                {
                    if (memory.KnownCompositeInteractions.ContainsKey(activatedInteraction.Label))
                    {
                        var dec = Mathf.Max(
                            activatedInteraction.Weight * decrementWrongProposition,
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

    public void LearnAlternative(Interaction newEnactedInteraction)
    {
        DecrementFailureActivations(newEnactedInteraction);

        var previousInteraction = EnactedInteraction;
        var lastInteraction = newEnactedInteraction;
        var previousSuperInteraction = _superInteraction;
        Interaction lastSuperInteraction = null;

        if (lastEnactedInteractions.Count() == lastEnactedInteractionsSize)
        {
            lastEnactedInteractions.Dequeue();
        }

        lastEnactedInteractions.Enqueue(newEnactedInteraction);

        var l1 = lastEnactedInteractions.ElementAt(lastEnactedInteractionsSize - 1);
        var l2 = lastEnactedInteractions.ElementAt(lastEnactedInteractionsSize - 2);
        var l3 = lastEnactedInteractions.ElementAt(lastEnactedInteractionsSize - 3);
        var l4 = lastEnactedInteractions.ElementAt(lastEnactedInteractionsSize - 4);
        var l5 = lastEnactedInteractions.ElementAt(lastEnactedInteractionsSize - 5);


        // II

        var abs21 = memory.AddOrGetAndReinforceCompositeInteraction(l2, l1);

        // III

        var abs32 = memory.AddOrGetCompositeInteraction(l3, l2);

        var abs32_1 = memory.AddOrGetAndReinforceCompositeInteraction(abs32, l1);
        var abs3_21 = memory.AddOrGetAndReinforceCompositeInteraction(l3, abs21);

        // IV

        var abs43 = memory.AddOrGetCompositeInteraction(l4, l3);
        var abs43_2 = memory.AddOrGetCompositeInteraction(abs43, l2);
        var abs4_32 = memory.AddOrGetCompositeInteraction(l4, abs32);


        var abs4_3_21 = memory.AddOrGetAndReinforceCompositeInteraction(l4, abs3_21);
        var abs4_32_1 = memory.AddOrGetAndReinforceCompositeInteraction(l4, abs32_1);
        var abs43_21 = memory.AddOrGetAndReinforceCompositeInteraction(abs43, abs21);
        var abs43_2_1 = memory.AddOrGetAndReinforceCompositeInteraction(abs43_2, l1);
        var abs_4_32_1 = memory.AddOrGetAndReinforceCompositeInteraction(abs4_32, l1);


        // V

        //memory.AddOrGetAndReinforceCompositeInteraction(l5, abs4_3_21);
        //memory.AddOrGetAndReinforceCompositeInteraction(l5, abs4_32_1);
        //memory.AddOrGetAndReinforceCompositeInteraction(l5, abs43_21);
        //memory.AddOrGetAndReinforceCompositeInteraction(l5, abs43_2_1);
        //memory.AddOrGetAndReinforceCompositeInteraction(l5, abs_4_32_1);

        //////
        lastSuperInteraction = abs21;
        _higherLevelSuperInteraction1 = abs3_21;
        _higherLevelSuperInteraction2 = abs32_1;
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

    public override void LearnCompositeInteraction(Interaction newEnactedInteraction)
    {
        //DecrementFailureInteraction(newEnactedInteraction);
        //DecrementExperimentEnactedInteractions(newEnactedInteraction);
        DecrementFailureActivations(newEnactedInteraction);

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