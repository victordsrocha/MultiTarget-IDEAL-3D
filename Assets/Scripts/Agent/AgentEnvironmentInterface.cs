using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentEnvironmentInterface : MonoBehaviour
{
    public ChickenMoveVelocity moveVelocity;
    public Memory memory;

    public Observation observation;
    public Interaction CurrentEnactedPrimitiveInteraction;

    public bool _observationDone;
    public Enacter enacter;

    public IEnumerator EnactPrimitiveInteraction(Interaction currentIntendedPrimitiveInteraction)
    {
        string result = String.Empty;
        string moveActionCod = currentIntendedPrimitiveInteraction.Label[0].ToString();
        string rotateActionCod = currentIntendedPrimitiveInteraction.Label[1].ToString();
        SendActionToMotors(moveActionCod, rotateActionCod);

        _observationDone = false;
        StartCoroutine(moveVelocity.EnactAction());
        yield return new WaitUntil(() => _observationDone);

        result += moveActionCod + rotateActionCod;
        result += observation.Result;
        CurrentEnactedPrimitiveInteraction = memory.AddOrGetPrimitiveInteraction(result);
        enacter.nextPrimitiveAction = true;
    }

    private void SendActionToMotors(string moveActionCod, string rotateActionCod)
    {
        // ▶ ▷ △ ▲ ▼ ▽ ◀ ◁ ◇ ◈ ◆ ← → ↑ ↓

        int move = 0, rotate = 0;

        move = moveActionCod switch
        {
            ">" => 1,
            "<" => -1,
            _ => move
        };

        rotate = rotateActionCod switch
        {
            "v" => 1,
            "^" => -1,
            "R" => 2,
            "L" => -2,
            _ => rotate
        };

        moveVelocity.SetMotorsValues(move, rotate);
    }



}