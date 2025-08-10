using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class EditorPlayerMover : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float turnSpeed = 90f;

    private CharacterController controller;
    private Keyboard kb;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        kb = Keyboard.current;
        if (kb == null)
        {
            Debug.LogWarning("No keyboard connected or Input System not initialized.");
        }
    }

    void Update()
    {
        if (!Application.isEditor) return;
        if (kb == null) return;

        Vector3 direction = Vector3.zero;

        if (kb.wKey.isPressed) direction += transform.forward;
        if (kb.sKey.isPressed) direction -= transform.forward;
        if (kb.aKey.isPressed) direction -= transform.right;
        if (kb.dKey.isPressed) direction += transform.right;

        if (direction != Vector3.zero)
        {
            controller.Move(direction.normalized * moveSpeed * Time.deltaTime);
        }

        // Rotate with Q/E
        if (kb.qKey.isPressed)
            transform.Rotate(Vector3.up, -turnSpeed * Time.deltaTime);
        if (kb.eKey.isPressed)
            transform.Rotate(Vector3.up, turnSpeed * Time.deltaTime);

        // Debug test
        if (kb.wKey.isPressed)
            Debug.Log("W is held");
    }
}
