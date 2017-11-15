using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles and applies steering forces to this GameObject based on the model of a zombie pursing humans.
/// </summary>
public class Zombie : Vehicle
{
    public Vehicle PursueTarget { get; private set; }

    /// <summary>
    /// Draw an additional debug line of the human this zombie is pursuing, and this zombie's future position.
    /// </summary>
    protected override void DrawDebugLines()
    {
        base.DrawDebugLines();
        //Raise line so it doesn't become hidden in the terrain.
        Vector3 yOff = new Vector3(0, 1, 0);
        if (PursueTarget != null)
            debugLineRenderer.DrawLine(2, transform.position + yOff, PursueTarget.transform.position + yOff);
        //Draw future position
        debugLineRenderer.SetShapeLocation(transform.position + Velocity * pursueInfo.secondsAhead);
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

    /// <summary>
    /// When a collision with a human or controllable human occurs, tell the GameManager to kill it.
    /// This method is invoked by the custom Collider class.
    /// </summary>
    /// <param name="other">Object which has been collided with</param>
    private void CollisionStarted(Object other)
    {
        Collider coll = (Collider)other;
        if (coll.GetComponent<Human>() || coll.GetComponent<ControllableHuman>())
        {
            GameManager.Instance.RequestAgentRemoval(coll.gameObject);
            GameManager.Instance.RequestAgentSpawn<Zombie>(coll.transform.position);
        }
    }
}
