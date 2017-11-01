using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : Vehicle
{
    public float seekWeight;
    public float fleeWeight;
    public float minZombieDistance = 6;
    public GameObject seekTarget;
    public GameObject fleeTarget;

    /// <summary>
    /// Make the human seek the seekTarget and flee the fleeTarget if too close
    /// </summary>
    protected override void CalcSteeringForces()
    {
        Vector3 netForce = Vector3.zero;
        Vector3 seek = Seek(seekTarget.transform.position) * seekWeight;
        Vector3 flee = Vector3.zero;

        float sqrMag = (transform.position - fleeTarget.transform.position).sqrMagnitude;
        if (sqrMag < Mathf.Pow(minZombieDistance, 2))
        {
            flee = Flee(fleeTarget.transform.position) * fleeWeight;
        }

        netForce = seek + flee;
        netForce = Vector3.ClampMagnitude(netForce, maxForce);
        ApplyForce(netForce);
    }
}
