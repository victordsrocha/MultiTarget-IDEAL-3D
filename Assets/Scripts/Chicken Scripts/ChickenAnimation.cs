using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickenAnimation : MonoBehaviour, ICharacterBaseAnimation
{
    public Animator animator;

    private static readonly int Eat = Animator.StringToHash("Eat");
    private static readonly int Run = Animator.StringToHash("Run");
    private static readonly int Walk = Animator.StringToHash("Walk");

    private void UpdateAnimatorState(int newAnimatorState)
    {
        if (newAnimatorState == Walk)
        {
            animator.SetBool(Walk, true);
            animator.SetBool(Run, false);
            animator.SetBool(Eat, false);
        }
        else if (newAnimatorState == Run)
        {
            animator.SetBool(Walk, false);
            animator.SetBool(Run, true);
            animator.SetBool(Eat, false);
        }
        else if (newAnimatorState == Eat)
        {
            animator.SetBool(Walk, false);
            animator.SetBool(Run, false);
            animator.SetBool(Eat, true);
        }
        else
        {
            animator.SetBool(Walk, false);
            animator.SetBool(Run, false);
            animator.SetBool(Eat, false);
        }
    }

    public void UpdateAnimation(Vector3 change)
    {
        if (change.x != 0 || change.y != 0)
        {
            UpdateAnimatorState(Walk);

            if (change.y > 0)
            {
                UpdateAnimatorState(Run);
            }
        }
        else
        {
            UpdateAnimatorState(0);
        }
    }

    public void PlayAttackAnimation(Action onAnimComplete)
    {
        StartCoroutine(EatCoroutineAnimation(onAnimComplete));
    }

    public IEnumerator EatCoroutineAnimation(Action onAnimComplete)
    {
        UpdateAnimatorState(Eat);
        yield return new WaitForSeconds(0.5f);
        UpdateAnimatorState(0);
        onAnimComplete?.Invoke();
    }
}