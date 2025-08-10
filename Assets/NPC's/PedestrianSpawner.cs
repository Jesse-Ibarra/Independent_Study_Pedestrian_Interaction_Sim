using UnityEngine;
using System.Collections.Generic;

public class PedestrianSpawner : MonoBehaviour
{    
    [Header("Pedestrian Prefabs")]
    public List<GameObject> pedestrianPrefabs;

    [System.Serializable]
    public class WaypointPath
    {
        public List<Transform> path;
    }

    [System.Serializable]
    public class SpawnPointWithPaths
    {
        public Transform spawnPoint;
        public List<WaypointPath> pathOptions; // Each spawn point has 3+ path options
    }

    [System.Serializable]
    public class SpawnPathGroup
    {
        public List<SpawnPointWithPaths> spawnPointsWithPaths; // Group of spawn points for one zone
    }

    public List<SpawnPathGroup> spawnPathGroups;

    private Dictionary<GameObject, int> prefabSpawnCounts;

    private static List<GameObject> activePedestrians = new List<GameObject>();

    private void Awake()
    {
        prefabSpawnCounts = new Dictionary<GameObject, int>();
        foreach (var prefab in pedestrianPrefabs)
        {
            prefabSpawnCounts[prefab] = 0;
        }
    }

    public void SpawnAllOnce()
    {
        foreach (var ped in activePedestrians)
    {
        if (ped != null) Destroy(ped);
    }
    activePedestrians.Clear();
    if (spawnPathGroups.Count < 3)
    {
        Debug.LogWarning("You need at least 3 spawn groups (2 pairs, 1 solo).");
        return;
    }

    // Get spawn points from each group
    var groupA = spawnPathGroups[0].spawnPointsWithPaths.Count > 0 ? spawnPathGroups[0].spawnPointsWithPaths[0] : null;
    var groupB = spawnPathGroups[1].spawnPointsWithPaths.Count > 0 ? spawnPathGroups[1].spawnPointsWithPaths[0] : null;
    var groupC = spawnPathGroups[2].spawnPointsWithPaths.Count > 0 ? spawnPathGroups[2].spawnPointsWithPaths[0] : null;

    if (groupA == null || groupB == null || groupC == null)
    {
        Debug.LogWarning("One or more required spawn points are missing.");
        return;
    }

    // Validate each has at least 2 paths (needed for the paired logic)
    if (groupA.pathOptions.Count < 2 || groupB.pathOptions.Count < 2 || groupC.pathOptions.Count < 1)
    {
        Debug.LogWarning("Each spawn point must have valid path options.");
        return;
    }

    // --- GROUP A: spawn 2 pedestrians on DIFFERENT paths ---
    int a1 = Random.Range(0, groupA.pathOptions.Count);
    int a2;
    do { a2 = Random.Range(0, groupA.pathOptions.Count); } while (a2 == a1);

    SpawnPedestrian(groupA, a1, a2); // path a1, alt a2
    SpawnPedestrian(groupA, a2, a1); // path a2, alt a1

    // --- GROUP B: spawn 2 pedestrians on DIFFERENT paths ---
    int b1 = Random.Range(0, groupB.pathOptions.Count);
    int b2;
    do { b2 = Random.Range(0, groupB.pathOptions.Count); } while (b2 == b1);

    SpawnPedestrian(groupB, b1, b2); // path b1, alt b2
    SpawnPedestrian(groupB, b2, b1); // path b2, alt b1

    // --- GROUP C: spawn 1 pedestrian ---
    int c1 = Random.Range(0, groupC.pathOptions.Count);
    int c2;
    do { c2 = Random.Range(0, groupC.pathOptions.Count); } while (c2 == c1);

    SpawnPedestrian(groupC, c1, c2);

    Debug.Log("Spawned: 2 from Group A, 2 from Group B, 1 from Group C — Total: 5");
}
    
    private GameObject GetLeastUsedPrefab()
    {
        int minCount = int.MaxValue;
        List<GameObject> leastUsed = new List<GameObject>();

        foreach (var kvp in prefabSpawnCounts)
        {
            if (kvp.Value < minCount)
            {
                leastUsed.Clear();
                leastUsed.Add(kvp.Key);
                minCount = kvp.Value;
            }
            else if (kvp.Value == minCount)
            {
                leastUsed.Add(kvp.Key);
            }
        }

        GameObject chosen = leastUsed[Random.Range(0, leastUsed.Count)];
        prefabSpawnCounts[chosen]++;
        return chosen;
    }

    private void SpawnPedestrian(SpawnPointWithPaths spawner, int pathIndex, int altIndex)
    {
        var initialPath = spawner.pathOptions[pathIndex].path;
        var alternatePath = spawner.pathOptions[altIndex].path;

        GameObject prefabToSpawn = GetLeastUsedPrefab();
        GameObject pedestrian = Instantiate(prefabToSpawn, spawner.spawnPoint.position, spawner.spawnPoint.rotation);

        Walking_Path wp = pedestrian.GetComponent<Walking_Path>();
        if (wp != null)
        {
            wp.SetWaypoints(initialPath);
            wp.SetAlternateWaypoints(alternatePath);
        }

        // Track this pedestrian
        activePedestrians.Add(pedestrian);
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

}