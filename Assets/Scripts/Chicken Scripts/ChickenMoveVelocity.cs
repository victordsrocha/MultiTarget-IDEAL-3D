using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class ChickenMoveVelocity : MonoBehaviour, IMoveVelocity
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float timeBetweenActions;

    private float _move;
    private float _rotate;
    private Rigidbody _rigidbody;
    private ICharacterBaseAnimation _characterBaseAnimation;

    private bool _enabled;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _characterBaseAnimation = GetComponent<ICharacterBaseAnimation>();
        SetInitialPosition();
        _enabled = false;
    }

    private void FixedUpdate()
    {
        if (_enabled)
        {
            Move();
            Rotate();
            _characterBaseAnimation.UpdateAnimation(new Vector3(_rotate, _move));
        }
    }

    private void SetInitialPosition()
    {
        float x = Random.Range(-22f, 22f);
        float z = Random.Range(-22f, 22f);
        float rotation = Random.Range(0f, 360f);
        Transform thisTransform;
        (thisTransform = transform).Rotate(Vector3.up, rotation);
        thisTransform.position = new Vector3(x, 0, z);
    }

    private void Move()
    {
        if (_move > 0)
        {
            _rigidbody.velocity = transform.forward * moveSpeed;
        }
        else if (_move < 0)
        {
            _rigidbody.velocity = -transform.forward * moveSpeed;
        }
        else
        {
            _rigidbody.velocity = Vector3.zero;
        }
    }

    private void Rotate()
    {
        var rotationVector = transform.rotation.eulerAngles.y;

        if (_rotate > 0)
        {
            transform.DORotate(new Vector3(0.0f, rotationVector + rotationSpeed, 0.0f), timeBetweenActions);
        }
        else if (_rotate < 0)
        {
            transform.DORotate(new Vector3(0.0f, rotationVector - rotationSpeed, 0.0f), timeBetweenActions);
        }
    }

    public void Enable()
    {
        _enabled = true;
    }

    public void Disable()
    {
        _enabled = false;
        _rigidbody.velocity = Vector3.zero;
    }

    public void SetMotorsValues(int move, int rotate)
    {
        this._move = move;
        this._rotate = rotate;
    }

    public IEnumerator EnactAction()
    {
        Enable();
        yield return new WaitForSeconds(timeBetweenActions);
        Disable();
        // TODO Collect observation here!
    }
}