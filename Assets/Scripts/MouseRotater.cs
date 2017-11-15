using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component which causes this GameObject to rotate around the pivot about the y-axis when the mouse is moved along the x-axis.
/// </summary>
/// <author>Dan Singer</author>
public class MouseRotater : MonoBehaviour {

    public Vector3 pivot = Vector3.zero;
    public float angularSpeed = 4;

	/// <summary>
    /// Rotate and move this around the pivot about the y-axis when mouse is moved along x-axis.
    /// </summary>
	void Update () {
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation((pivot - transform.position).normalized), angularSpeed * Time.deltaTime);
		if (Input.GetMouseButton(0))
        {
            float mx = Input.GetAxis("Mouse X") * angularSpeed * Time.deltaTime;
            //Position rotation
            Vector3 fromPivot = transform.position - pivot;
            fromPivot = Quaternion.Euler(0, mx, 0) * fromPivot;
            transform.position = pivot + fromPivot;
        }
	}
}
