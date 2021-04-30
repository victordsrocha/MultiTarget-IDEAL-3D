using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Memory : MonoBehaviour, IMemory
{
    private Dictionary<string, Interaction> _knownInteractions;
    private Dictionary<string, Experiment> _knownExperiments;

    private void Start()
    {
        _knownInteractions = new Dictionary<string, Interaction>();
        _knownExperiments = new Dictionary<string, Experiment>();
        Init();
    }

    public Interaction AddOrGetInteraction(string label)
    {
        if (!_knownInteractions.ContainsKey(label))
        {
            Interaction interaction = new Interaction(label);
            _knownInteractions.Add(label, interaction);
        }

        return _knownInteractions[label];
    }

    public Interaction GetPrimitiveInteraction(string label)
    {
        return _knownInteractions[label];
    }

    public Interaction AddOrGetPrimitiveInteraction(string label)
    {
        Interaction interaction;
        if (!_knownInteractions.ContainsKey(label))
        {
            interaction = AddOrGetInteraction(label);
            interaction.Valence = CalcValence(label);
        }
        else
        {
            interaction = _knownInteractions[label];
        }

        return interaction;
    }

    public Experiment AddOrGetAbstractExperiment(Interaction interaction)
    {
        string label = interaction.Label;
        if (!_knownExperiments.ContainsKey(label))
        {
            var abstractExperiment = new Experiment(label, interaction);
            interaction.Experiment = abstractExperiment;
            _knownExperiments.Add(label, abstractExperiment);
        }

        return _knownExperiments[label];
    }

    public Interaction AddOrGetCompositeInteraction(Interaction preInteraction, Interaction postInteraction)
    {
        string label = '(' + preInteraction.Label + ' ' + postInteraction.Label + ')';
        Interaction interaction;
        if (!_knownInteractions.ContainsKey(label))
        {
            interaction = AddOrGetInteraction(label);
            interaction.PreInteraction = preInteraction;
            interaction.PostInteraction = postInteraction;
            interaction.Valence = preInteraction.Valence + postInteraction.Valence;
            AddOrGetAbstractExperiment(interaction);
        }
        else
        {
            interaction = _knownInteractions[label];
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

        sumValence += -1 * source[0].Count(c => c == '↑'); // Rotate Left
        sumValence += -1 * source[0].Count(c => c == '↓'); // Rotate Right

        sumValence += -3 * source[0].Count(c => c == '→'); // Forward
        sumValence += -3 * source[0].Count(c => c == '←'); // Backward

        sumValence += +10 * source[1].Count(c => c == 'a'); // food appear
        sumValence += -15 * source[1].Count(c => c == 'd'); // food disappear
        sumValence += +50 * source[1].Count(c => c == 'r'); // food reached
        sumValence += +20 * source[1].Count(c => c == 'c'); // food closer
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
        sumValence += -50 * source[3].Count(c => c == 'b'); // wall bump
        //sumValence += 0 * source[3].Count(c => c == 'c'); // wall closer
        //sumValence += 0 * source[3].Count(c => c == 'f'); // wall further
        // sumValence += 0 * source[3].Count(c => c == 'u'); // wall unchanged

        return sumValence;
    }

    public void Init()
    {
        // ▶ ▷ △ ▲ ▼ ▽ ◀ ◁ ◇ ◈ ◆ ← → ↑ ↓

        var move1 = AddOrGetPrimitiveInteraction("→↑,uu,uu,u");
        var move2 = AddOrGetPrimitiveInteraction("→-,uu,uu,u");
        var move3 = AddOrGetPrimitiveInteraction("→↓,uu,uu,u");
        var move4 = AddOrGetPrimitiveInteraction("-↑,uu,uu,u");
        var move5 = AddOrGetPrimitiveInteraction("--,uu,uu,u");
        var move6 = AddOrGetPrimitiveInteraction("-↓,uu,uu,u");
        var move7 = AddOrGetPrimitiveInteraction("←↑,uu,uu,u");
        var move8 = AddOrGetPrimitiveInteraction("←-,uu,uu,u");
        var move9 = AddOrGetPrimitiveInteraction("←↓,uu,uu,u");

        AddOrGetAbstractExperiment(move1);
        AddOrGetAbstractExperiment(move2);
        AddOrGetAbstractExperiment(move3);
        AddOrGetAbstractExperiment(move4);
        AddOrGetAbstractExperiment(move5);
        AddOrGetAbstractExperiment(move6);
        AddOrGetAbstractExperiment(move7);
        AddOrGetAbstractExperiment(move8);
        AddOrGetAbstractExperiment(move9);
    }
}