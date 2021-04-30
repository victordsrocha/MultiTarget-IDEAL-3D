using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

public class FruitsManager : MonoBehaviour
{
    public LayerMask obstacleMask;
    public Fruit fruitPrefab;
    public Fruit poisonPrefab;

    public int fruitMaxQuantity;

    public GameObject foodListGameObject;
    public GameObject poisonListGameObject;

    private void Update()
    {
        if (QuantityFruits() < fruitMaxQuantity)
        {
            InstantiateNewFruit();
        }
    }

    private int QuantityFruits()
    {
        return foodListGameObject.transform.childCount + poisonListGameObject.transform.childCount;
    }

    private Fruit InstantiateNewFruit()
    {
        //var targetPrefab = 0.5 > Random.Range(0, 1) ? foodPrefab : poisonPrefab;

        Fruit targetPrefab;
        GameObject parent;

        if (0.5 > Random.Range(0f, 1f))
        {
            targetPrefab = fruitPrefab;
            parent = foodListGameObject;
        }
        else
        {
            targetPrefab = poisonPrefab;
            parent = poisonListGameObject;
        }

        Fruit newTarget = null;

        float x = Random.Range(-23.9f, 23.9f);
        float z = Random.Range(-23.9f, 23.9f);
        Vector3 birthSpot = new Vector3(x, 0.5f, z);

        Collider[] overlapSphere = Physics.OverlapSphere(birthSpot, 0.51f, obstacleMask);
        if (overlapSphere.Length == 0)
        {
            newTarget = Instantiate(targetPrefab, parent.transform, true);
            newTarget.transform.position = birthSpot;
            
            
            //newTarget.birthTime = Time.time;

            //int time = (int) Time.time;
            //AddRecordTarget(time.ToString(), "targetRecord.csv");
        }

        return newTarget;
    }
}