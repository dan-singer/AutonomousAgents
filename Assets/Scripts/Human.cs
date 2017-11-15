using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles and applies steering forces to this GameObject based on the model of a human fleeing zombies.
/// </summary>
/// <author>Dan Singer</author>
[RequireComponent(typeof(Collider))]
public class Human : Vehicle
{
    public float minZombieDistance = 6;
    public Vehicle EvadeTarget { get; private set; }


    /// <summary>
    /// Apply steering forces to this human.
    /// </summary>
    protected override void CalcSteeringForces()
    {
        Vector3 netForce = Vector3.zero;
        Vector3 flee = Vector3.zero;

        EvadeTarget = GetNearest(GameManager.Instance.Zombies);

        //Only flee when closer than minZombieDistance
        float sqrMag = (transform.position - EvadeTarget.transform.position).sqrMagnitude;
        if (sqrMag < Mathf.Pow(minZombieDistance, 2))
        {
            netForce += Evade(EvadeTarget, evadeInfo.secondsAhead) * evadeInfo.weight;
        }
        else {
            netForce += Wander(wanderInfo.unitsAhead, wanderInfo.radius) * wanderInfo.weight;
        }
        //Obstacle avoidance
        foreach (GameObject obstacle in GameManager.Instance.Obstacles)
        {
            netForce += Avoid(obstacle, avoidInfo.radius) * avoidInfo.weight;
        }
        //Stay in park
        netForce += ConstrainTo(GameManager.Instance.floor.bounds) * constrainInfo.weight;
        //Separation
        netForce += Separate(GameManager.Instance.Humans, separationInfo.radius) * separationInfo.weight;
        netForce = Vector3.ClampMagnitude(netForce, maxForce);
        ApplyForce(netForce);
    }
}
