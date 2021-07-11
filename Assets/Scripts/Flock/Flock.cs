using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Flock : MonoBehaviour
{
    // Fruits Manager

    public LayerMask obstacleMask;
    public FlockAgent fruitPrefab;

    public GameObject fruitListGameObject;

    // Flock

    // public FlockAgent agentPrefab;
    public FlockBehavior behavior;

    [Range(1, 50)] public int startingCount = 10;

    [Range(1f, 100f)] public float driveFactor = 10f;
    [Range(0f, 100f)] public float maxSpeed = 0.1f;
    [Range(1f, 10f)] public float neighborRadius = 1.5f;
    [Range(0f, 1f)] public float avoidanceRadiusMultiplier = 0.5f;

    private float _squareMaxSpeed;
    private float _squareNeighborRadius;
    private float _squareAvoidanceRadius;
    public float SquareAvoidanceRadius => _squareAvoidanceRadius;

    // Start is called before the first frame update
    private void Start()
    {
        _squareMaxSpeed = maxSpeed * maxSpeed;
        _squareNeighborRadius = neighborRadius * neighborRadius;
        _squareAvoidanceRadius = _squareNeighborRadius * avoidanceRadiusMultiplier * avoidanceRadiusMultiplier;

        for (int i = 0; i < startingCount; i++)
        {
            FlockAgent newAgent = InstantiateNewFlockAgent(fruitPrefab, fruitListGameObject);
        }
    }

    private void Update()
    {
        if (CurrentQuantityFruits() < startingCount)
        {
            InstantiateNewFlockAgent(fruitPrefab, fruitListGameObject);
        }
    }

    private int CurrentQuantityFruits()
    {
        return fruitListGameObject.transform.childCount;
    }

    FlockAgent InstantiateNewFlockAgent(FlockAgent targetPrefab, GameObject parent)
    {
        //var targetPrefab = 0.5 > Random.Range(0, 1) ? foodPrefab : poisonPrefab; 

        FlockAgent newTarget = null;

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


        if (newTarget is { })
        {
            newTarget.flock = this;
            newTarget.behavior = behavior;
        }

        return newTarget;
    }
}