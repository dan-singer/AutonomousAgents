using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : Vehicle
{
    public GameObject seekTarget;
    public float seekWeight;

    protected override void CalcSteeringForces()
    {
        Vector3 netForce = Vector3.zero;
        netForce += Seek(seekTarget.transform.position) * seekWeight;
        netForce = Vector3.ClampMagnitude(netForce, maxForce);
        ApplyForce(netForce);
    }
}
