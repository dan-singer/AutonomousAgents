using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

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


    private Queue<Action> lateUpdateQueue; 

    // Use this for initialization
    void Start()
    {
        SpawnAgents();
        lateUpdateQueue = new Queue<Action>();
        lateUpdateQueue.Enqueue(() => { DebugLineRenderer.Draw = false; });
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

        CollisionManager.Instance.UpdateAllColliders();
    }

    /// <summary>
    /// Spawn an instance of original on a random position on the floor
    /// </summary>
    private GameObject SpawnOnFloor(GameObject original, Vector3? loc = null)
    {
        Vector3 spawnLoc = loc == null ? RandomPosOnFloor(original) : loc.Value;
        GameObject newGo = Instantiate<GameObject>(original, spawnLoc, Quaternion.identity);
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

    private Vector3 SampleHeight(GameObject target, Vector3 orig)
    {
        orig.y = floor.bounds.max.y + target.GetComponent<Renderer>().bounds.extents.y;
        return orig;
    }

    private void RemoveAgent(GameObject agent)
    {
        Human h = agent.GetComponent<Human>();
        Zombie z = agent.GetComponent<Zombie>();
        if (h != null)
            Humans.Remove(h);
        if (z != null)
            Zombies.Remove(z);
        AllActors.Remove(agent);
        Destroy(agent);
        CollisionManager.Instance.UpdateAllColliders();
    }

    private void SpawnAgent<T>(Vector3 loc) where T: Vehicle
    {
        if (typeof(T) == typeof(Human))
        {
            GameObject humanGO = SpawnOnFloor(humanPrefab, loc); //Note that this will add to AllActors list
            Humans.Add(humanGO.GetComponent<Human>());
        }
        else if (typeof(T) == typeof(Zombie))
        {
            GameObject zombieGO = SpawnOnFloor(zombiePrefab, loc); //Note that this will add to AllActors list
            Zombies.Add(zombieGO.GetComponent<Zombie>());
        }
        CollisionManager.Instance.UpdateAllColliders();
    }

    //NOTE: Requests are done like because we must wait for all list iterations from vehicles to be over before modifying the lists!

    /// <summary>
    /// Request that an agent be spawned in the lateupdate function
    /// </summary>
    /// <typeparam name="T">Type to spawn</typeparam>
    /// <param name="loc">Where to spawn</param>
    public void RequestAgentSpawn<T>(Vector3 loc) where T: Vehicle
    {
        lateUpdateQueue.Enqueue(() => { SpawnAgent<T>(loc); }); 
    }
    /// <summary>
    /// Request that an agent be removed in the lateupdate function
    /// </summary>
    /// <param name="agent">Agent to remove</param>
    public void RequestAgentRemoval(GameObject agent)
    {
        lateUpdateQueue.Enqueue(() => { RemoveAgent(agent); });
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            DebugLineRenderer.Draw = !DebugLineRenderer.Draw;
        }
    }

    private void LateUpdate()
    {
        while (lateUpdateQueue.Count > 0)
        {
            Action action = lateUpdateQueue.Dequeue();
            action();
        }
    }
}
