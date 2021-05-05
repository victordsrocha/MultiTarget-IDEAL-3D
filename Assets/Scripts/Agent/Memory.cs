using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Memory : MonoBehaviour, IMemory
{
    public Dictionary<string, Interaction> KnownInteractions { get; private set; }
    public Dictionary<string, Experiment> KnownExperiments { get; private set; }

    private void Start()
    {
        KnownInteractions = new Dictionary<string, Interaction>();
        KnownExperiments = new Dictionary<string, Experiment>();
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


    public int CalcValence(string label)
    {
        // ▶ ▷ △ ▲ ▼ ▽ ◀ ◁ ◇ ◈ ◆ ← → ↑ ↓

        string[] source = label.Split(new char[] {','});

        int sumValence = 0;

        sumValence += -1 * source[0].Count(c => c == '-'); // Nothing
        sumValence += -1 * source[0].Count(c => c == '↑'); // Rotate Left
        sumValence += -1 * source[0].Count(c => c == '↓'); // Rotate Right
        sumValence += -1 * source[0].Count(c => c == '→'); // Forward
        sumValence += -1 * source[0].Count(c => c == '←'); // Backward

        sumValence += +15 * source[1].Count(c => c == 'a'); // food appear
        sumValence += -15 * source[1].Count(c => c == 'd'); // food disappear
        sumValence += +100 * source[1].Count(c => c == 'r'); // food reached
        sumValence += +10 * source[1].Count(c => c == 'c'); // food closer
        sumValence += -10 * source[1].Count(c => c == 'f'); // food further
        // sumValence += 0 * source[1].Count(c => c == 'u'); // food unchanged

        //sumValence += 0 * source[2].Count(c => c == 'a'); // poison appear
        //sumValence += 0 * source[2].Count(c => c == 'd'); // poison disappear
        sumValence += -100 * source[2].Count(c => c == 'r'); // poison reached
        //sumValence += 0 * source[2].Count(c => c == 'c'); // poison closer
        //sumValence += 0 * source[2].Count(c => c == 'f'); // poison further
        // sumValence += 0 * source[2].Count(c => c == 'u'); // poison unchanged

        //sumValence += 0 * source[3].Count(c => c == 'a'); // wall appear
        //sumValence += 0 * source[3].Count(c => c == 'd'); // wall disappear
        //sumValence += -50 * source[3].Count(c => c == 'b'); // wall bump
        //sumValence += 0 * source[3].Count(c => c == 'c'); // wall closer
        //sumValence += 0 * source[3].Count(c => c == 'f'); // wall further
        // sumValence += 0 * source[3].Count(c => c == 'u'); // wall unchanged
        

        return sumValence;
    }

    public void Init()
    {
        // ▶ ▷ △ ▲ ▼ ▽ ◀ ◁ ◇ ◈ ◆ ← → ↑ ↓

        AddOrGetPrimitiveInteraction("→↑,uu,uu,u,nn");
        AddOrGetPrimitiveInteraction("→-,uu,uu,u,nn");
        AddOrGetPrimitiveInteraction("→↓,uu,uu,u,nn");
        AddOrGetPrimitiveInteraction("-↑,uu,uu,u,nn");
        //AddOrGetPrimitiveInteraction("--,uu,uu,u,nn");
        AddOrGetPrimitiveInteraction("-↓,uu,uu,u,nn");
        //AddOrGetPrimitiveInteraction("←↑,uu,uu,u,nn");
        //AddOrGetPrimitiveInteraction("←-,uu,uu,u,nn");
        //AddOrGetPrimitiveInteraction("←↓,uu,uu,u,nn");
    }
}