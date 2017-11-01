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

    public int humansToSpawnMin = 5, humansToSpawnMax = 10;

    private GameObject[] humans;
    private GameObject zombie;
    private GameObject target;

    private float targetRad;
    private float humanRad;


	// Use this for initialization
	void Start () {
        
        zombie = SpawnOnFloor(zombiePrefab);
        target = SpawnOnFloor(targetPrefab);

        int humanCount = Random.Range(humansToSpawnMin, humansToSpawnMax + 1);
        humans = new GameObject[humanCount];
        for (int i=0; i<humanCount; i++)
        {
            humans[i] = SpawnOnFloor(humanPrefab);
            humans[i].GetComponent<Human>().fleeTarget = zombie;
            humans[i].GetComponent<Human>().seekTarget = target;
        }
        

        targetRad = target.GetComponent<MeshRenderer>().bounds.extents.magnitude;
        humanRad  = humanPrefab.GetComponent<MeshRenderer>().bounds.extents.magnitude;
	}

    /// <summary>
    /// Spawn an instance of original on a random position on the floor
    /// </summary>
    private GameObject SpawnOnFloor(GameObject original)
    {
        GameObject newGo = Instantiate<GameObject>(original, RandomPosOnFloor(original), Quaternion.identity);
        return newGo;
    }

    /// <summary>
    /// Get a random position on the floor such that the gameobject will be on top of the floor
    /// </summary>
    private Vector3 RandomPosOnFloor(GameObject original)
    {
        Vector3 spawnLoc = new Vector3(
            Random.Range(floor.bounds.min.x, floor.bounds.max.x),
            floor.bounds.max.y + original.GetComponent<Renderer>().bounds.extents.y,
            Random.Range(floor.bounds.min.z, floor.bounds.max.z)
        );

        return spawnLoc;
    }


    // Update is called once per frame
    void Update () {
        //Make target move when near a human using a quick bounding sphere check
		foreach (GameObject human in humans)
        {
            float distSqr = (target.transform.position - human.transform.position).sqrMagnitude;
            if (distSqr < Mathf.Pow(humanRad + targetRad, 2) )
            {
                target.transform.position = RandomPosOnFloor(target);
            }
        }
	}
}
