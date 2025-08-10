using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Bhaptics.SDK2;

public class HapticController : MonoBehaviour
{
    void Start()
    {
        Debug.Log("HapticController started: 'punch' played on Start.");
        BhapticsLibrary.Play("punch");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("punch");
            BhapticsLibrary.Play("punch");
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("store");
            BhapticsLibrary.Play("store");
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            BhapticsLibrary.PlayMotors(
                position: (int) Bhaptics.SDK2.PositionType.Vest,
                motors: new int [7] {50, 50, 50, 50, 50, 50, 50},
                durationMillis: 1000
            );
        }
    }
}
