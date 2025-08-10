using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class EyeGazeDebug : MonoBehaviour
{
    public Transform gazeDot;  // drag the sphere here

    void Update()
    {
        List<InputDevice> eyeTrackingDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.EyeTracking, eyeTrackingDevices);

        if (eyeTrackingDevices.Count == 0)
        {
            Debug.LogWarning("No eye tracking devices found.");
        }

        foreach (var device in eyeTrackingDevices)
        {
            if (!device.isValid)
            {
                Debug.LogWarning("Eye tracking device is not valid.");
                continue;
            }

            if (device.TryGetFeatureValue(CommonUsages.eyesData, out Eyes eyes))
            {
                if (eyes.TryGetFixationPoint(out Vector3 fixationPoint))
                {
                    Debug.Log("Fixation Point: " + fixationPoint);

                    if (gazeDot != null)
                    {
                        gazeDot.position = fixationPoint;
                    }
                    else
                    {
                        Debug.LogWarning("Gaze Dot is not assigned.");
                    }
                }
                else
                {
                    Debug.LogWarning("Fixation point not available from eyes.");
                }

                if (eyes.TryGetLeftEyeOpenAmount(out float leftOpen))
                {
                    Debug.Log("Left Eye Open Amount: " + leftOpen);
                }

                if (eyes.TryGetRightEyeOpenAmount(out float rightOpen))
                {
                    Debug.Log("Right Eye Open Amount: " + rightOpen);
                }
            }
            else
            {
                Debug.LogWarning("Failed to get eyesData from device.");
            }
        }
    }
}
