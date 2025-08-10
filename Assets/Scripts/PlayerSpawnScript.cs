using UnityEngine;

public class ForcePlayerSpawn : MonoBehaviour
{
    public Transform spawnPoint; // Assign in Inspector

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (spawnPoint != null && player != null)
        {
            player.transform.position = spawnPoint.position;
            player.transform.rotation = spawnPoint.rotation;
        }
        else
        {
            Debug.LogWarning("Spawn Point or Player not assigned/found.");
        }
    }
}
