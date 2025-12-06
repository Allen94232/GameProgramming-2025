using UnityEngine;

/// <summary>
/// Spawns vehicles at regular intervals along a specific path
/// </summary>
public class VehicleSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("List of vehicle prefabs to spawn randomly")]
    public GameObject[] vehiclePrefabs;
    
    [Tooltip("Starting waypoint for spawned vehicles")]
    public RoadWaypoint startWaypoint;
    
    [Tooltip("Time between vehicle spawns (seconds)")]
    public float spawnInterval = 5f;
    
    [Tooltip("Random variation in spawn time (+/- seconds)")]
    public float spawnVariation = 2f;
    
    [Tooltip("Start spawning immediately when game starts")]
    public bool autoStart = true;

    [Header("Vehicle Properties")]
    [Tooltip("Speed of spawned vehicles")]
    public float vehicleSpeed = 10f;
    
    [Tooltip("Random speed variation (+/- units per second)")]
    public float speedVariation = 3f;

    [Header("Limits")]
    [Tooltip("Maximum number of vehicles this spawner can have active at once (0 = unlimited)")]
    public int maxActiveVehicles = 5;
    
    [Tooltip("Stop spawning after this many vehicles (0 = unlimited)")]
    public int maxTotalVehicles = 0;

    // State tracking
    private float spawnTimer = 0f;
    private float nextSpawnTime = 0f;
    private int totalSpawned = 0;
    private int activeVehicles = 0;
    private bool isSpawning = false;

    void Start()
    {
        // Validate setup
        if (vehiclePrefabs == null || vehiclePrefabs.Length == 0)
        {
            Debug.LogError($"VehicleSpawner {gameObject.name}: No vehicle prefabs assigned!");
            return;
        }

        if (startWaypoint == null)
        {
            Debug.LogError($"VehicleSpawner {gameObject.name}: No start waypoint assigned!");
            return;
        }

        // Initialize spawn timer
        nextSpawnTime = spawnInterval + Random.Range(-spawnVariation, spawnVariation);
        nextSpawnTime = Mathf.Max(0.1f, nextSpawnTime); // Ensure minimum delay

        if (autoStart)
        {
            StartSpawning();
        }
    }

    void Update()
    {
        if (!isSpawning)
            return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= nextSpawnTime)
        {
            TrySpawnVehicle();
            spawnTimer = 0f;
            
            // Calculate next spawn time with variation
            nextSpawnTime = spawnInterval + Random.Range(-spawnVariation, spawnVariation);
            nextSpawnTime = Mathf.Max(0.1f, nextSpawnTime);
        }
    }

    void TrySpawnVehicle()
    {
        // Check if reached max total vehicles
        if (maxTotalVehicles > 0 && totalSpawned >= maxTotalVehicles)
        {
            StopSpawning();
            return;
        }

        // Check if reached max active vehicles
        if (maxActiveVehicles > 0 && activeVehicles >= maxActiveVehicles)
        {
            return; // Don't spawn, wait for existing vehicles to despawn
        }

        SpawnVehicle();
    }

    void SpawnVehicle()
    {
        // Randomly select a vehicle prefab from the list
        GameObject selectedPrefab = vehiclePrefabs[Random.Range(0, vehiclePrefabs.Length)];
        
        // Instantiate vehicle at spawn position
        GameObject newVehicle = Instantiate(selectedPrefab, startWaypoint.GetPosition(), Quaternion.identity);
        
        // Configure vehicle
        Vehicle vehicleScript = newVehicle.GetComponent<Vehicle>();
        if (vehicleScript != null)
        {
            vehicleScript.startWaypoint = startWaypoint;
            
            // Apply speed with variation
            float randomSpeed = vehicleSpeed + Random.Range(-speedVariation, speedVariation);
            vehicleScript.speed = Mathf.Max(1f, randomSpeed); // Ensure minimum speed
        }
        else
        {
            Debug.LogWarning($"VehicleSpawner {gameObject.name}: Spawned vehicle has no Vehicle component!");
        }

        // Track spawned vehicle
        totalSpawned++;
        activeVehicles++;

        // Subscribe to vehicle destruction to update count
        VehicleDestroyNotifier notifier = newVehicle.AddComponent<VehicleDestroyNotifier>();
        notifier.spawner = this;

        Debug.Log($"VehicleSpawner {gameObject.name}: Spawned vehicle #{totalSpawned} (Active: {activeVehicles})");
    }

    // Called by VehicleDestroyNotifier when vehicle is destroyed
    public void OnVehicleDestroyed()
    {
        activeVehicles--;
        activeVehicles = Mathf.Max(0, activeVehicles); // Ensure non-negative
    }

    // Public control methods
    public void StartSpawning()
    {
        isSpawning = true;
        Debug.Log($"VehicleSpawner {gameObject.name}: Started spawning");
    }

    public void StopSpawning()
    {
        isSpawning = false;
        Debug.Log($"VehicleSpawner {gameObject.name}: Stopped spawning");
    }

    public void ResetSpawner()
    {
        totalSpawned = 0;
        activeVehicles = 0;
        spawnTimer = 0f;
        nextSpawnTime = spawnInterval;
        Debug.Log($"VehicleSpawner {gameObject.name}: Reset");
    }

    // Visualize spawn point in editor
    private void OnDrawGizmos()
    {
        if (startWaypoint == null)
            return;

        // Draw spawn point
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(startWaypoint.GetPosition(), 1f);
        
        // Draw spawn indicator
        Gizmos.DrawLine(startWaypoint.GetPosition() + Vector3.up * 1.5f, startWaypoint.GetPosition() + Vector3.up * 2.5f);
    }

    private void OnDrawGizmosSelected()
    {
        if (startWaypoint == null)
            return;

        // Highlight spawn point when selected
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(startWaypoint.GetPosition(), 0.5f);
    }
}

/// <summary>
/// Helper component to notify spawner when vehicle is destroyed
/// </summary>
public class VehicleDestroyNotifier : MonoBehaviour
{
    public VehicleSpawner spawner;

    private void OnDestroy()
    {
        if (spawner != null)
        {
            spawner.OnVehicleDestroyed();
        }
    }
}
