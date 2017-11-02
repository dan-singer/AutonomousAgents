using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Vehicle : MonoBehaviour {

    //Vectors for force-based movement
    public Vector3 Acceleration { get; private set; }
    public Vector3 Velocity { get; private set; }
    public Vector3 Direction { get; private set; }

    private const float NORMAL_FORCE_MAGNITUDE = 1;

    //Floats for force-based movement
    public float mass;
    public float maxSpeed;
    public float maxForce;
    public float radius;

    // Use this for initialization
    protected virtual void Start () {
    }

    /// <summary>
    /// Return the closes object of type T to this Vehicle
    /// </summary>
    protected T GetNearest<T>() where T:MonoBehaviour
    {
        T[] objects = FindObjectsOfType<T>();
        if (objects.Length == 0)
            return default(T);
        T min = objects[0];
        for (int i = 1; i < objects.Length; i++)
        {
            float minDistSqr = (transform.position - min.transform.position).sqrMagnitude;
            float distSqr = (transform.position - objects[i].transform.position).sqrMagnitude;
            if (distSqr < minDistSqr)
            {
                min = objects[i];
            }
        }
        return min;
    }



    /// <summary>
    /// Applies a force to this vehicle's acceleration.
    /// </summary>
    /// <param name="force">Force to apply</param>
    public void ApplyForce(Vector3 force)
    {
        Acceleration += (force / mass);
    }

    /// <summary>
    /// Apply a friction force 
    /// </summary>
    protected void ApplyFriction(float frictionCoeff)
    {
        Vector3 friction = frictionCoeff * NORMAL_FORCE_MAGNITUDE * -Velocity.normalized;
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
    protected Vector3 Seek(Vector3 target)
    {
        //Desired vel = target's position - my position
        Vector3 desiredVel = target - transform.position;
        //Scale desired to max speed
        desiredVel = desiredVel.normalized * maxSpeed;
        //Steering force = desired vel - current vel
        Vector3 steerForce = desiredVel - Velocity;
        //return steering force
        return steerForce;
    }

    /// <summary>
    /// Get a seeking force away from target
    /// </summary>
    protected Vector3 Flee(Vector3 target)
    {
        Vector3 desiredVel = transform.position - target;
        desiredVel = desiredVel.normalized * maxSpeed;
        return (desiredVel - Velocity);
    }

    /// <summary>
    /// Set the GameObject's forward to the current Direction
    /// </summary>
    private void SetTransform()
    {
    
    }

    /// <summary>
    /// Calculate the steering forces for this vehicle
    /// </summary>
    protected abstract void CalcSteeringForces();

    /// <summary>
    /// Calculate velocity and then position from the acceleration derived from forces this frame
    /// </summary>
    private void UpdatePosition()
    {
        //New "movement formula"
        Velocity += Acceleration * Time.deltaTime;
        transform.position += Velocity * Time.deltaTime;
        //Get normalized velocity as direction
        Direction = Velocity.normalized;
        //Reset acceleration
        Acceleration = Vector3.zero;
    }

	// Update is called once per frame
	protected void LateUpdate () {

        CalcSteeringForces();
        UpdatePosition();
        SetTransform();
	}
}
