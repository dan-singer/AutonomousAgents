using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a collider that can be used for collision detection
/// </summary>
/// <author>Dan Singer</author>
[RequireComponent(typeof(Renderer))]
public class Collider : MonoBehaviour {

    public enum Method
    {
        AABB,
        BoundingCircles
    }

    public Method collisionMethod;

    public Renderer Rend { get; private set; }

    private HashSet<Collider> collidingWith;

    /// <summary>
    /// These will be broadcast at various stages of the collision.
    /// </summary>
    private static string collisionMessage = "CollisionOccurring";
    private static string collisionStartedMessage = "CollisionStarted";
    private static string collisionEndedMessage = "CollisionEnded";

    /// <summary>
    /// Radius of a sphere encapsulating the mesh.
    /// </summary>
    public float Radius
    {
        get
        {
            return Mathf.Max(Rend.bounds.extents.x, Rend.bounds.extents.y, Rend.bounds.extents.z);
        }
    }
    /// <summary>
    /// Radius of a sphere that is contained in the mesh.
    /// </summary>
    public float InnerRadius
    {
        get
        {
            return Mathf.Min(Rend.bounds.extents.x, Rend.bounds.extents.y, Rend.bounds.extents.z);
        }
    }

    // Use this for initialization
    void Start() {
        Rend = GetComponent<Renderer>();
        collidingWith = new HashSet<Collider>();
    }

    /// <summary>
    /// Loop through each collider, and perform a collision check on it if it hasn't been performed yet this frame.
    /// </summary>
    void Update() {

        foreach (Collider coll in CollisionManager.AllColliders)
        {
            bool shouldNotCheck = coll == null || coll == this || CollisionManager.WasCollCheckAlreadyPerformed(this, coll);
            if (shouldNotCheck)
                continue;

            Debug.Assert(coll != null);

            bool collided;
            if (collisionMethod == Method.AABB)
                collided = AABB(coll);
            else
                collided = BoundingCircle(coll);

            CollisionManager.ReportCollisionCheckThisFrame(this, coll);

            if (collided)
            {
                //Collision began
                if (!collidingWith.Contains(coll))
                {
                    collidingWith.Add(coll);
                    BroadcastCollisionMessage(collisionStartedMessage, coll);
                }
                //Collision occuring
                BroadcastCollisionMessage(collisionMessage,coll);
            }
            else
            {
                //Collision just ended
                if (collidingWith.Contains(coll))
                {
                    collidingWith.Remove(coll);
                    BroadcastCollisionMessage(collisionEndedMessage, coll);
                }
            }
        }
    }

    /// <summary>
    /// Broadcast a message to this MonoBehaviour and to the collider we collided with.
    /// </summary>
    /// <param name="msg">Collision message. See collisionMessage fields.</param>
    /// <param name="other">Other collider we collided with.</param>
    private void BroadcastCollisionMessage(string msg, Collider other)
    {
        BroadcastMessage(msg, other, SendMessageOptions.DontRequireReceiver);
        other.BroadcastMessage(msg, this, SendMessageOptions.DontRequireReceiver);

    }

    /// <summary>
    /// See if there is a collision using Axis Aligned Bounding Box Collision Test
    /// </summary>
    /// <param name="other">Other collider to check against</param>
    /// <returns>True if collision, false otherwise</returns>
    private bool AABB(Collider other)
    {
        bool test = Rend.bounds.max.x > other.Rend.bounds.min.x
            && Rend.bounds.min.x < other.Rend.bounds.max.x
            && Rend.bounds.max.y > other.Rend.bounds.min.y
            && Rend.bounds.min.y < other.Rend.bounds.max.y;

        return test;
    }
    /// <summary>
    /// See if there is a collision using Bounding Circle Collision Test
    /// </summary>
    /// <param name="other">Other collider to check against</param>
    /// <returns>True if collision, false otherwise</returns>
    private bool BoundingCircle(Collider other)
    {
        Vector3 line = other.Rend.bounds.center - Rend.bounds.center;
        float radSum = Radius + other.Radius;
        bool test = line.sqrMagnitude < Mathf.Pow(radSum, 2);
        return test;
    }
}
