using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyCollider : MonoBehaviour
{
    public bool isColliding;
    public LayerMask wallMask;

    private void Start()
    {
        isColliding = false;
    }

    private void Update()
    {
        isColliding = IsCollidingCheck();
    }

    public bool IsCollidingCheck()
    {
        Collider[] overlapSphere = Physics.OverlapSphere(transform.position, 0.2f, wallMask);
        return overlapSphere.Length != 0;
    }
}