using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : Vehicle
{
    public Vehicle PursueTarget { get; private set; }
    public float pursueWeight;
    public float avoidWeight = 1;
    public float avoidRadius = 4;
    public float pursueSecondsAhead = 2;
    public float constrainWeight = 1;
    public float separationRadius = 4;
    public float separationWeight = 0.5f;

    protected override void DrawDebugLines()
    {
        base.DrawDebugLines();
        debugLineRenderer.DrawLine(2, transform.position, PursueTarget.transform.position);
    }

    /// <summary>
    /// Determines and applies steering forces for this zombie.
    /// </summary>
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

        //Separation
        netForce += Separate<Zombie>(GameManager.Instance.Zombies, separationRadius) * separationWeight;
        netForce = Vector3.ClampMagnitude(netForce, maxForce);
        ApplyForce(netForce);
    }

    private void CollisionStarted(Object other)
    {
        Collider coll = (Collider)other;
        print("Collision started");
        if (coll.GetComponent<Human>())
        {
            Human human = coll.GetComponent<Human>();
            GameManager.Instance.RemoveAgent(human.gameObject);
            GameManager.Instance.SpawnAgent<Zombie>(transform.position);
        }
    }
}
