using UnityEngine;

public class FaceExpressionsLogger : MonoBehaviour
{
    public OVRFaceExpressions faceExpressions;

    void Update()
    {
        if (faceExpressions == null || !faceExpressions.FaceTrackingEnabled)
            return;

        LogExpression("Smile Left", OVRFaceExpressions.FaceExpression.LipCornerPullerL);
        LogExpression("Smile Right", OVRFaceExpressions.FaceExpression.LipCornerPullerR);
        LogExpression("Brow Raise Left", OVRFaceExpressions.FaceExpression.OuterBrowRaiserL);
        LogExpression("Brow Raise Right", OVRFaceExpressions.FaceExpression.OuterBrowRaiserR);
        LogExpression("Jaw Drop", OVRFaceExpressions.FaceExpression.JawDrop);
        LogExpression("Tongue Out", OVRFaceExpressions.FaceExpression.TongueOut);
    }

    void LogExpression(string label, OVRFaceExpressions.FaceExpression expression)
    {
        float value = faceExpressions.GetWeight(expression);
        if (value > 0.3f) 
        {
            Debug.Log($"{label}: {value:F2}");
        }
    }
}
