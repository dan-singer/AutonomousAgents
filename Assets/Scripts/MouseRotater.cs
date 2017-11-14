using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseRotater : MonoBehaviour {

    public Vector3 pivot = Vector3.zero;
    public float angularSpeed = 4;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
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
