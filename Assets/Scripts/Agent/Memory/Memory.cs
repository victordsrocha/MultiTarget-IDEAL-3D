using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Memory : MonoBehaviour, IMemory
{
    public float forgettingRate = 0.05f;
    public Dictionary<string, Interaction> KnownCompositeInteractions { get; private set; }
    public Dictionary<string, Interaction> KnownPrimitiveInteractions { get; private set; }
    public Dictionary<string, Interaction> ForgottenCompositeInteraction { get; private set; }
    public Dictionary<string, Experiment> KnownExperiments { get; private set; }

    public List<Interaction> NeutralInteractions { get; private set; }

    private int primitiveLabelSize;

    private void Awake()
    {
        KnownCompositeInteractions = new Dictionary<string, Interaction>();
        KnownPrimitiveInteractions = new Dictionary<string, Interaction>();
        ForgottenCompositeInteraction = new Dictionary<string, Interaction>();

        KnownExperiments = new Dictionary<string, Experiment>();

        NeutralInteractions = new List<Interaction>();

        Init();
    }

    public Interaction AddOrGetInteraction(string label)
    {
        if (label.Length == primitiveLabelSize)
        {
            if (KnownPrimitiveInteractions.ContainsKey(label))
            {
                return KnownPrimitiveInteractions[label];
            }
            else
            {
                Interaction interaction = new Interaction(label);
                KnownPrimitiveInteractions.Add(label, interaction);
                return KnownPrimitiveInteractions[label];
            }
        }
        else
        {
            if (KnownCompositeInteractions.ContainsKey(label))
            {
                return KnownCompositeInteractions[label];
            }
            else if (ForgottenCompositeInteraction.ContainsKey(label))
            {
                return ForgottenCompositeInteraction[label];
            }
            else
            {
                Interaction interaction = new Interaction(label);
                KnownCompositeInteractions.Add(label, interaction);
                return KnownCompositeInteractions[label];
            }
        }
    }

    public Interaction GetPrimitiveInteraction(string label)
    {
        return KnownPrimitiveInteractions[label];
    }

    public Interaction AddOrGetPrimitiveInteraction(string label)
    {
        Interaction interaction;
        if (!KnownPrimitiveInteractions.ContainsKey(label))
        {
            interaction = AddOrGetInteraction(label);
            interaction.Valence = CalcValence(label);
            AddOrGetAbstractExperiment(interaction);
        }
        else
        {
            interaction = KnownPrimitiveInteractions[label];
        }

        return interaction;
    }

    public Experiment AddOrGetAbstractExperiment(Interaction interaction)
    {
        string label = interaction.Label;
        if (!KnownExperiments.ContainsKey(label))
        {
            var abstractExperiment = new Experiment(label, interaction);
            interaction.Experiment = abstractExperiment;
            KnownExperiments.Add(label, abstractExperiment);
        }

        return KnownExperiments[label];
    }

    public Interaction AddOrGetCompositeInteraction(Interaction preInteraction, Interaction postInteraction)
    {
        string label = '(' + preInteraction.Label + ' ' + postInteraction.Label + ')';
        Interaction interaction;
        if (!KnownCompositeInteractions.ContainsKey(label) && !ForgottenCompositeInteraction.ContainsKey(label))
        {
            interaction = AddOrGetInteraction(label);
            interaction.PreInteraction = preInteraction;
            interaction.PostInteraction = postInteraction;
            interaction.Valence = preInteraction.Valence + postInteraction.Valence;
            interaction.Weight = 1f;
            AddOrGetAbstractExperiment(interaction);
        }
        else
        {
            if (KnownCompositeInteractions.ContainsKey(label))
            {
                interaction = KnownCompositeInteractions[label];
            }
            else
            {
                interaction = ForgottenCompositeInteraction[label];
            }
        }

        return interaction;
    }

    public Interaction AddOrGetAndReinforceCompositeInteraction(Interaction preInteraction, Interaction postInteraction)
    {
        var compositeInteraction = AddOrGetCompositeInteraction(preInteraction, postInteraction);

        if (compositeInteraction.Weight == 0f)
        {
            KnownCompositeInteractions.Add(compositeInteraction.Label, compositeInteraction);
            ForgottenCompositeInteraction.Remove(compositeInteraction.Label);
        }

        compositeInteraction.Weight += 1;
        return compositeInteraction;
    }

    public void DecrementAndForgetSchemas(List<Interaction> enactedInteractions)
    {
        List<Interaction> interactionsToForget = new List<Interaction>();
        foreach (var knownInteraction in KnownCompositeInteractions.Values)
        {
            if (!enactedInteractions.Contains(knownInteraction))
            {
                // estou dividindo pela valencia como uma forma provisoria de garantir que 
                // memorias muito boas ou muito ruins sejam mais dificilemtne esquecidas
                if (knownInteraction.Valence > 1 || knownInteraction.Valence < -1)
                {
                    knownInteraction.Weight -= forgettingRate / Mathf.Abs(knownInteraction.Valence);
                }
                else
                {
                    knownInteraction.Weight -= forgettingRate;
                }
            }

            if (knownInteraction.Weight < 0.01f)
            {
                interactionsToForget.Add(knownInteraction);
            }
        }

        foreach (var interaction in interactionsToForget)
        {
            interaction.Weight = 0f;
            ForgottenCompositeInteraction.Add(interaction.Label, interaction);
            KnownCompositeInteractions.Remove(interaction.Label);
        }
    }


    public int CalcValence(string label)
    {
        // ▶ ▷ △ ▲ ▼ ▽ ◀ ◁ ◇ ◈ ◆ ← → ↑ ↓ 

        string[] source = label.Split(new char[] {','});

        int sumValence = 0;

        sumValence += -1 * source[0].Count(c => c == '-'); // Nothing
        sumValence += -1 * source[0].Count(c => c == '^'); // Rotate Left
        sumValence += -1 * source[0].Count(c => c == 'v'); // Rotate Right
        sumValence += -1 * source[0].Count(c => c == '>'); // Forward
        sumValence += -1 * source[0].Count(c => c == '<'); // Backward

        bool focusFood = source[1][0] == 'f';

        // Food status
        if (focusFood)
        {
            switch (source[1][1])
            {
                case 'a':
                    sumValence += 0;
                    break;
                case 'd':
                    sumValence -= 0;
                    break;
                case 'c':
                    sumValence += 10;
                    break;
                case 'f':
                    sumValence -= 10;
                    break;
            }
        }

        // Reach
        if (source[1][3] == 'f')
        {
            sumValence += 200;
        }
        else if (source[1][3] == 'p')
        {
            sumValence -= 400;
        }

        // Bump
        switch (source[1][5])
        {
            case 'b':
                sumValence -= 5;
                break;
            case 'B':
                sumValence -= 20;
                break;
            case 'l':
                sumValence += 5;
                break;
        }

        return sumValence;
    }

    public void Init()
    {
        // ▶ ▷ △ ▲ ▼ ▽ ◀ ◁ ◇ ◈ ◆ ← → ↑ ↓

        primitiveLabelSize = ">-,------".Length;

        //AddOrGetPrimitiveInteraction(">^,------");
        AddOrGetPrimitiveInteraction(">-,------");
        //AddOrGetPrimitiveInteraction(">v,------");
        AddOrGetPrimitiveInteraction("-^,------");
        //AddOrGetPrimitiveInteraction("--,------");
        AddOrGetPrimitiveInteraction("-v,------");
        //AddOrGetPrimitiveInteraction("<^,------");
        //AddOrGetPrimitiveInteraction("<-,------");
        //AddOrGetPrimitiveInteraction("<v,------");

        foreach (var knownInteraction in KnownPrimitiveInteractions.Values)
        {
            NeutralInteractions.Add(knownInteraction);
        }
    }
}