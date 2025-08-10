using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction; // Ensure you have access to OVREyeGaze

[RequireComponent(typeof(LineRenderer))]
public class EyeTrackingRay : MonoBehaviour
{
    [SerializeField] private float rayDistance = 10f;
    [SerializeField] private float rayWidth = 0.01f;
    [SerializeField] private LayerMask layersToInclude;

    [SerializeField] private Color rayColorDefaultState = Color.yellow;
    [SerializeField] private Color rayColorHoverState = Color.red;

    [SerializeField] private OVREyeGaze eyeGaze; // Reference this in the Inspector or assign via code

    private LineRenderer lineRenderer;
    private List<EyeInteractable> eyeInteractables = new List<EyeInteractable>();

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        SetupRay();

        if (eyeGaze == null)
        {
            eyeGaze = FindObjectOfType<OVREyeGaze>();
            if (eyeGaze == null)
                Debug.LogError("OVREyeGaze not assigned and not found in scene!");
        }
    }

    void SetupRay()
    {
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = rayWidth;
        lineRenderer.endWidth = rayWidth;
        lineRenderer.startColor = rayColorDefaultState;
        lineRenderer.endColor = rayColorDefaultState;
    }

    void LateUpdate()
    {
        if (eyeGaze == null) return;

        RaycastHit hit;

        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = transform.forward;
        Vector3 rayEnd = rayOrigin + rayDirection * rayDistance;

        lineRenderer.SetPosition(0, rayOrigin);
        lineRenderer.SetPosition(1, rayEnd);

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance, layersToInclude))
        {
            Unselect();
            lineRenderer.startColor = rayColorHoverState;
            lineRenderer.endColor = rayColorHoverState;

            var eyeInteractable = hit.transform.GetComponent<EyeInteractable>();
            if (eyeInteractable != null)
            {
                eyeInteractables.Add(eyeInteractable);
                eyeInteractable.IsHovered = true;
            }
        }
        else
        {
            lineRenderer.startColor = rayColorDefaultState;
            lineRenderer.endColor = rayColorDefaultState;
            Unselect(true);
        }
    }

    void Unselect(bool clear = false)
    {
        foreach (var interactable in eyeInteractables)
        {
            interactable.IsHovered = false;
        }

        if (clear)
        {
            eyeInteractables.Clear();
        }
    }
}
