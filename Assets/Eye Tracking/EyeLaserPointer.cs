using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class EyeLaserPointer : MonoBehaviour
{
    public OVREyeGaze eyeGaze;              // Assign in Inspector
    public float laserLength = 10f;
    public KeyCode toggleKey = KeyCode.L;   // Press 'L' to toggle laser
    public bool laserEnabled = true;

    private LineRenderer line;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        if (line == null)
        {
            Debug.LogError("LineRenderer is missing on EyeLaserPointer object.");
            enabled = false;
            return;
        }

        if (eyeGaze == null)
        {
            Debug.LogError("OVREyeGaze not assigned to EyeLaserPointer.");
            enabled = false;
            return;
        }

        SetupLaserAppearance();
    }

    void Update()
    {
        /*
        if (Input.GetKeyDown(toggleKey))
        {
            laserEnabled = !laserEnabled;
            line.enabled = laserEnabled;
        }
*/
        if (!laserEnabled || !eyeGaze.enabled) return;

        Vector3 origin = eyeGaze.transform.position;
        Vector3 direction = eyeGaze.transform.forward;

        line.SetPosition(0, origin);
        line.SetPosition(1, origin + direction * laserLength);
    }

    private void SetupLaserAppearance()
    {
        line.useWorldSpace = true;
        line.startWidth = 0.01f;
        line.endWidth = 0.01f;
        line.positionCount = 2;
        line.material = new Material(Shader.Find("Unlit/Color"));
        line.material.color = Color.green;
        line.enabled = laserEnabled;
    }
}
