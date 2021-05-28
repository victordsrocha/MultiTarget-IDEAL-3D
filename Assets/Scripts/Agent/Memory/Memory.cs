using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Memory : MonoBehaviour, IMemory
{
    [SerializeField] private float forgettingRate = 0.05f;
    public Dictionary<string, Interaction> KnownInteractions { get; private set; }
    public Dictionary<string, Experiment> KnownExperiments { get; private set; }

    public List<Interaction> NeutralInteractions { get; private set; }

    private void Awake()
    {
        KnownInteractions = new Dictionary<string, Interaction>();
        KnownExperiments = new Dictionary<string, Experiment>();

        NeutralInteractions = new List<Interaction>();

        Init();
    }

    public Interaction AddOrGetInteraction(string label)
    {
        if (!KnownInteractions.ContainsKey(label))
        {
            Interaction interaction = new Interaction(label);
            KnownInteractions.Add(label, interaction);
        }

        return KnownInteractions[label];
    }

    public Interaction GetPrimitiveInteraction(string label)
    {
        return KnownInteractions[label];
    }

    public Interaction AddOrGetPrimitiveInteraction(string label)
    {
        Interaction interaction;
        if (!KnownInteractions.ContainsKey(label))
        {
            interaction = AddOrGetInteraction(label);
            interaction.Valence = CalcValence(label);
            AddOrGetAbstractExperiment(interaction);
        }
        else
        {
            interaction = KnownInteractions[label];
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
        if (!KnownInteractions.ContainsKey(label))
        {
            interaction = AddOrGetInteraction(label);
            interaction.PreInteraction = preInteraction;
            interaction.PostInteraction = postInteraction;
            interaction.Valence = preInteraction.Valence + postInteraction.Valence;
            AddOrGetAbstractExperiment(interaction);
        }
        else
        {
            interaction = KnownInteractions[label];
        }

        return interaction;
    }

    public Interaction AddOrGetAndReinforceCompositeInteraction(Interaction preInteraction, Interaction postInteraction)
    {
        var compositeInteraction = AddOrGetCompositeInteraction(preInteraction, postInteraction);
        compositeInteraction.Weight += 1;
        return compositeInteraction;
    }

    public void DecrementAndForgetSchemas(List<Interaction> enactedInteractions)
    {
        foreach (var knownInteraction in KnownInteractions.Values)
        {
            if (!knownInteraction.IsPrimitive())
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

                // When a schema reaches a weight of 0 it is deleted from memory
                if (knownInteraction.Weight < 0.01f)
                {
                    // KnownInteractions.Remove(knownInteraction.Label);
                    knownInteraction.Weight = 0f;

                    /* Não é possível excluir, uma interação esquecida pode ser pré-interação de uma outra */
                }
            }
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

        //AddOrGetPrimitiveInteraction(">^,------");
        AddOrGetPrimitiveInteraction(">-,------");
        //AddOrGetPrimitiveInteraction(">v,------");
        AddOrGetPrimitiveInteraction("-^,------");
        //AddOrGetPrimitiveInteraction("--,------");
        AddOrGetPrimitiveInteraction("-v,------");
        //AddOrGetPrimitiveInteraction("<^,------");
        //AddOrGetPrimitiveInteraction("<-,------");
        //AddOrGetPrimitiveInteraction("<v,------");

        foreach (var knownInteraction in KnownInteractions.Values)
        {
            NeutralInteractions.Add(knownInteraction);
        }
    }
}