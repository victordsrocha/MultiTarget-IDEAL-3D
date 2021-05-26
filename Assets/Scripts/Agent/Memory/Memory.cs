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

        sumValence += +15 * source[1].Count(c => c == 'a'); // food appear
        sumValence += -15 * source[1].Count(c => c == 'd'); // food disappear
        sumValence += +100 * source[1].Count(c => c == 'r'); // food reached
        sumValence += +10 * source[1].Count(c => c == 'c'); // food closer
        sumValence += -10 * source[1].Count(c => c == 'f'); // food further
        // sumValence += 0 * source[1].Count(c => c == 'u'); // food unchanged

        //sumValence += -10 * source[2].Count(c => c == 'a'); // poison appear
        //sumValence += +10 * source[2].Count(c => c == 'd'); // poison disappear
        sumValence += -100 * source[2].Count(c => c == 'r'); // poison reached 
        //sumValence += -4 * source[2].Count(c => c == 'c'); // poison closer
        //sumValence += +4 * source[2].Count(c => c == 'f'); // poison further
        // sumValence += 0 * source[2].Count(c => c == 'u'); // poison unchanged

        //sumValence += 0 * source[3].Count(c => c == 'a'); // wall appear
        //sumValence += 0 * source[3].Count(c => c == 'd'); // wall disappear
        sumValence += -20 * source[3].Count(c => c == 'b'); // wall bump
        sumValence += +10 * source[3].Count(c => c == 'l'); // wall release
        //sumValence += 0 * source[3].Count(c => c == 'c'); // wall closer
        //sumValence += 3 * source[3].Count(c => c == 'f'); // wall further
        // sumValence += 0 * source[3].Count(c => c == 'u'); // wall unchanged


        return sumValence;
    }

    public void Init()
    {
        // ▶ ▷ △ ▲ ▼ ▽ ◀ ◁ ◇ ◈ ◆ ← → ↑ ↓

        //AddOrGetPrimitiveInteraction(">^,uu,uu,u,nn");
        AddOrGetPrimitiveInteraction(">-,uu,uu,u,nn");
        //AddOrGetPrimitiveInteraction(">v,uu,uu,u,nn");
        AddOrGetPrimitiveInteraction("-^,uu,uu,u,nn");
        //AddOrGetPrimitiveInteraction("--,uu,uu,u,nn");
        AddOrGetPrimitiveInteraction("-v,uu,uu,u,nn");
        //AddOrGetPrimitiveInteraction("<^,uu,uu,u,nn");
        //AddOrGetPrimitiveInteraction("<-,uu,uu,u,nn");
        //AddOrGetPrimitiveInteraction("<v,uu,uu,u,nn");

        foreach (var knownInteraction in KnownInteractions.Values)
        {
            NeutralInteractions.Add(knownInteraction);
        }
    }
}