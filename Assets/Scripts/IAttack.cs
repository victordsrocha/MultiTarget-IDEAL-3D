using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttack
{
    bool IsAttackEnabled { get; }
    void AttackCoroutine();
}