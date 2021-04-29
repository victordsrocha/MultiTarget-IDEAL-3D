using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Energy : MonoBehaviour
{
    public double currentEnergy;
    private Player _player;

    public float period = 0.5f;
    private float time = 0.0f;

    private void Awake()
    {
        _player = transform.GetComponent<Player>();
    }

    private void Start()
    {
        currentEnergy = 50000;
    }

    private void LateUpdate()
    {
        time += Time.deltaTime;
        if (time >= period)
        {
            time = 0.0f;
            currentEnergy -= EnergyDecay();
        }
    }

    double EnergyDecay()
    {
        double motorsStrength = Math.Pow(10 * Math.Abs(_player.change.x) + 10 * Math.Abs(_player.change.y), 2);
        double w = 0; // um of the activation values of all internal neurons in each time step;
        double constant = 10;

        return motorsStrength + w + constant;
    }
}