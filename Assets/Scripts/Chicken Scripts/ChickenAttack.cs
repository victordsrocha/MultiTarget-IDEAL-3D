using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickenAttack : MonoBehaviour, IAttack
{
    private enum State
    {
        Normal,
        Attacking
    }

    private IMoveVelocity _moveVelocity;
    private ICharacterBaseAnimation _characterBaseAnimation;

    public bool IsAttackEnabled => _state == State.Normal;
    private State _state;

    private void Awake()
    {
        _moveVelocity = GetComponent<IMoveVelocity>();
        _characterBaseAnimation = GetComponent<ICharacterBaseAnimation>();
        SetStateNormal();
    }

    private void SetStateNormal()
    {
        _state = State.Normal;
        _moveVelocity.Enable();
    }

    private void SetStateAttacking()
    {
        _state = State.Attacking;
        _moveVelocity.Disable();
    }

    public void AttackCoroutine()
    {
        SetStateAttacking();
        _characterBaseAnimation.PlayAttackAnimation(SetStateNormal);
    }
}