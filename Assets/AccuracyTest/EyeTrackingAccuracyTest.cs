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

    private GameObject[] randomizedSpheres;
    private string logPath;
    private bool testStarted = false;

    [SerializeField] private LayerMask gazeLayersToInclude;

    private const float targetRadius = 0.5f; // Accept gaze within 0.5m radius

    public string csvFileName = "EyeTrackingLog.csv";  // Set in Inspector

    void Start()
    {
        string folderPath = Path.Combine(Application.dataPath, "Logs");

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        // Use the name set in the Inspector
        logPath = Path.Combine(folderPath, csvFileName);

        if (!File.Exists(logPath))
        {
            File.WriteAllText(logPath,
                "Time,Target,TargetPos,TargetScale,InitialHit,AvgGazeHit,CenterError,EyeToTargetDistance,ValidFrames,ExpectedFrames,GazeHitRate,InitialHit%,AvgGaze%\n");
        }

        Debug.Log("Eye Tracking Log saved to: " + logPath);
    }

    void Awake()
    {
        foreach (GameObject sphere in targetSpheres)
        {
            Renderer rend = sphere.GetComponent<Renderer>();
            if (rend != null)
                rend.material = new Material(rend.material);
        }

        randomizedSpheres = targetSpheres.OrderBy(x => Random.value).ToArray();
    }

    [ContextMenu("Start Accuracy Test")]
    public void StartTest()
    {
        if (testStarted) return;

        testStarted = true;
        File.AppendAllText(logPath, $"\n--- New Session: {System.DateTime.Now} ---\n");

        StartCoroutine(RunTest());
    }

    IEnumerator RunTest()
    {
        foreach (GameObject currentTarget in randomizedSpheres)
        {
            Vector3 targetCenter = currentTarget.transform.position;
            Vector3 initialHit = Vector3.zero;
            bool initialHitFound = false;

            List<Vector3> gazeSamples = new List<Vector3>();

            HighlightColor(currentTarget, Color.blue);
            yield return new WaitForSeconds(2f);

            Highlight(currentTarget, true);
            float startTime = Time.time;

            while (Time.time - startTime < dwellTime)
            {
                Vector3 origin = eyeGaze.transform.position;
                Vector3 direction = eyeGaze.transform.forward;

                if (Physics.Raycast(origin, direction, out RaycastHit hit, 40f, gazeLayersToInclude))
                {
                    Debug.DrawRay(origin, direction * 10f, Color.green, 0.1f);

                    if (!initialHitFound)
                    {
                        initialHit = hit.point;
                        initialHitFound = true;
                    }
                    else
                    {
                        float dist = Vector3.Distance(hit.point, targetCenter);
                        if (dist <= targetRadius)
                            gazeSamples.Add(hit.point);
                    }
                }

                yield return null;
            }

            Highlight(currentTarget, false);

            int expectedFrames = Mathf.RoundToInt(dwellTime / Time.deltaTime);
            string targetPos = $"{targetCenter.x:F2} {targetCenter.y:F2} {targetCenter.z:F2}";
            Vector3 scale = currentTarget.transform.localScale;
            string scaleStr = $"{scale.x:F2} {scale.y:F2} {scale.z:F2}";

            if (initialHitFound && gazeSamples.Count > 0)
            {
                Vector3 avgGaze = gazeSamples.Aggregate(Vector3.zero, (sum, pt) => sum + pt) / gazeSamples.Count;
                float error = Vector3.Distance(avgGaze, targetCenter);

                if (error > targetRadius)
                {
                    Debug.LogWarning($"Discarded gaze on {currentTarget.name} — error {error:F3} exceeds radius {targetRadius}m");
                    File.AppendAllText(logPath, $"{Time.time:F4},{currentTarget.name},{targetPos},{scaleStr},null,null,null,null,0,{expectedFrames},0.00,null,null\n");
                    continue;
                }

                float eyeToTargetDistance = Vector3.Distance(eyeGaze.transform.position, targetCenter);
                int validFrames = gazeSamples.Count;
                float gazeHitRate = (float)validFrames / expectedFrames;

                float initialError = Vector3.Distance(initialHit, targetCenter);
                float initialPercent = Mathf.Min(initialError / targetRadius * 100f, 100f);
                float avgPercent = Mathf.Min(error / targetRadius * 100f, 100f);

                string initialHitStr = $"{initialHit.x:F2} {initialHit.y:F2} {initialHit.z:F2}";
                string avgHitStr = $"{avgGaze.x:F2} {avgGaze.y:F2} {avgGaze.z:F2}";

                string log = $"{Time.time:F4},{currentTarget.name},{targetPos},{scaleStr},{initialHitStr},{avgHitStr},{error:F4},{eyeToTargetDistance:F4},{validFrames},{expectedFrames},{gazeHitRate:F2},{initialPercent:F1},{avgPercent:F1}";
                Debug.Log(log);
                File.AppendAllText(logPath, log + "\n");
            }
            else
            {
                Debug.LogWarning($"No valid gaze hit recorded for {currentTarget.name}");
                File.AppendAllText(logPath, $"{Time.time:F4},{currentTarget.name},{targetPos},{scaleStr},null,null,null,null,0,{expectedFrames},0.00,null,null\n");
            }
        }

        testStarted = false;
    }

    void Highlight(GameObject sphere, bool enable)
    {
        Renderer rend = sphere.GetComponent<Renderer>();
        if (rend != null)
            rend.material.color = enable ? Color.red : Color.white;
    }

    void HighlightColor(GameObject sphere, Color color)
    {
        Renderer rend = sphere.GetComponent<Renderer>();
        if (rend != null)
            rend.material.color = color;
    }
}
