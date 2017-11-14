using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Human : Vehicle
{
    public float minZombieDistance = 6;
    public Vehicle FleeTarget { get; private set; }


    /// <summary>
    /// Make the human seek the seekTarget and flee the fleeTarget if too close
    /// </summary>
    protected override void CalcSteeringForces()
    {
        Vector3 netForce = Vector3.zero;
        Vector3 flee = Vector3.zero;

        FleeTarget = GetNearest(GameManager.Instance.Zombies);

        //Only flee when closer than minZombieDistance
        float sqrMag = (transform.position - FleeTarget.transform.position).sqrMagnitude;
        if (sqrMag < Mathf.Pow(minZombieDistance, 2))
        {
            netForce += Evade(FleeTarget, evadeInfo.secondsAhead) * evadeInfo.weight;
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
