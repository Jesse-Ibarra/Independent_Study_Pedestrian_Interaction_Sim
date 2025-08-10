using UnityEngine;

public class TestPrint : MonoBehaviour
{
    void Start()
    {
        Debug.Log("✅ TestPrint Start() called");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("✅ Spacebar was pressed in TestPrint");
        }
    }
}

