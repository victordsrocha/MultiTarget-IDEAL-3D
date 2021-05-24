using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Agent : MonoBehaviour
{
    public Memory memory;
    public Decider decider;
    public Enacter enacter;

    private Queue<int> _last100Valences;
    public double happiness;
    public double 

    public bool stepEnabled;
    public int stepNumber;

    public StepManager stepManager;
    public bool isEnactComplete;
    private bool nextStep;

    private void Start()
    {
        _last100Valences = new Queue<int>();

        stepNumber = 0;
        StartCoroutine(StepCoroutine());
    }

    private IEnumerator StepCoroutine()
    {
        while (true)
        {
            stepNumber++;
            stepManager.stepNumberText.text = "Step: " + stepNumber.ToString();

            var intendedInteraction = decider.SelectInteraction();

            stepManager.intendedInteractionText.text = "Intended Interaction: (" + intendedInteraction.Valence + ") " +
                                                       intendedInteraction.Label;
            
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
        StartCoroutine(enacter.EnactCoroutine(intendedInteraction));
        yield return new WaitUntil(() => isEnactComplete);

        stepManager.enactedInteractionText.text = "Enacted Interaction: (" + enacter.FinalEnactedInteraction.Valence +
                                                  ") " + enacter.FinalEnactedInteraction.Label;

        if (stepManager.stepByStep)
        {
            stepManager.frozen = true;
            yield return new WaitUntil(() => !stepManager.frozen);
        }

        Learn(intendedInteraction, enacter.FinalEnactedInteraction);
        nextStep = true;
    }

    void Learn(Interaction intendedInteraction, Interaction enactedInteraction)
    {
        if (enactedInteraction != intendedInteraction)
        {
            intendedInteraction.Experiment.EnactedInteractions.Add(enactedInteraction);
        }

        decider.LearnCompositeInteraction(enactedInteraction);
        decider.EnactedInteraction = enactedInteraction;

        //Debug.Log(enactedInteraction.Label);
        Debug.Log(decider.EnactedInteractionText);
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