using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class FlockAgent : MonoBehaviour
{
    public Flock AgentFlock { get; private set; }
    public Flock flock;

    public int id;
    public FlockBehavior behavior;

    [Range(1f, 100f)] public float driveFactor = 5f;
    [Range(0f, 100f)] public float maxSpeed = .1f;
    [Range(1f, 10f)] public float neighborRadius = 1.5f;
    [Range(0f, 1f)] public float avoidanceRadiusMultiplier = 0.5f;

    private float _squareMaxSpeed;
    private float _squareNeighborRadius;
    private float _squareAvoidanceRadius;

    public Toggle frozenToggle;

    public float SquareAvoidanceRadius => _squareAvoidanceRadius;


    public Collider AgentCollider { get; private set; }
    private Rigidbody _rigidbody;

    // Start is called before the first frame update
    private void Start()
    {
        _squareMaxSpeed = maxSpeed * maxSpeed;
        _squareNeighborRadius = neighborRadius * neighborRadius;
        _squareAvoidanceRadius = _squareNeighborRadius * avoidanceRadiusMultiplier * avoidanceRadiusMultiplier;
        AgentCollider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!frozenToggle.isOn)
        {
            List<Transform> context = GetNearbyObjects(this);

            Vector3 move = behavior.CalculateMove(this, context, flock);

            move.y = 0;

            move *= driveFactor;
            if (move.sqrMagnitude > _squareMaxSpeed)
            {
                move = move.normalized * maxSpeed;
            }

            Move(move);
        }
        else
        {
            _rigidbody.velocity = Vector3.zero;
        }
    }

    public void Initialize(Flock flock)
    {
        AgentFlock = flock;
    }

    public void Move(Vector3 velocity)
    {
        transform.forward = velocity;
        transform.position += velocity * Time.deltaTime;

        //transform.forward = velocity;
        //_rigidbody.rotation = 
        //_rigidbody.velocity = velocity;
    }

    List<Transform> GetNearbyObjects(FlockAgent agent)
    {
        List<Transform> context = new List<Transform>();
        Collider[] contextColliders = Physics.OverlapSphere(agent.transform.position, neighborRadius);
        foreach (Collider c in contextColliders)
        {
            if (c != agent.AgentCollider)
            {
                context.Add(c.transform);
            }
        }

        return context;
    }
}