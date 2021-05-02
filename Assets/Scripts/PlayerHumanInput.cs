using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHumanInput : MonoBehaviour
{
    private float _move;
    private float _rotate;
    private ChickenMoveVelocity _chickenMoveVelocity;

    private void Start()
    {
        _chickenMoveVelocity = GetComponent<ChickenMoveVelocity>();
    }

    private void Update()
    {
        _move = 0;
        _rotate = 0;

        _move = Input.GetAxisRaw("Vertical");
        _rotate = Input.GetAxisRaw("Horizontal");
        _chickenMoveVelocity.SetMotorsValues((int)_move,(int) _rotate);
    }
}