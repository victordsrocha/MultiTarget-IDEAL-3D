using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enacter : MonoBehaviour
{
    public Memory memory;
    public AgentEnvironmentInterface envInterface;
    public Interaction FinalEnactedInteraction;
    private Queue<Interaction> _enactedInteractionsPrimitiveQueue;
    
    public bool nextPrimitiveAction;
    public Agent agent;

    private void Start()
    {
        _enactedInteractionsPrimitiveQueue = new Queue<Interaction>();
    }

    public Interaction Enact(Interaction intendedInteraction)
    {
        if (intendedInteraction.IsPrimitive())
        {
            var enactedPrimitiveInteraction = _enactedInteractionsPrimitiveQueue.Dequeue();

            if (enactedPrimitiveInteraction == null)
            {
                throw new Exception();
            }

            return enactedPrimitiveInteraction;
            //return EnvInterface.EnactPrimitiveInteraction(intendedInteraction);
        }
        else
        {
            // Enact the pre-interaction
            var enactedPreInteraction = Enact(intendedInteraction.PreInteraction);
            if (enactedPreInteraction != intendedInteraction.PreInteraction)
            {
                // if the preInteraction failed then the enaction of the intendedInteraction is interrupted here
                return enactedPreInteraction;
            }
            else
            {
                // enact the post-interaction
                var enactedPostInteraction = Enact(intendedInteraction.PostInteraction);
                return memory.AddOrGetCompositeInteraction(enactedPreInteraction, enactedPostInteraction);
            }
        }
    }

    public IEnumerator EnactCoroutine(Interaction intendedInteraction)
    {
        Stack<Interaction> stackNextPrimitive = new Stack<Interaction>();
        stackNextPrimitive.Push(intendedInteraction);

        while (stackNextPrimitive.Count > 0)
        {
            var nextInteraction = stackNextPrimitive.Pop();
            if (nextInteraction.IsPrimitive())
            {
                nextPrimitiveAction = false;
                StartCoroutine(envInterface.EnactPrimitiveInteraction(nextInteraction));
                yield return new WaitUntil(() => nextPrimitiveAction);
                
                _enactedInteractionsPrimitiveQueue.Enqueue(envInterface.CurrentEnactedPrimitiveInteraction);
                if (nextInteraction != envInterface.CurrentEnactedPrimitiveInteraction)
                {
                    break;
                }
            }
            else
            {
                stackNextPrimitive.Push(nextInteraction.PostInteraction);
                stackNextPrimitive.Push(nextInteraction.PreInteraction);
            }
        }

        FinalEnactedInteraction = Enact(intendedInteraction);
        agent.isEnactComplete = true;
    }
}