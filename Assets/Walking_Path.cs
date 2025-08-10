using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walking_Path : MonoBehaviour
{
    public Transform[] pathA;
    public Transform[] pathB;

    private Transform[] currentPath;
    private int targetPoint = 0;
    public float speed = 3f;
    public float rotationSpeed = 5f;
    private bool usingPathA = true;
    private bool shouldMove = true;

    private Transform playerTransform;
    private bool playerInContact = false;
    public float resumeDistance = 2f; // Distance player must move away to resume

    private Animator anim; 

    private float gazeTimer = 0f;
    public float gazeThreshold = 2f;  // seconds needed to trigger path switch

    [SerializeField] private LayerMask gazeLayerMask; // Assign in Inspector
    [SerializeField] private Camera eyeCamera; 

    void Start()
    {
        currentPath = pathA;
        targetPoint = 0;

        // Find Animator on the entity
        anim = GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogWarning("Animator not found on entity.");
        }

        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("Player not found in scene. Make sure it's tagged correctly.");
        }
    }

    void Update()
    {
        // Gaze detection
        if (eyeCamera != null)
        {
            Ray gazeRay = new Ray(eyeCamera.transform.position, eyeCamera.transform.forward);
            if (Physics.Raycast(gazeRay, out RaycastHit hit, 30f, gazeLayerMask))
            {
                Debug.DrawRay(gazeRay.origin, gazeRay.direction * 20f, Color.green);

                if (hit.transform == this.transform)
                {
                    gazeTimer += Time.deltaTime;
                    Debug.Log($"[GAZE] Looking at {name}... Timer: {gazeTimer:F2}s");

                    if (gazeTimer >= gazeThreshold)
                    {
                        Debug.Log("[GAZE] Looked long enough — switching path.");
                        SwapPaths();
                        gazeTimer = 0f;
                    }
                }
                else
                {
                    if (gazeTimer > 0f)
                        Debug.Log($"[GAZE] Gaze broken. Hit: {hit.transform.name}. Resetting timer.");
                    gazeTimer = 0f;
                }
            }
            else
            {
                if (gazeTimer > 0f)
                    Debug.Log("[GAZE] Raycast hit nothing. Resetting gaze timer.");
                gazeTimer = 0f;
            }
        }

        // If player is still near, stay paused
        if (playerInContact && playerTransform != null)
        {
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            if (dist > resumeDistance)
            {
                Debug.Log("Player moved away – resuming movement.");
                shouldMove = true;
                playerInContact = false;
            }
        }

        // Set Animator to idle when not moving
        if (!shouldMove || currentPath.Length == 0)
        {
            if (anim) anim.SetFloat("MoveSpeed", 0f);
            return;
        }

        Vector3 targetPos = currentPath[targetPoint].position;
        float step = speed * Time.deltaTime;

        // Move
        transform.position = Vector3.MoveTowards(transform.position, targetPos, step);

        // Rotate
        Vector3 direction = (targetPos - transform.position).normalized;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }

        // Set Animator walking speed
        if (anim) anim.SetFloat("MoveSpeed", speed);

        // Reached target
        if (Vector3.Distance(transform.position, targetPos) < 0.3f)
        {
            targetPoint = (targetPoint + 1) % currentPath.Length;
        }

        // Swap path with key
        /*
        if (Input.GetKeyDown(KeyCode.P))
        {
            SwapPaths();
        } 
        */ 

    }

    void SwapPaths()
    {
        usingPathA = !usingPathA;
        Transform[] newPath = usingPathA ? pathA : pathB;
        targetPoint = Mathf.Clamp(targetPoint, 0, newPath.Length - 1);
        currentPath = newPath;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Entity in contact with player – stopping");
            shouldMove = false;
            playerInContact = true;

            if (anim) anim.SetFloat("MoveSpeed", 0f);
        }
    }
}
