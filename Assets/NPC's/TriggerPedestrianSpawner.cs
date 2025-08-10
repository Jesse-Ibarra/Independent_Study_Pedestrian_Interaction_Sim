using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerPedestrianSpawner : MonoBehaviour
{
    public PedestrianSpawner spawner;
    public TriggerPedestrianSpawner nextZoneToEnable; // Reference to next trigger zone
    private bool hasSpawned = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (hasSpawned) return;

        if (spawner != null)
        {
            spawner.SpawnAllOnce();
            hasSpawned = true;

            // Disable this zone so it doesn't keep spawning
            gameObject.SetActive(false);

            // Enable the previous zone when the player hits the next one
            if (nextZoneToEnable != null)
                nextZoneToEnable.gameObject.SetActive(true);
        }
    }

    private void OnEnable()
    {
        // Reset spawn state when re-enabled so it can spawn again
        hasSpawned = false;
    }
}
