using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enacter : MonoBehaviour
{
    private AgentEnvironmentInterface envInterface;

    private void Start()
    {
        envInterface = GetComponent<AgentEnvironmentInterface>();
    }

    // precisa ser uma coroutine
    private void EnactIntendedInteraction(Interaction intendedInteraction)
    {
        var primitiveQueue = new Queue<Interaction>();
        var primitiveStack = new Stack<Interaction>();
        primitiveStack.Push(intendedInteraction);

        while (primitiveStack.Any())
        {
            var topInteraction = primitiveStack.Pop();
            if (!topInteraction.IsPrimitive())
            {
                primitiveStack.Push(topInteraction.PostInteraction);
                primitiveStack.Push(topInteraction.PreInteraction);
            }
            else
            {
                primitiveQueue.Enqueue(topInteraction);
            }
        }

        while (primitiveQueue.Any())
        {
            var currentIntendedPrimitiveInteraction = primitiveQueue.Dequeue();

            StartCoroutine(envInterface.EnactPrimitiveInteraction(currentIntendedPrimitiveInteraction));
            
            // WAIT -> esperar corrotina ser encerrada
            var currentEnactedPrimitiveInteraction = envInterface.CurrentEnactedPrimitiveInteraction;
            
            if (currentIntendedPrimitiveInteraction == currentEnactedPrimitiveInteraction)
            {
                // previousRecordInteraction = recordWithStructure(enactedInteraction);
                continue;
            }
            else
            {
                // previousRecordInteraction = recordWithStructure(enactedInteraction);
                break;
            }
        }
    }
}