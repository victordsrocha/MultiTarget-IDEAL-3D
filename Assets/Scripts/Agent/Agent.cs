using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    private Memory _memory;
    private Decider _decider;
    private Enacter _enacter;
    
    private void Start()
    {
        _memory = GetComponent<Memory>();
        _decider = GetComponent<Decider>();
        _enacter = GetComponent<Enacter>();

        StartCoroutine(StepCoroutine());
    }
    
    private IEnumerator StepCoroutine()
    {
        while (true)
        {
            var intendedInteraction = _decider.SelectInteraction();
            yield return StartCoroutine(TryEnactAndLearnCoroutine(intendedInteraction));
        }
    }
    
    IEnumerator TryEnactAndLearnCoroutine(Interaction intendedInteraction)
    {
        yield return StartCoroutine(_enacter.EnactCoroutine(intendedInteraction));
        Learn(intendedInteraction, _enacter.FinalEnactedInteraction);
    }
    
    void Learn(Interaction intendedInteraction, Interaction enactedInteraction)
    {
        if (enactedInteraction != intendedInteraction)
        {
            intendedInteraction.Experiment.EnactedInteractions.Add(enactedInteraction);
        }

        _decider.LearnCompositeInteraction(enactedInteraction);
        _decider.EnactedInteraction = enactedInteraction;
    }
}
