using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Third person controller which extends the Vehicle class.
/// </summary>
/// <author>Dan Singer</author>
public class ControllableHuman : Vehicle {

    private bool wasPressingBack = false;
    private bool wasMovingHorz = false;
    private Vector3 fwd;
    private Vector3 right;

    public float invincibilityDuration = 1;

    private Animator animator;
    /// <summary>
    /// Generate and apply forces to steer the human based on player input.
    /// </summary>
    protected override void CalcSteeringForces()
    {
        float horz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");

        //NOTE: these following statements seem odd, but they're valid.
        //If I'm pressing down and I wasn't before, cache the fwd vector so I don't keep flipping directions
        if (vert < 0 && !wasPressingBack)
            fwd = transform.forward;
        //Otherwise, assuming I'm now going forward again, keep continually updating fwd vector
        else if (vert > 0)
            fwd = transform.forward;

        //If I JUST pressed left or right and I'm not moving forward, cache the right vector so I don't spin in circles, but rather, move in a line
        if (vert == 0 && horz != 0 && !wasMovingHorz)
            right = transform.right;
        //Otherwise, assuming I did stop moving horizontally or I did begin moving forward
        else if (vert != 0 || horz == 0)
            right = transform.right;


        Vector3 fwdOffset = fwd * vert;
        Vector3 horzOffset = right * horz;
        //Seek the position ahead of me based on input
        Vector3 seekPt = transform.position + horzOffset + fwdOffset;

        if (horz == 0 && vert == 0)
            ApplyForce(-Velocity * .5f * mass / Time.deltaTime);
        else
            ApplyForce(Seek(seekPt));


        //Animation
        animator.SetFloat("Speed", Velocity.magnitude);

        wasPressingBack = Input.GetAxis("Vertical") < 0;
        wasMovingHorz = Input.GetAxis("Horizontal") != 0;
    }

    /// <summary>
    /// The controllable human should not draw debug lines.
    /// </summary>
    protected override void DrawDebugLines()
    {
    }

    /// <summary>
    /// Initialize the ControllableHuman.
    /// </summary>
    protected override void Start ()
    {
        base.Start();
        animator = GetComponent<Animator>();
	}

    /// <summary>
    /// Disable the collider, wait a period of time, then enable the collider.
    /// </summary>
    public void ToggleCollider()
    {
        StartCoroutine(ToggleColliderIE());
    }

    /// <summary>
    /// Handle timing and logic for ToggleCollider method.
    /// </summary>
    private IEnumerator ToggleColliderIE()
    {
        //Do this in case start was not called yet
        if (coll == null) coll = GetComponent<Collider>();
        coll.enabled = false;
        CollisionManager.Instance.UpdateAllColliders();
        yield return new WaitForSeconds(invincibilityDuration);
        coll.enabled = true;
        CollisionManager.Instance.UpdateAllColliders();

    }
}
