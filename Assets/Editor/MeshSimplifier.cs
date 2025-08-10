using UnityEngine;
using UnityEditor;
using UnityMeshSimplifier;

public class MeshReducer : MonoBehaviour
{
    [MenuItem("Tools/Simplify Mesh")]
    static void SimplifyMesh()
    {
        GameObject selectedObj = Selection.activeGameObject;
        if (selectedObj == null)
        {
            Debug.LogError("No GameObject selected.");
            return;
        }

        var meshFilter = selectedObj.GetComponentInChildren<MeshFilter>();
        var meshRenderer = selectedObj.GetComponentInChildren<MeshRenderer>();

        if (meshFilter == null || meshRenderer == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError("No MeshRenderer/MeshFilter with a mesh found on the selected GameObject.");
            return;
        }

        Mesh originalMesh = meshFilter.sharedMesh;

        // Simplify the mesh
        var meshSimplifier = new MeshSimplifier();
        meshSimplifier.Initialize(originalMesh);
        meshSimplifier.SimplifyMesh(0.50f); // Reduce to 50% of original detail

        Mesh simplifiedMesh = meshSimplifier.ToMesh();
        simplifiedMesh.name = originalMesh.name + "_LOD50%";

        // Save the new mesh as an asset
        string path = "Assets/SimplifiedMeshes";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder("Assets", "SimplifiedMeshes");
        }

        string fullPath = path + "/" + simplifiedMesh.name + ".asset";
        AssetDatabase.CreateAsset(simplifiedMesh, fullPath);
        AssetDatabase.SaveAssets();

        // Apply the simplified mesh back to the MeshFilter
        meshFilter.sharedMesh = simplifiedMesh;

        Debug.Log("Simplified mesh saved and applied: " + fullPath);
    }
}
