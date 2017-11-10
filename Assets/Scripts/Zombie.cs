using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : Vehicle
{
    public Vehicle PursueTarget { get; private set; }
    public float pursueWeight;
    public float avoidWeight = 3;
    public float avoidRadius = 4;
    public float pursueSecondsAhead = 2;
    public float constrainWeight = 4;


    protected override void DrawDebugLines()
    {
        base.DrawDebugLines();
        debugLineRenderer.DrawLine(2, transform.position, PursueTarget.transform.position);
    }


    protected override void CalcSteeringForces()
    {
        PursueTarget = GetNearest<Human>(GameManager.Instance.Humans);
        if (PursueTarget == null)
            return;
        Vector3 netForce = Vector3.zero;
        netForce += Pursue(PursueTarget, pursueSecondsAhead) * pursueWeight;

        //Obstacle avoidance
        foreach (GameObject obstacle in GameManager.Instance.Obstacles)
        {
            netForce += Avoid(obstacle, avoidRadius) * avoidWeight;
        }
        //Constrain to bounds
        netForce += ConstrainTo(GameManager.Instance.floor.bounds) * constrainWeight;

        netForce = Vector3.ClampMagnitude(netForce, maxForce);
        ApplyForce(netForce);
    }
}
