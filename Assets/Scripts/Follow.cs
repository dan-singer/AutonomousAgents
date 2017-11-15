using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component which causes this gameobject to follow the target.
/// </summary>
/// <author>Dan Singer</author>
public class Follow : MonoBehaviour {

    public Transform target;
    public Vector3 localOffset;
    public float smooth = 2;

    /// <summary>
    /// Contains logic to make camera smoothly follow and rotate in the direction of target.
    /// </summary>
    void LateUpdate()
    {
        if (!target)
            return;
        Vector3 globalOffset = target.TransformDirection(localOffset);
        transform.position = Vector3.Lerp(transform.position, target.transform.position + globalOffset, smooth * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.transform.forward), smooth * Time.deltaTime);
    }
}
