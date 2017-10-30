using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour {

    public enum ForceType
    {
        Seeking,
        Fleeing
    }

    //Vectors for force-based movement
    public Vector3 acceleration, velocity, vehiclePosition;
    public Vector3 direction;

    private const float NORMAL_FORCE_MAGNITUDE = 1;

    //Floats for force-based movement
    public float mass;
    public float maxSpeed;
    public float maxForce;
    public float radius;

    public bool bounce = true;

    private ForceType curForceType;
    public ForceType CurForceType
    {
        get
        {
            return curForceType;
        }
        set
        {
            curForceType = value;
            if (ForceTypeChanged != null)
                ForceTypeChanged(curForceType);
        }
    }
    public Transform target;

    //Events
    public event Action<ForceType> ForceTypeChanged;

    private SpriteRenderer spriteRenderer;

    // Use this for initialization
    void Start () {
        vehiclePosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Applies a force to this vehicle's acceleration.
    /// </summary>
    /// <param name="force">Force to apply</param>
    public void ApplyForce(Vector3 force)
    {
        force.z = 0;
        acceleration += (force / mass);
    }

    /// <summary>
    /// Apply a friction force 
    /// </summary>
    public void ApplyFriction(float frictionCoeff)
    {
        Vector3 friction = frictionCoeff * NORMAL_FORCE_MAGNITUDE * -velocity.normalized;
        ApplyForce(friction);
    }

    //public void Bounce()
    //{
    //    Vector3 normalDirection = Vector3.zero;
    //    float significantMagnitude = 0;
    //    if (vehiclePosition.x > AgentManager.MaxScreenPt.x - spriteRenderer.bounds.extents.x)
    //    {
    //        normalDirection = -Vector3.right;
    //        significantMagnitude = velocity.x;
    //    }
    //    if (vehiclePosition.x < AgentManager.MinScreenPt.x + spriteRenderer.bounds.extents.x)
    //    {
    //        normalDirection = Vector3.right;
    //        significantMagnitude = velocity.x;

    //    }
    //    if (vehiclePosition.y > AgentManager.MaxScreenPt.y - spriteRenderer.bounds.extents.y)
    //    {
    //        normalDirection = -Vector3.up;
    //        significantMagnitude = velocity.y;
    //    }
    //    if (vehiclePosition.y < AgentManager.MinScreenPt.y + spriteRenderer.bounds.extents.y)
    //    {
    //        normalDirection = Vector3.up;
    //        significantMagnitude = velocity.y;
    //    }
    //    if (normalDirection == Vector3.zero)
    //        return;
    //    significantMagnitude = Mathf.Abs(significantMagnitude);
    //    //We divide by Time.deltaTime so velocity will be it's opposite at the end of the frame.
    //    ApplyForce(normalDirection * mass * significantMagnitude * 2 / (Time.deltaTime));

    //}

    /// <summary>
    /// Seek method
    /// </summary>
    /// <returns>Seek force vector</returns>
    public Vector3 Seek(Vector3 target)
    {
        //Desired vel = target's position - my position
        Vector3 desiredVel = target - transform.position;
        //Scale desired to max speed
        desiredVel = desiredVel.normalized * maxSpeed;
        //Steering force = desired vel - current vel
        Vector3 steerForce = desiredVel - velocity;
        //return steering force
        return steerForce;
    }

    /// <summary>
    /// Get a seeking force away from target
    /// </summary>
    public Vector3 Flee(Vector3 target)
    {
        Vector3 desiredVel = transform.position - target;
        desiredVel = desiredVel.normalized * maxSpeed;
        return (desiredVel - velocity);
    }

    private void RotateTowardsDirection()
    {
        float angle = Mathf.Atan2(direction.y, direction.x);
        transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
    }

	// Update is called once per frame
	void LateUpdate () {

        //Update this in case position changed from somewhere else.
        vehiclePosition = transform.position;

        //if (bounce)
        //    Bounce();

        switch (CurForceType)
        {
            case ForceType.Seeking:
                ApplyForce(Seek(target.position));
                break;
            case ForceType.Fleeing:
                ApplyForce(Flee(target.position));
                break;
            default:
                break;
        }

        RotateTowardsDirection();

        //New "movement formula"
        velocity += acceleration * Time.deltaTime;
        vehiclePosition += velocity * Time.deltaTime;
        //Get normalized velocity as direction
        direction = velocity.normalized;
        transform.position = vehiclePosition;
        //Reset acceleration
        acceleration = Vector3.zero;
	}
}
