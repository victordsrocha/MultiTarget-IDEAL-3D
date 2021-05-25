using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

public class Agent : MonoBehaviour
{
    public Memory memory;
    public Decider decider;
    public Enacter enacter;

    private Queue<int> _lastValences;
    public float happiness;
    public float gamaHappiness;

    public bool stepEnabled;
    public int stepNumber;

    public StepManager stepManager;
    public bool isEnactComplete;
    private bool nextStep;

    private void Start()
    {
        _lastValences = new Queue<int>();

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
        //Profiler.BeginSample("My Sample Happiness");
        
        int queueSize = 10;
        if (_lastValences.Count() == queueSize)
        {
            _lastValences.Dequeue();
        }

        _lastValences.Enqueue(valence);

        float sum = 0;
        int p = 0;
        for (int i = _lastValences.Count; i > 0; i--)
        {
            sum += (float)(_lastValences.ElementAt(i-1) * (Mathf.Pow(gamaHappiness,p)/6.51322));
            p++;
        }

        happiness = sum;
        //Profiler.EndSample();
    }
}