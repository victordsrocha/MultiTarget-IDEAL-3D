using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class BeakTrigger : MonoBehaviour
{
    // 0 => null
    // 1 => food
    // 2 => poison
    public int lastFruit = 0;

    private void OnTriggerEnter(Collider fruitCollider)
    {
        if (fruitCollider.CompareTag("Food") || fruitCollider.CompareTag("Poison"))
        {
            lastFruit = fruitCollider.CompareTag("Food") ? 1 : 2;
            fruitCollider.GetComponent<Fruit>().DestroyFood(0f);
        }
    }
}