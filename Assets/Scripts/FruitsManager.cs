using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class FruitsManager : MonoBehaviour
{
    public LayerMask obstacleMask;
    public Fruit fruitPrefab;
    public Fruit poisonPrefab;

    public int fruitMaxQuantity;


    public GameObject foodListGameObject;
    public GameObject poisonListGameObject;

    public Toggle resetToggle;
    public Agent agent;
    private int resetCount = 1;

    private void Update()
    {
        if (QuantityFruits() < fruitMaxQuantity)
        {
            InstantiateNewFruit();
        }

        if (agent.stepNumber > (5000 * resetCount) && resetToggle.isOn)
        {
            resetCount++;
            ResetFruits();
        }
    }

    private int QuantityFruits()
    {
        return foodListGameObject.transform.childCount + poisonListGameObject.transform.childCount;
    }

    public void ResetFruits()
    {
        List<Transform> foods = foodListGameObject.transform.Cast<Transform>().ToList();
        List<Transform> poisons = poisonListGameObject.transform.Cast<Transform>().ToList();

        foreach (Transform food in foods)
        {
            food.GetComponent<Fruit>().DestroyFood(0f);
        }

        foreach (Transform poison in poisons)
        {
            poison.GetComponent<Fruit>().DestroyFood(0f);
        }
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