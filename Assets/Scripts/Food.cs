using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    public void DestroyFood()
    {
        Destroy(gameObject, 0.8f);
    }
}