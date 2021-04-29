using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickenAnimation : MonoBehaviour
{
    private Animator _animator;

    private static readonly int Eat = Animator.StringToHash("Eat");
    private static readonly int Run = Animator.StringToHash("Run");
    private static readonly int Walk = Animator.StringToHash("Walk");

    // Start is called before the first frame update
    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    private void UpdateAnimatorState(int newAnimatorState)
    {
        if (newAnimatorState == Walk)
        {
            _animator.SetBool(Walk, true);
            _animator.SetBool(Run, false);
            _animator.SetBool(Eat, false);
        }
        else if (newAnimatorState == Run)
        {
            _animator.SetBool(Walk, false);
            _animator.SetBool(Run, true);
            _animator.SetBool(Eat, false);
        }
        else if (newAnimatorState == Eat)
        {
            _animator.SetBool(Walk, false);
            _animator.SetBool(Run, false);
            _animator.SetBool(Eat, true);
        }
        else
        {
            _animator.SetBool(Walk, false);
            _animator.SetBool(Run, false);
            _animator.SetBool(Eat, false);
        }
    }

    public IEnumerator EatCoroutineAnimation()
    {
        UpdateAnimatorState(Eat);
        yield return new WaitForSeconds(.4f);
        UpdateAnimatorState(0);
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
}