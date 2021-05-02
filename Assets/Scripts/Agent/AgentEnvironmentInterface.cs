using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentEnvironmentInterface : MonoBehaviour
{
    private IMoveVelocity _moveVelocity;
    private Memory _memory;

    private Observation _observation;
    public Interaction CurrentEnactedPrimitiveInteraction;

    private void Start()
    {
        _moveVelocity = GetComponent<IMoveVelocity>();
        _memory = GetComponent<Memory>();
        _observation = GetComponent<Observation>();
    }

    public IEnumerator EnactPrimitiveInteraction(Interaction currentIntendedPrimitiveInteraction)
    {
        string result = String.Empty;
        string moveActionCod = currentIntendedPrimitiveInteraction.Label[0].ToString();
        string rotateActionCod = currentIntendedPrimitiveInteraction.Label[1].ToString();
        SendActionToMotors(moveActionCod, rotateActionCod);

        yield return StartCoroutine(_moveVelocity.EnactAction());

        result += moveActionCod + rotateActionCod;
        result += _observation.ObservationString();
        CurrentEnactedPrimitiveInteraction = _memory.AddOrGetPrimitiveInteraction(result);
    }

    private void SendActionToMotors(string moveActionCod, string rotateActionCod)
    {
        // ▶ ▷ △ ▲ ▼ ▽ ◀ ◁ ◇ ◈ ◆ ← → ↑ ↓

        int move = 0, rotate = 0;

        move = moveActionCod switch
        {
            "→" => 1,
            "←" => -1,
            _ => move
        };

        rotate = rotateActionCod switch
        {
            "↓" => 1,
            "↑" => -1,
            _ => rotate
        };

        _moveVelocity.SetMotorsValues(move, rotate);
    }
}