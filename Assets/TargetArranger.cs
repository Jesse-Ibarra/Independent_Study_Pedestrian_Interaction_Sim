using UnityEngine;

public class TargetArranger : MonoBehaviour
{
    [Tooltip("Assign the same array you use in EyeTrackingAccuracyTest")]
    public GameObject[] targetSpheres;

    [Tooltip("Meters from camera to every target")]
    public float radius = 2.0f;

    [Tooltip("Tilt (°) above / below centre for the 8-ring")]
    public float ringTiltDeg = 35f;   // 30–45° recommended

    void Start()
    {
        if (targetSpheres == null || targetSpheres.Length < 9)
        {
            Debug.LogError("Need at least 9 targets (1 centre + 8 ring).");
            return;
        }

        Transform cam = Camera.main.transform;
        Vector3  origin   = cam.position;
        Vector3  forward  = cam.forward.normalized;
        Vector3  right    = cam.right.normalized;
        Vector3  up       = cam.up.normalized;

        /* ---------- 1. place centre target ---------- */
        targetSpheres[0].transform.position = origin + forward * radius;
        targetSpheres[0].transform.LookAt(origin);

        /* ---------- 2. place 8-ring ---------- */
        float ringRadius = radius * Mathf.Sin(ringTiltDeg * Mathf.Deg2Rad);
        float forwardOffset = radius * Mathf.Cos(ringTiltDeg * Mathf.Deg2Rad);

        for (int i = 0; i < 8; i++)
        {
            float azimuth = i * 45f * Mathf.Deg2Rad;   // 0,45,90…315°
            Vector3 radial =  (right * Mathf.Cos(azimuth) + up * Mathf.Sin(azimuth))
                              * ringRadius;

            Vector3 pos = origin + forward * forwardOffset + radial;

            int idx = i + 1; // targets[1]-[8] are ring
            if (idx >= targetSpheres.Length) break;

            targetSpheres[idx].transform.position = pos;
            targetSpheres[idx].transform.LookAt(origin);
        }
    }
}
