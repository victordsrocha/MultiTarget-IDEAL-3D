using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class ChickenMoveVelocity : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6;
    [SerializeField] private float rotationSpeed = 5;
    [SerializeField] private float timeBetweenActions = 0.1f;

    private float _move;
    private float _rotate;
    private Rigidbody _rigidbody;
    private ICharacterBaseAnimation _characterBaseAnimation;

    private Observation _observation;
    private AgentEnvironmentInterface _interface;

    private bool _isRotationComplete;
    private bool _isMoveComplete;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _characterBaseAnimation = GetComponent<ICharacterBaseAnimation>();
        _observation = GetComponent<Observation>();
        _interface = GetComponent<AgentEnvironmentInterface>();
        //SetInitialPosition();
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

    private IEnumerator Move(float duration)
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

        yield return new WaitForSeconds(duration);

        _rigidbody.velocity = Vector3.zero;
        _isMoveComplete = true;
    }

    private IEnumerator Rotate()
    {
        var rotationVector = transform.rotation.eulerAngles.y;
        TweenerCore<Quaternion, Vector3, QuaternionOptions> dot = null;

        switch (_rotate)
        {
            case 1:
                dot = transform.DORotate(new Vector3(0.0f, rotationVector + rotationSpeed, 0.0f), timeBetweenActions);
                break;
            case -1:
                dot = transform.DORotate(new Vector3(0.0f, rotationVector - rotationSpeed, 0.0f), timeBetweenActions);
                break;
        }

        if (dot != null) yield return dot;
        _isRotationComplete = true;
    }

    private void RotateAngularVelocity()
    {
        var rotationVector = transform.up;
        switch (_rotate)
        {
            case 1:
                _rigidbody.angularVelocity = -rotationVector * rotationSpeed;
                break;
            case -1:
                _rigidbody.angularVelocity = rotationVector * rotationSpeed;
                break;
        }
    }

    public void SetMotorsValues(int move, int rotate)
    {
        this._move = move;
        this._rotate = rotate;
    }

    private void UpdateAnimation()
    {
        _characterBaseAnimation.UpdateAnimation(new Vector3(_rotate, _move));
    }

    public IEnumerator EnactAction()
    {
        _isRotationComplete = false;
        _isMoveComplete = false;

        bool forward = this._move > 0;

        UpdateAnimation();

        StartCoroutine(Rotate());
        StartCoroutine(Move(timeBetweenActions));

        yield return new WaitUntil(() => _isRotationComplete && _isMoveComplete);

        _observation.ObservationResult(forward);
        _interface._observationDone = true;
    }
}