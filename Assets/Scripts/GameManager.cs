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
    public GameObject controllableHumanPrefab;
    public MeshRenderer floor;
    public Follow followCam;
    public Transform worldTarget;

    public SpawnInfo humanSpawnInfo;
    public SpawnInfo zombieSpawnInfo;
    public SpawnInfo obstacleSpawnInfo;

    public Button buttonEnter;
    public Text txtInstr;

    public List<GameObject> AllActors { get; private set; }
    public List<Vehicle> Humans { get; private set; }
    public List<Vehicle> Zombies { get; private set; }
    public List<GameObject> Obstacles { get; private set; }
    public ControllableHuman ActiveControllableHuman { get; private set; }

    private const string INSTR_NORMAL = "<b>Click and drag</b> to rotate camera";
    private const string INSTR_CONTROLLABLE = "<b>W A S D</b> to move";



    private Queue<Action> lateUpdateQueue;

    // Use this for initialization
    void Start()
    {
        followCam.target = worldTarget;
        lateUpdateQueue = new Queue<Action>();
        lateUpdateQueue.Enqueue(() => { DebugLineRenderer.Draw = false; });
        txtInstr.text = INSTR_NORMAL;

        if (buttonEnter)
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


        SpawnAgents();


    }


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

    public void SpawnZombie()
    {
        RequestAgentSpawn<Zombie>(null);
    }
    public void SpawnHuman()
    {
        RequestAgentSpawn<Human>(null);
    }
    public void ToggleDebugLines()
    {
        DebugLineRenderer.Draw = !DebugLineRenderer.Draw;
    }
    public void SpawnObstacle()
    {
        GameObject obstacle = SpawnOnFloor(obstaclePrefab, centerPivot: false);
        Obstacles.Add(obstacle);
        CollisionManager.Instance.UpdateAllColliders();
    }

    /// <summary>
    /// Spawn an instance of original on a random position on the floor
    /// </summary>
    private GameObject SpawnOnFloor(GameObject original, Vector3? loc = null, bool centerPivot=false)
    {
        Vector3 spawnLoc = loc == null ? RandomPosOnFloor(original, centerPivot) : loc.Value;
        GameObject newGo = Instantiate<GameObject>(original, spawnLoc, Quaternion.identity);
        AllActors.Add(newGo);
        return newGo;
    }

    /// <summary>
    /// Get a random position on the floor such that the gameobject will be on top of the floor
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

    private Vector3 SampleHeight(GameObject target, Vector3 orig)
    {
        orig.y = floor.bounds.max.y + target.GetComponent<Collider>().renderer.bounds.extents.y;
        return orig;
    }

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
    /// Request that an agent be spawned in the lateupdate function
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
    /// Necessary for events to be broadcasted appropriately for new spawn
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnDelay()
    {
        yield return new WaitForSeconds(0.01f);
        DebugLineRenderer.Draw = DebugLineRenderer.Draw;
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
    private void Update()
    {
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
