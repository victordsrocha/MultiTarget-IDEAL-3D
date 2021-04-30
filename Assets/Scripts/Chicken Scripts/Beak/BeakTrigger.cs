using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class BeakTrigger : MonoBehaviour
{
    private IAttack _playerAttack;

    private void Start()
    {
        _playerAttack = transform.parent.GetComponent<IAttack>();
    }

    private void OnTriggerEnter(Collider fruitCollider)
    {
        if (_playerAttack.IsAttackEnabled)
        {
            if (fruitCollider.CompareTag("Food") || fruitCollider.CompareTag("Poison"))
            {
                fruitCollider.GetComponent<Fruit>().DestroyFood(.4f); 
                _playerAttack.Attack();
            }
        }
    }
}