using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : Vehicle
{
    public float fleeWeight;
    public float minZombieDistance = 6;
    public Zombie FleeTarget { get; private set; }

    /// <summary>
    /// Make the human seek the seekTarget and flee the fleeTarget if too close
    /// </summary>
    protected override void CalcSteeringForces()
    {
        Vector3 netForce = Vector3.zero;
        Vector3 flee = Vector3.zero;

        FleeTarget = GetNearest<Zombie>();

        float sqrMag = (transform.position - FleeTarget.transform.position).sqrMagnitude;
        if (sqrMag < Mathf.Pow(minZombieDistance, 2))
        {
            flee = Flee(FleeTarget.transform.position) * fleeWeight;
        }

        netForce = flee;
        netForce = Vector3.ClampMagnitude(netForce, maxForce);
        ApplyForce(netForce);
    }
}
