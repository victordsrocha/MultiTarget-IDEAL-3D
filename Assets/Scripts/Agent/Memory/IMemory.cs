using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMemory
{
    Interaction AddOrGetCompositeInteraction(Interaction preInteraction, Interaction postInteraction);
    Interaction AddOrGetAndReinforceCompositeInteraction(Interaction preInteraction, Interaction postInteraction);
    Interaction AddOrGetInteraction(string label);
    Experiment AddOrGetAbstractExperiment(Interaction interaction);
    Interaction AddOrGetPrimitiveInteraction(string label);
    Interaction GetPrimitiveInteraction(string label);
    int CalcValence(string label);
    void Init();
}