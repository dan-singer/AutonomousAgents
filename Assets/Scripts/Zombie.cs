using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : Vehicle
{
    public GameObject SeekTarget { get; private set; }
    public float seekWeight;
    public float avoidWeight = 3;
    public float avoidRadius = 4;


    protected override void DrawDebugLines()
    {
        base.DrawDebugLines();
        debugLineRenderer.DrawLine(2, transform.position, SeekTarget.transform.position);
    }


    protected override void CalcSteeringForces()
    {
        SeekTarget = GetNearest<Human>(GameManager.Instance.Humans).gameObject;
        if (SeekTarget == null)
            return;
        Vector3 netForce = Vector3.zero;
        netForce += Seek(SeekTarget.transform.position) * seekWeight;

        //Obstacle avoidance
        foreach (GameObject obstacle in GameManager.Instance.Obstacles)
        {
            netForce += Avoid(obstacle, avoidRadius) * avoidWeight;
        }

        netForce = Vector3.ClampMagnitude(netForce, maxForce);
        ApplyForce(netForce);
    }
}
