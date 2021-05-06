using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Agent : MonoBehaviour
{
    private Memory _memory;
    private Decider _decider;
    private Enacter _enacter;

    private Queue<int> _last100Valences;
    public double happiness;

    public bool stepEnabled;
    public int stepNumber;

    public StepManager stepManager;
    public bool isEnactComplete;
    private bool nextStep;

    private void Start()
    {
        _memory = GetComponent<Memory>();
        _decider = GetComponent<Decider>();
        _enacter = GetComponent<Enacter>();

        _last100Valences = new Queue<int>();

        stepNumber = 0;
        StartCoroutine(StepCoroutine());
    }

    private IEnumerator StepCoroutine()
    {
        while (true)
        {
            stepNumber++;
            var intendedInteraction = _decider.SelectInteraction();

            stepManager.intendedInteractionText.text = "Intended Interaction: " + intendedInteraction.Label;
            //stepManager.enactedInteractionText.text = "Enacted Interaction: ";

            if (stepManager.stepByStep || stepManager.frozen)
            {
                stepManager.frozen = true;
                yield return new WaitUntil(() => !stepManager.frozen);
            }

            nextStep = false;
            StartCoroutine(TryEnactAndLearnCoroutine(intendedInteraction));
            yield return new WaitUntil(() => nextStep);
        }
    }

    IEnumerator TryEnactAndLearnCoroutine(Interaction intendedInteraction)
    {
        isEnactComplete = false;
        StartCoroutine(_enacter.EnactCoroutine(intendedInteraction));
        yield return new WaitUntil(() => isEnactComplete);

        stepManager.enactedInteractionText.text = "Enacted Interaction: " + _enacter.FinalEnactedInteraction.Label;

        if (stepManager.stepByStep)
        {
            stepManager.frozen = true;
            yield return new WaitUntil(() => !stepManager.frozen);
        }

        Learn(intendedInteraction, _enacter.FinalEnactedInteraction);
        nextStep = true;
    }

    void Learn(Interaction intendedInteraction, Interaction enactedInteraction)
    {
        if (enactedInteraction != intendedInteraction)
        {
            intendedInteraction.Experiment.EnactedInteractions.Add(enactedInteraction);
        }

        _decider.LearnCompositeInteraction(enactedInteraction);
        _decider.EnactedInteraction = enactedInteraction;

        //Debug.Log(enactedInteraction.Label);
        Debug.Log(_decider.enactedInteractionText);
        UpdateHapiness(enactedInteraction.Valence);
    }

    void UpdateHapiness(int valence)
    {
        if (_last100Valences.Count() == 100)
        {
            _last100Valences.Dequeue();
        }

        _last100Valences.Enqueue(valence);

        float sum = 0;
        foreach (var v in _last100Valences)
        {
            sum += v;
        }

        happiness = sum / _last100Valences.Count();
    }
}