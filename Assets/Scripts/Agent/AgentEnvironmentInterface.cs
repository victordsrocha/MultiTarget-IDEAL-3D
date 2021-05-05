using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentEnvironmentInterface : MonoBehaviour
{
    private ChickenMoveVelocity _moveVelocity;
    private Memory _memory;

    private Observation _observation;
    public Interaction CurrentEnactedPrimitiveInteraction;

    public bool _observationDone;
    private Enacter _enacter;

    private void Start()
    {
        _moveVelocity = GetComponent<ChickenMoveVelocity>();
        _memory = GetComponent<Memory>();
        _observation = GetComponent<Observation>();
        _enacter = GetComponent<Enacter>();
    }

    public IEnumerator EnactPrimitiveInteraction(Interaction currentIntendedPrimitiveInteraction)
    {
        string result = String.Empty;
        string moveActionCod = currentIntendedPrimitiveInteraction.Label[0].ToString();
        string rotateActionCod = currentIntendedPrimitiveInteraction.Label[1].ToString();
        SendActionToMotors(moveActionCod, rotateActionCod);

        _observationDone = false;
        StartCoroutine(_moveVelocity.EnactAction());
        yield return new WaitUntil(() => _observationDone);

        result += moveActionCod + rotateActionCod;
        result += _observation.Result;
        CurrentEnactedPrimitiveInteraction = _memory.AddOrGetPrimitiveInteraction(result);
        _enacter.nextPrimitiveAction = true;
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