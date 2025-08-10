using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class EyeTrackingAccuracyTest : MonoBehaviour
{
    public GameObject[] targetSpheres;
    public OVREyeGaze eyeGaze;
    public float dwellTime = 3f;

    [Header("Target Placement Settings")]
    public float placementRadius = 3.5f;
    [Range(0, 89)] public float ringTiltDeg = 35f;

    [Header("Logging Filenames")]
    public string summaryCsv = "EyeTrackingLog.csv";   // per-target summary
    public string samplesCsv = "RawGazePoints.csv";    // per-frame samples (for heat map)

    [SerializeField] private LayerMask gazeLayersToInclude;
    
    private const float targetRadius = 20f;            // acceptance radius (m)
    private GameObject[] randomOrder;
    private string summaryPath, samplePath;
    private bool testStarted = false;

    void Start()
    {
        string logDir = Path.Combine(Application.dataPath, "Logs");
        if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);

        summaryPath = Path.Combine(logDir, summaryCsv);
        samplePath  = Path.Combine(logDir, samplesCsv);

        if (!File.Exists(summaryPath))
            File.WriteAllText(summaryPath,
                "Time,Target,CenterError,EyeToTargetDistance,FrameHitRate(0-1),NormalizedCenterError(%),GazeAngleOffset(deg)\n");

        if (!File.Exists(samplePath))
            File.WriteAllText(samplePath,
                "Time,Target,Yaw(deg),Pitch(deg)\n");

        Debug.Log($"Summary  → {summaryPath}\nSamples → {samplePath}");
    }

    void Awake()
    {
        foreach (var s in targetSpheres)
        {
            var r = s.GetComponent<Renderer>();
            if (r) r.material = new Material(r.material); // duplicate material per target
        }
    }

    [ContextMenu("Start Accuracy Test")]
    public void StartTest()
    {
        if (testStarted) return;

        PositionTargets();

        testStarted = true;
        File.AppendAllText(summaryPath, $"\n--- New Session: {System.DateTime.Now} ---\n");
        File.AppendAllText(samplePath,  $"\n--- New Session: {System.DateTime.Now} ---\n");

        StartCoroutine(RunTest());
    }

    private void PositionTargets()
    {
        if (targetSpheres == null || targetSpheres.Length < 9)
        {
            Debug.LogError("Need at least 9 targets (1 center + 8 ring)");
            return;
        }

        var cam = Camera.main.transform;
        Vector3 origin  = cam.position;
        Vector3 forward = cam.forward.normalized;
        Vector3 right   = cam.right.normalized;
        Vector3 up      = cam.up.normalized;

        // center
        targetSpheres[0].transform.position = origin + forward * placementRadius;
        targetSpheres[0].transform.LookAt(origin);

        float ringR   = placementRadius * Mathf.Sin(ringTiltDeg * Mathf.Deg2Rad);
        float fOffset = placementRadius * Mathf.Cos(ringTiltDeg * Mathf.Deg2Rad);

        for (int i = 0; i < 8; i++)
        {
            float az = i * 45f * Mathf.Deg2Rad; // 0-315° every 45°
            Vector3 radial = (right * Mathf.Cos(az) + up * Mathf.Sin(az)) * ringR;
            Vector3 pos    = origin + forward * fOffset + radial;

            int idx = i + 1;
            targetSpheres[idx].transform.position = pos;
            targetSpheres[idx].transform.LookAt(origin);
        }
    }

    IEnumerator RunTest()
    {
        randomOrder = targetSpheres.OrderBy(_ => Random.value).ToArray();

        foreach (var tgt in randomOrder)
        {
            Vector3 center = tgt.transform.position;
            List<Vector3> samples = new();
            bool gotFirstHit = false;

            Highlight(tgt, true, Color.blue);
            yield return new WaitForSeconds(2f);
            Highlight(tgt, true, Color.red);

            float t0 = Time.time;
            while (Time.time - t0 < dwellTime)
            {
                Vector3 eye = eyeGaze.transform.position;
                Vector3 dir = eyeGaze.transform.forward;

                if (Physics.Raycast(eye, dir, out var hit, 40f, gazeLayersToInclude))
                {
                    float d = Vector3.Distance(hit.point, center);
                    if (d <= targetRadius) samples.Add(hit.point);
                    if (!gotFirstHit) gotFirstHit = true;

                    /* ─ log per-frame yaw/pitch ─ */
                    Vector3 toTarget = (center - eye).normalized;
                    Vector3 toHit    = (hit.point - eye).normalized;

                    Vector3 projToHitXZ = Vector3.ProjectOnPlane(toHit, Vector3.up).normalized;
                    Vector3 projToTargetXZ = Vector3.ProjectOnPlane(toTarget, Vector3.up).normalized;

                    Vector3 projToHitYZ = Vector3.ProjectOnPlane(toHit, Vector3.right).normalized;
                    Vector3 projToTargetYZ = Vector3.ProjectOnPlane(toTarget, Vector3.right).normalized;

                    float yaw = float.NaN;
                    float pitch = float.NaN;

                    if (projToHitXZ != Vector3.zero && projToTargetXZ != Vector3.zero)
                        yaw = Vector3.SignedAngle(projToHitXZ, projToTargetXZ, Vector3.up);

                    if (projToHitYZ != Vector3.zero && projToTargetYZ != Vector3.zero)
                        pitch = Vector3.SignedAngle(projToHitYZ, projToTargetYZ, Vector3.right);

                    File.AppendAllText(samplePath,
                        $"{Time.time:F4},{tgt.name},{(float.IsNaN(yaw) ? "" : yaw.ToString("F2"))},{(float.IsNaN(pitch) ? "" : pitch.ToString("F2"))}\n");


                    File.AppendAllText(samplePath,
                        $"{Time.time:F4},{tgt.name},{yaw:F2},{pitch:F2}\n");
                }
                yield return null;
            }
            Highlight(tgt, false);

            int expFrames = Mathf.RoundToInt(dwellTime / Time.deltaTime);
            if (!gotFirstHit || samples.Count == 0)
            {
                File.AppendAllText(summaryPath,$"{Time.time:F4},{tgt.name},null,null,0.00,null,null\n");
                continue; 
            }

            Vector3 avg = samples.Aggregate(Vector3.zero,(s,p)=>s+p)/samples.Count;
            float   err = Vector3.Distance(avg, center);
            if (err > targetRadius)
            {
                File.AppendAllText(summaryPath,$"{Time.time:F4},{tgt.name},null,null,0.00,null,null\n");
                continue;
            }

            float eyeDist = Vector3.Distance(eyeGaze.transform.position, center);
            float hitRate = (float)samples.Count / expFrames;
            float normErr = Mathf.Min(err / targetRadius * 100f, 100f);

            Vector3 dirT = (center - eyeGaze.transform.position).normalized;
            Vector3 dirA = (avg    - eyeGaze.transform.position).normalized;
            float   ang  = Vector3.Angle(dirT, dirA);

            File.AppendAllText(summaryPath,
                $"{Time.time:F4},{tgt.name},{err:F4},{eyeDist:F4},{hitRate:F2},{normErr:F1},{ang:F2}\n");
        }
        testStarted = false;
    }

    void Highlight(GameObject g, bool on, Color? c=null)
    {
        var r = g.GetComponent<Renderer>();
        if (!r) return;
        if (!on) { r.material.color = Color.white; return; }
        r.material.color = c ?? Color.red;
    }
}