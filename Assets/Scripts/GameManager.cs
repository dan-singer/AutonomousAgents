using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// Manages the HvZ simulation
/// </summary>
/// <author>Dan Singer</author>
public class GameManager : MonoBehaviour {


    private static GameManager instance;
    /// <summary>
    /// Singleton patern
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


    /// <summary>
    /// Small class to group a min and max value. Used for spawning prefabs.
    /// </summary>
    [System.Serializable]
    public class SpawnInfo
    {
        public int Min, Max;
    }

    public GameObject humanPrefab;
    public GameObject zombiePrefab;
    public GameObject obstaclePrefab;
    public GameObject controllableHumanPrefab;
    public MeshRenderer floor;
    public Follow followCam;
    public Transform worldTarget;

    public SpawnInfo humanSpawnInfo;
    public SpawnInfo zombieSpawnInfo;
    public SpawnInfo obstacleSpawnInfo;

    public Button buttonEnter;
    public Text txtInstr;

    //Structures storing spawned objects
    public List<GameObject> AllActors { get; private set; }
    public List<Vehicle> Humans { get; private set; }
    public List<Vehicle> Zombies { get; private set; }
    public List<GameObject> Obstacles { get; private set; }
    public ControllableHuman ActiveControllableHuman { get; private set; }

    private const string INSTR_NORMAL = "<b>Click and drag</b> to rotate camera";
    private const string INSTR_CONTROLLABLE = "<b>W A S D</b> to move";

    //Stores a queue of Actions to be performed in the LateUpdate method.
    private Queue<Action> lateUpdateQueue;

    /// <summary>
    /// Initializes the simulation and spawns agents.
    /// </summary>
    void Start()
    {
        followCam.target = worldTarget;
        lateUpdateQueue = new Queue<Action>();
        
        lateUpdateQueue.Enqueue(() => { DebugLineRenderer.Draw = false; });
        txtInstr.text = INSTR_NORMAL;

        //The "Enter Simulation" button is easier to setup with code, as it's button text must be changed when it's clicked
        if (buttonEnter)
        {
            buttonEnter.onClick.AddListener(() => {
                if (ActiveControllableHuman != null)
                {
                    buttonEnter.transform.GetChild(0).GetComponent<Text>().text = "Enter Simulation";
                    RequestAgentRemoval(ActiveControllableHuman.gameObject);
                }
                else
                {
                    buttonEnter.transform.GetChild(0).GetComponent<Text>().text = "Exit Simulation";
                    RequestAgentSpawn<ControllableHuman>(Vector3.zero);
                }
            });
        }

        SpawnAgents();


    }


    /// <summary>
    /// Restart the simulation by deleting all actors (except the controllable human), and respawning them in random locations.
    /// </summary>
    public void RestartSim()
    {

        for (int i = AllActors.Count - 1; i >= 0; i--)
        {
            if (AllActors[i].GetComponent<ControllableHuman>())
                continue;
            Destroy(AllActors[i]);
        }
        if (ActiveControllableHuman)
            ActiveControllableHuman.ToggleCollider();
        SpawnAgents();
        lateUpdateQueue = new Queue<Action>();
        lateUpdateQueue.Enqueue(() => { DebugLineRenderer.Draw = DebugLineRenderer.Draw; });
    }

    /// <summary>
    /// Spawn all agents into the world. This does not spawn a ControllableHuman.
    /// </summary>
    private void SpawnAgents()
    {
        AllActors = new List<GameObject>(); Humans = new List<Vehicle>(); Zombies = new List<Vehicle>(); Obstacles = new List<GameObject>();

        int humanCount = Random.Range(humanSpawnInfo.Min, humanSpawnInfo.Max + 1);
        for (int i = 0; i < humanCount; i++)
        {
            SpawnAgent<Human>(null);
        }
        int zombieCount = Random.Range(zombieSpawnInfo.Min, zombieSpawnInfo.Max + 1);
        for (int i = 0; i < zombieCount; i++)
        {
            SpawnAgent<Zombie>(null);
        }
        int obstacleCount = Random.Range(obstacleSpawnInfo.Min, obstacleSpawnInfo.Max + 1);
        for (int i = 0; i < obstacleCount; i++)
        {
            SpawnObstacle();
        }

    }

    /// <summary>
    /// Simple method to spawn a zombie. Typically call with Unity UI.
    /// </summary>
    public void SpawnZombie()
    {
        RequestAgentSpawn<Zombie>(null);
    }
    ///<summary>
    /// Simple method to spawn a zombie. Typically call with Unity UI.
    ///</summary>
    public void SpawnHuman()
    {
        RequestAgentSpawn<Human>(null);
    }
    ///<summary>
    /// Toggle whether debug lines should be drawn. Typically call with Unity UI.
    ///</summary>
    public void ToggleDebugLines()
    {
        DebugLineRenderer.Draw = !DebugLineRenderer.Draw;
    }

    /// <summary>
    /// Spawn an obstacle into the world.
    /// </summary>
    public void SpawnObstacle()
    {
        GameObject obstacle = SpawnOnFloor(obstaclePrefab, centerPivot: false);
        Obstacles.Add(obstacle);
        CollisionManager.Instance.UpdateAllColliders();
    }

    /// <summary>
    /// Spawn an instance of original on a random position on the floor.
    /// </summary>
    private GameObject SpawnOnFloor(GameObject original, Vector3? loc = null, bool centerPivot=false)
    {
        Vector3 spawnLoc = loc == null ? RandomPosOnFloor(original, centerPivot) : loc.Value;
        GameObject newGo = Instantiate<GameObject>(original, spawnLoc, Quaternion.identity);
        AllActors.Add(newGo);
        return newGo;
    }

    /// <summary>
    /// Get a random position on the floor such that the gameobject will be on top of the floor.
    /// </summary>
    private Vector3 RandomPosOnFloor(GameObject original, bool centerPivot=true)
    {
        float y = centerPivot ? floor.bounds.max.y + original.GetComponent<Collider>().renderer.bounds.extents.y : floor.bounds.max.y;
        Vector3 spawnLoc = new Vector3(
            Random.Range(floor.bounds.min.x, floor.bounds.max.x),
            y,
            Random.Range(floor.bounds.min.z, floor.bounds.max.z)
        );

        return spawnLoc;
    }

    /// <summary>
    /// Remove an agent from the simulation which has been spawned in by the GameManager.
    /// </summary>
    /// <param name="agent">Agent to remove</param>
    private void RemoveAgent(GameObject agent)
    {
        Human h = agent.GetComponent<Human>();
        ControllableHuman ch = agent.GetComponent<ControllableHuman>();
        Zombie z = agent.GetComponent<Zombie>();
        if (h != null)
            Humans.Remove(h);
        if (ch != null)
        {
            ActiveControllableHuman = null;
            Humans.Remove(ch); //It was stored here so zombies can track it, so we can remove it now
            worldTarget.GetComponent<MouseRotater>().enabled = true;
            followCam.target = worldTarget;
            buttonEnter.transform.GetChild(0).GetComponent<Text>().text = "Enter Simulation";
            txtInstr.text = INSTR_NORMAL;
        }
        if (z != null)
            Zombies.Remove(z);
        AllActors.Remove(agent);
        Destroy(agent);
        CollisionManager.Instance.UpdateAllColliders();
    }

    /// <summary>
    /// Spawn an agent into the simulation.
    /// </summary>
    /// <typeparam name="T">Type of agent to spawn, which must be a Vehicle</typeparam>
    /// <param name="loc">Nullable vector3 for location. If null, spawned at random location</param>
    private void SpawnAgent<T>(Vector3? loc) where T: Vehicle
    {
        //Note that SpawnOnFloor also adds to AllActors list
        if (typeof(T) == typeof(Human))
        {
            GameObject humanGO = SpawnOnFloor(humanPrefab, loc); 
            Humans.Add(humanGO.GetComponent<Human>());
        }
        else if (typeof(T) == typeof(Zombie))
        {
            GameObject zombieGO = SpawnOnFloor(zombiePrefab, loc); 
            Zombies.Add(zombieGO.GetComponent<Zombie>());
        }
        else if (typeof(T) == typeof(ControllableHuman))
        {
            ActiveControllableHuman = SpawnOnFloor(controllableHumanPrefab, centerPivot: false).GetComponent<ControllableHuman>();
            Humans.Add(ActiveControllableHuman); //Also add to Humans list so it can be targeted
            worldTarget.GetComponent<MouseRotater>().enabled = false;
            ActiveControllableHuman.ToggleCollider();
            followCam.target = ActiveControllableHuman.transform;
            txtInstr.text = INSTR_CONTROLLABLE;
        }
        CollisionManager.Instance.UpdateAllColliders();
    }

    //NOTE: Requests are done like because we must wait for all list iterations from vehicles to be over before modifying the lists!

    /// <summary>
    /// Request that an agent be spawned in the lateupdate function.
    /// </summary>
    /// <typeparam name="T">Type to spawn</typeparam>
    /// <param name="loc">Where to spawn</param>
    public void RequestAgentSpawn<T>(Vector3? loc) where T: Vehicle
    {
        lateUpdateQueue.Enqueue(() => {
            SpawnAgent<T>(loc);
            StartCoroutine(SpawnDelay());
        }); 
    }
    /// <summary>
    /// Necessary for events to be broadcasted appropriately for new spawn.
    /// </summary>
    private IEnumerator SpawnDelay()
    {
        yield return new WaitForSeconds(0.01f);
        DebugLineRenderer.Draw = DebugLineRenderer.Draw;
    }
    /// <summary>
    /// Request that an agent be removed in the lateupdate function.
    /// </summary>
    /// <param name="agent">Agent to remove</param>
    public void RequestAgentRemoval(GameObject agent)
    {
        lateUpdateQueue.Enqueue(() => { RemoveAgent(agent); });
    }

    /// <summary>
    /// Handle lateUpdateQueue Actions, if any.
    /// </summary>
    private void LateUpdate()
    {
        while (lateUpdateQueue.Count > 0)
        {
            Action action = lateUpdateQueue.Dequeue();
            action();
        }
    }
}
