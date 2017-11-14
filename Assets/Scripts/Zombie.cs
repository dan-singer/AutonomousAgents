using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : Vehicle
{
    public Vehicle PursueTarget { get; private set; }

    protected override void DrawDebugLines()
    {
        base.DrawDebugLines();
        Vector3 yOff = new Vector3(0, 1, 0);
        if (PursueTarget != null)
            debugLineRenderer.DrawLine(2, transform.position + yOff, PursueTarget.transform.position + yOff);
    }

    /// <summary>
    /// Determines and applies steering forces for this zombie.
    /// </summary>
    protected override void CalcSteeringForces()
    {
        Vector3 netForce = Vector3.zero;

        PursueTarget = GetNearest(GameManager.Instance.Humans);
        if (PursueTarget != null)
            netForce += Pursue(PursueTarget, pursueInfo.secondsAhead) * pursueInfo.weight;
        else
            netForce += Wander(wanderInfo.unitsAhead, wanderInfo.radius) * wanderInfo.weight;

        //Obstacle avoidance
        foreach (GameObject obstacle in GameManager.Instance.Obstacles)
        {
            netForce += Avoid(obstacle, avoidInfo.radius) * avoidInfo.weight;
        }
        //Constrain to bounds
        netForce += ConstrainTo(GameManager.Instance.floor.bounds) * constrainInfo.weight;

        //Separation
        netForce += Separate(GameManager.Instance.Zombies, separationInfo.radius) * separationInfo.weight;
        netForce = Vector3.ClampMagnitude(netForce, maxForce);
        ApplyForce(netForce);
    }   

    private void CollisionStarted(Object other)
    {
        Collider coll = (Collider)other;
        if (coll.GetComponent<Human>() || coll.GetComponent<ControllableHuman>())
        {
            GameManager.Instance.RequestAgentRemoval(coll.gameObject);
            GameManager.Instance.RequestAgentSpawn<Zombie>(transform.position);
        }
    }
}
