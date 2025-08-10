using UnityEngine;
using Bhaptics.SDK2;

public class ProximityHaptics : MonoBehaviour
{
    public Transform player;
    public Transform enemy;

    public float maxDistance = 10f;
    public float minDistance = 1.5f;
    public float minInterval = 0.1f;
    public float maxInterval = 1.5f;

    private float timer = 0f;
    private float currentInterval = 1f;

    void Update()
    {
        float distance = Vector3.Distance(player.position, enemy.position);

        if (distance > maxDistance)
        {
            timer = 0f;
            return;
        }

        float proximity = 1f - Mathf.InverseLerp(minDistance, maxDistance, distance);
        currentInterval = Mathf.Lerp(maxInterval, minInterval, proximity);
        timer += Time.deltaTime;

        if (timer < currentInterval) return;

        // Scale intensity (10 to 100) and set fixed duration
        int intensity = Mathf.RoundToInt(Mathf.Lerp(10f, 100f, proximity));
        int duration = 100; // ms

        BhapticsLibrary.Play("heartbeat", duration, intensity);

        timer = 0f;
    }
}
