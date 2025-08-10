using UnityEngine;
using UnityEngine.XR;

public class EyeGazeTracker : MonoBehaviour
{
    void Update()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.CenterEye);

        if (device.TryGetFeatureValue(CommonUsages.eyesData, out Eyes eyes))
        {
            if (eyes.TryGetFixationPoint(out Vector3 fixationPoint))
            {
                Debug.Log(" Fixation Point: " + fixationPoint.ToString("F3"));
            }
            else
            {
                Debug.Log(" No fixation point available.");
            }
        }
        else
        {
            Debug.Log(" Eye data not available from device.");
        }
    }
}
