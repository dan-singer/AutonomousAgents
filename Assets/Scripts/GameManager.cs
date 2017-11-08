using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the HvZ simulation
/// </summary>
/// <author>Dan Singer</author>
public class GameManager : MonoBehaviour {


    //Singleton pattern
    private static GameManager instance;
    /// <summary>
    /// Singleton
    /// </summary>
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<GameManager>();
            return instance;
        }
    }


    [System.Serializable]
    public class SpawnInfo
    {
        public int Min, Max;
    }

    public GameObject humanPrefab;
    public GameObject zombiePrefab;
    public GameObject obstaclePrefab;
    public MeshRenderer floor;

    public SpawnInfo humanSpawnInfo;
    public SpawnInfo zombieSpawnInfo;
    public SpawnInfo obstacleSpawnInfo;


    public List<GameObject> AllActors { get; private set; }
    public List<Human> Humans { get; private set; }
    public List<Zombie> Zombies { get; private set; }
    public List<GameObject> Obstacles { get; private set; }

    // Use this for initialization
    void Start()
    {
        SpawnAgents();
    }

    private void SpawnAgents()
    {
        AllActors = new List<GameObject>(); Humans = new List<Human>(); Zombies = new List<Zombie>(); Obstacles = new List<GameObject>();

        int humanCount = Random.Range(humanSpawnInfo.Min, humanSpawnInfo.Max + 1);
        for (int i = 0; i < humanCount; i++)
        {
            GameObject human = SpawnOnFloor(humanPrefab);
            Humans.Add(human.GetComponent<Human>());
        }
        int zombieCount = Random.Range(zombieSpawnInfo.Min, zombieSpawnInfo.Max + 1);
        for (int i=0; i<zombieCount; i++)
        {
            GameObject zombie = SpawnOnFloor(zombiePrefab);
            Zombies.Add(zombie.GetComponent<Zombie>());
        }
        int obstacleCount = Random.Range(obstacleSpawnInfo.Min, obstacleSpawnInfo.Max + 1);
        for (int i=0; i<obstacleCount; i++)
        {
            GameObject obstacle = SpawnOnFloor(obstaclePrefab);
            Obstacles.Add(obstacle);
        }
    }

    /// <summary>
    /// Spawn an instance of original on a random position on the floor
    /// </summary>
    private GameObject SpawnOnFloor(GameObject original)
    {
        GameObject newGo = Instantiate<GameObject>(original, RandomPosOnFloor(original), Quaternion.identity);
        AllActors.Add(newGo);
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
