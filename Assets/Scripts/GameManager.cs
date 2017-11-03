using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the HvZ simulation
/// </summary>
/// <author>Dan Singer</author>
public class GameManager : MonoBehaviour {

    [System.Serializable]
    public class SpawnInfo
    {
        public int Min, Max;
    }


    public GameObject humanPrefab;
    public GameObject zombiePrefab;
    public MeshRenderer floor;

    public SpawnInfo humanSpawnInfo;
    public SpawnInfo zombieSpawnInfo;

    private GameObject zombie;
    private GameObject target;

    private float targetRad;
    private float humanRad;


    // Use this for initialization
    void Start()
    {
        SpawnAgents();
    }

    private void SpawnAgents()
    {
        zombie = SpawnOnFloor(zombiePrefab);

        int humanCount = Random.Range(humanSpawnInfo.Min, humanSpawnInfo.Max + 1);
        for (int i = 0; i < humanCount; i++)
        {
            GameObject human = SpawnOnFloor(humanPrefab);
        }
        int zombieCount = Random.Range(zombieSpawnInfo.Min, zombieSpawnInfo.Max + 1);
        for (int i=0; i<zombieCount; i++)
        {
            GameObject zombie = SpawnOnFloor(zombiePrefab);
        }
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
    void Update()
    {
    }
}
