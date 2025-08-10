using UnityEngine;
using System.Collections.Generic;

public class Walking_Path : MonoBehaviour
{
    public float speed = 3f;
    public float rotationSpeed = 5f;
    public float resumeDistance = 4f; // Distance from player needed to resume movement after collision

    private int targetIndex = 0;
    private bool shouldMove = true;
    private bool playerInContact = false;

    private Animator anim;
    private Transform playerTransform;

    private float gazeTimer = 0f;
    public float gazeThreshold = 2f;

    [SerializeField] private Transform eyeCamera;
    [SerializeField] private LayerMask gazeLayerMask;

    private List<Transform> currentWaypoints = new List<Transform>();
    private List<Transform> alternateWaypoints = new List<Transform>();

    public void SetWaypoints(List<Transform> path)
    {
        currentWaypoints = path;
        targetIndex = 0;
    }

    public void SetAlternateWaypoints(List<Transform> path)
    {
        alternateWaypoints = path;
    }

    private void Start()
    {
        anim = GetComponent<Animator>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        Transform eyeObj = GameObject.Find("RightEyeInteractor")?.transform;
        if (eyeObj != null) eyeCamera = eyeObj;

        if (eyeCamera == null)
        {
            Debug.LogWarning($"{name}: Eye camera not found. Gaze-based path switching disabled.");
        }
    }

    private void Update()
{
    HandleGaze();

    // Resume walking when player moves far enough away
    if (playerInContact && playerTransform != null)
    {
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        if (distance > resumeDistance)
        {
            playerInContact = false;
            shouldMove = true;
        }
    }

    // If not moving, stop animation and skip movement
    if (!shouldMove)
    {
        if (anim) anim.SetFloat("MoveSpeed", 0f);
        return;
    }

    if (currentWaypoints == null || currentWaypoints.Count == 0) return;

    Transform target = currentWaypoints[targetIndex];
    float step = speed * Time.deltaTime;
    transform.position = Vector3.MoveTowards(transform.position, target.position, step);

    if (Vector3.Distance(transform.position, target.position) < 1.5f)
    {
        targetIndex++;
        if (targetIndex >= currentWaypoints.Count)
        {
            Destroy(gameObject);
            return;
        }
    }

    Vector3 direction = (target.position - transform.position).normalized;
    if (direction != Vector3.zero)
    {
        Quaternion lookRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotationSpeed * Time.deltaTime);
    }

    if (anim) anim.SetFloat("MoveSpeed", speed);
}

    private void HandleGaze()
    {
        if (eyeCamera == null || !shouldMove) return;

        Ray ray = new Ray(eyeCamera.position, eyeCamera.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 25f, gazeLayerMask))
        {
            if (hit.transform == transform)
            {
                Vector3 toCam = (eyeCamera.position - transform.position).normalized;
                float angleToFront = Vector3.Dot(transform.forward, toCam);

                if (angleToFront > 0.5f)
                {
                    gazeTimer += Time.deltaTime;
                    if (gazeTimer >= gazeThreshold)
                    {
                        SwitchToAlternatePath();
                        gazeTimer = 0f;
                    }
                }
                else
                {
                    gazeTimer = 0f;
                }
            }
            else
            {
                gazeTimer = 0f;
            }
        }
        else
        {
            gazeTimer = 0f;
        }
    }

    private void SwitchToAlternatePath()
    {
        if (alternateWaypoints == null || alternateWaypoints.Count == 0) return;

        int currentWaypoint = targetIndex;
        SetWaypoints(alternateWaypoints);
        targetIndex = Mathf.Min(currentWaypoint, currentWaypoints.Count - 1);

        Debug.Log($"{name} switched to alternate path at waypoint {targetIndex + 1}");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player Collided with Pedestrian");
            playerInContact = true;
            shouldMove = false;
        }
    }
}