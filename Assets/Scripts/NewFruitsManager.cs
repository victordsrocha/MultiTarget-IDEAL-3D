using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewFruitsManager : MonoBehaviour
{
    public LayerMask obstacleMask;
    public Fruit fruitPrefab;
    public Fruit poisonPrefab;

    public int fruitQuantity;
    public int poisonQuantity;

    public GameObject foodListGameObject;
    public GameObject poisonListGameObject;

    public Agent agent;

    private void Update()
    {
        if (CurrentQuantityFruits() < fruitQuantity)
        {
            InstantiateNewFruit(fruitPrefab, foodListGameObject);
        }

        if (CurrentQuantityPoisons() < poisonQuantity)
        {
            InstantiateNewFruit(poisonPrefab, poisonListGameObject);
        }
    }

    private int CurrentQuantityFruits()
    {
        return foodListGameObject.transform.childCount;
    }

    private int CurrentQuantityPoisons()
    {
        return poisonListGameObject.transform.childCount;
    }

    private Fruit InstantiateNewFruit(Fruit targetPrefab, GameObject parent)
    {
        //var targetPrefab = 0.5 > Random.Range(0, 1) ? foodPrefab : poisonPrefab; 

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