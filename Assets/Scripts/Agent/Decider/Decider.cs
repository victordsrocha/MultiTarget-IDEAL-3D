using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Decider : MonoBehaviour
{
    // add metodos de atualização do Trace

    public abstract Interaction EnactedInteraction { get; set; }
    public abstract string EnactedInteractionText { get; set; }

    public abstract Interaction SelectInteraction();
    public abstract void  LearnCompositeInteraction(Interaction newEnactedInteraction);
}