using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum PlayerState
{
    Idle,
    Move,
    Eat
}

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float timeBetweenActions = 0.1f;
    [SerializeField] private float rotationSpeed;

    private Rigidbody _myRigidbody;
    private Animator _animator;
    public PlayerState currentState;
    private Vector3 change;

    private static readonly int Eat = Animator.StringToHash("Eat");
    private static readonly int Run = Animator.StringToHash("Run");
    private static readonly int Walk = Animator.StringToHash("Walk");

    private void Start()
    {
        change = Vector3.zero;
        _myRigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        currentState = PlayerState.Idle;
    }

    private void Update()
    {
        change.x = Input.GetAxisRaw("Horizontal");
        change.y = Input.GetAxisRaw("Vertical");

        Debug.Log(change.ToString());

        if (Input.GetButtonDown("Fire1") && currentState != PlayerState.Eat)
        {
            StartCoroutine(EatCoroutine());
        }
        else if (currentState != PlayerState.Eat)
        {
            Move();
            Rotate();
        }

        UpdateState();
    }

    public IEnumerator EatCoroutine()
    {
        change.x = 0;
        change.y = 0;

        _myRigidbody.velocity = Vector3.zero;
        _myRigidbody.angularVelocity = Vector3.zero;

        UpdateAnimatorState(Eat);
        currentState = PlayerState.Eat;
        yield return new WaitForSeconds(.5f);
        currentState = PlayerState.Idle;
        UpdateAnimatorState(0);
    }

    public void Move()
    {
        if (change.y > 0)
        {
            _myRigidbody.velocity = transform.forward * speed;
        }
        else if (change.y < 0)
        {
            _myRigidbody.velocity = -transform.forward * speed;
        }
        else
        {
            _myRigidbody.velocity = Vector3.zero;
        }
    }

    /*
    private void Rotate()
    {
        if (change.x > 0)
        {
            _myRigidbody.angularVelocity = -transform.up * rotationSpeed;
        }
        else if (change.x < 0)
        {
            _myRigidbody.angularVelocity = transform.up * rotationSpeed;
        }
        else
        {
            _myRigidbody.angularVelocity = Vector3.zero;
        }
    }
    */

    private void Rotate()
    {
        var rotationVector = transform.rotation.eulerAngles.y;

        if (change.x > 0)
        {
            transform.DORotate(new Vector3(0.0f, rotationVector + rotationSpeed, 0.0f), timeBetweenActions);
        }
        else if (change.x < 0)
        {
            transform.DORotate(new Vector3(0.0f, rotationVector - rotationSpeed, 0.0f), timeBetweenActions);
        }
    }

    private void UpdateState()
    {
        if (currentState == PlayerState.Eat) return;

        if (change.x != 0 || change.y != 0)
        {
            currentState = PlayerState.Move;
            UpdateAnimatorState(Walk);

            if (change.y > 0)
            {
                UpdateAnimatorState(Run);
            }
        }
        else
        {
            currentState = PlayerState.Idle;
            UpdateAnimatorState(0);
        }
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
}