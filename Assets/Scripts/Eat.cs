using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Eat : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Food") || other.CompareTag("Poison"))
        {
            StartCoroutine(this.transform.parent.GetComponent<Player>().EatCoroutine());
            other.GetComponent<Food>().DestroyFood();
        }
    }
}