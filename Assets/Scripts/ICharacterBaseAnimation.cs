using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterBaseAnimation
{
    void UpdateAnimation(Vector3 changeVector);
    void PlayAttackAnimation(Action onAnimComplete);
}