using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages Exercise 9: Code Refactoring
/// </summary>
public class ExerciseManager : MonoBehaviour {

    public GameObject humanPrefab;
    public GameObject zombiePrefab;
    public GameObject targetPrefab;
    public MeshRenderer floor;


	// Use this for initialization
	void Start () {
		
	}

    /// <summary>
    /// Spawn an instance of original on a random position on the floor
    /// </summary>
    private GameObject SpawnOnFloor(GameObject original)
    {
        Vector3 spawnLoc = new Vector3(
            Random.Range(floor.bounds.min.x, floor.bounds.max.x),
            floor.bounds.max.y + original.GetComponent<Renderer>().bounds.extents.y,
            Random.Range(floor.bounds.min.z, floor.bounds.max.z)
        );

        GameObject newGo = Instantiate<GameObject>(original, spawnLoc, Quaternion.identity);

        return newGo;

    }

	
	// Update is called once per frame
	void Update () {
		
	}
}
