using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

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
    public PlayerState currentState;
    public Vector3 change;

    private ChickenAnimation _animation;

    private void Awake()
    {
        float x = Random.Range(-22f, 22f);
        float z = Random.Range(-22f, 22f);

        float rotation = Random.Range(0f, 360f);

        transform.Rotate(Vector3.up, rotation);

        transform.position = new Vector3(x, 0, z);
    }

    private void Start()
    {
        change = Vector3.zero;
        _myRigidbody = GetComponent<Rigidbody>();
        currentState = PlayerState.Idle;

        _animation = GetComponent<ChickenAnimation>();
    }

    private void Update()
    {
        change.x = Input.GetAxisRaw("Horizontal");
        change.y = Input.GetAxisRaw("Vertical");
        
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
        _animation.UpdateAnimation(change);
    }

    public IEnumerator EatCoroutine()
    {
        change.x = 0;
        change.y = 0;

        _myRigidbody.velocity = Vector3.zero;
        _myRigidbody.angularVelocity = Vector3.zero;

        currentState = PlayerState.Eat;
        yield return StartCoroutine(_animation.EatCoroutineAnimation());
        currentState = PlayerState.Idle;
    }

    private void Move()
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
        }
        else
        {
            currentState = PlayerState.Idle;
        }
    }
}