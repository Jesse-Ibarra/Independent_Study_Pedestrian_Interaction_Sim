using UnityEngine;
using Bhaptics.SDK2;

public class PedestrianHapticTrigger : MonoBehaviour
{
    public float minDistanceToRetrigger = 2f;

    private bool hapticPlayed = false;
    private Transform playerTransform;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("Player with tag 'Player' not found!");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !hapticPlayed)
        {
            ContactPoint contact = collision.contacts[0];
            Vector3 localHitDirection = playerTransform.InverseTransformPoint(contact.point);

            string hapticEventName = GetHapticRegion(localHitDirection);
            Debug.Log($"[HAPTIC] Collision detected. Local hit direction: {localHitDirection}, triggering event: {hapticEventName}");

            BhapticsLibrary.Play(hapticEventName);
            hapticPlayed = true;
        }

    }

    string GetHapticRegion(Vector3 localDirection)
    {
        float angle = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
        angle = (angle + 360) % 360; // Normalize angle to 0–360

        Debug.Log($"[HAPTIC] Calculated angle: {angle} degrees");

        if (angle >= 330 || angle < 30)
        {
            Debug.Log("[HAPTIC] Region: Front Center");
            return "bumped_front_center1";
        }
        if (angle >= 30 && angle < 90)
        {
            Debug.Log("[HAPTIC] Region: Front Right");
            return "bumped_front_right1";
        }
        if (angle >= 90 && angle < 150)
        {
            Debug.Log("[HAPTIC] Region: Back Right");
            return "bumped_back_right1";
        }
        if (angle >= 150 && angle < 210)
        {
            Debug.Log("[HAPTIC] Region: Back Center");
            return "bumped_back_center1";
        }
        if (angle >= 210 && angle < 270)
        {
            Debug.Log("[HAPTIC] Region: Back Left");
            return "bumped_back_left1";
        }
        if (angle >= 270 && angle < 330)
        {
            Debug.Log("[HAPTIC] Region: Front Left");
            return "bumped_front_left1";
        }

        Debug.LogWarning("[HAPTIC] Unknown angle region, using fallback.");
        return "bumped_front_center"; // Fallback
    }

    void Update()
    {
        if (hapticPlayed && playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance > minDistanceToRetrigger)
            {
                hapticPlayed = false;
                Debug.Log("[HAPTIC] Player moved away — haptics can retrigger.");
            }
        }
    }
}
