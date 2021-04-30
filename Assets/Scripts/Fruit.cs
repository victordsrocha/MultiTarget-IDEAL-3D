using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fruit : MonoBehaviour
{
    public void DestroyFood(float delay)
    {
        Destroy(gameObject, delay);
    }
}