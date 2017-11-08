using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Human : Vehicle
{
    public float fleeWeight;
    public float avoidWeight = 3;
    public float minZombieDistance = 6;
    public float avoidRadius = 4;
    public Zombie FleeTarget { get; private set; }


    /// <summary>
    /// Make the human seek the seekTarget and flee the fleeTarget if too close
    /// </summary>
    protected override void CalcSteeringForces()
    {
        Vector3 netForce = Vector3.zero;
        Vector3 flee = Vector3.zero;

        FleeTarget = GetNearest<Zombie>(GameManager.Instance.Zombies);

        //Only flee when closer than minZombieDistance
        float sqrMag = (transform.position - FleeTarget.transform.position).sqrMagnitude;
        if (sqrMag < Mathf.Pow(minZombieDistance, 2))
        {
            netForce += Flee(FleeTarget.transform.position) * fleeWeight;
        }

        //Obstacle avoidance
        foreach (GameObject obstacle in GameManager.Instance.Obstacles)
        {
            netForce += Avoid(obstacle, avoidRadius) * avoidWeight;
        }
        netForce = Vector3.ClampMagnitude(netForce, maxForce);
        ApplyForce(netForce);
    }
}
