using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Eat : MonoBehaviour
{
    private Player _player;

    private void Start()
    {
        _player = transform.parent.GetComponent<Player>();
    }

    private void OnTriggerEnter(Collider fruitCollider)
    {
        if (_player.currentState == PlayerState.Eat) return;
        
        if (fruitCollider.CompareTag("Food") || fruitCollider.CompareTag("Poison"))
        {
            StartCoroutine(_player.EatCoroutine(fruitCollider));
        }
    }
}