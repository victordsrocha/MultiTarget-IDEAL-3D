using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyCollider : MonoBehaviour
{
    private Collider _collider;
    public bool isColliding;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        isColliding = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        isColliding = true;
    }

    private void OnCollisionExit(Collision other)
    {
        isColliding = false;
    }
}