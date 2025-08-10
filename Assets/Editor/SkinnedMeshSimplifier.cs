using UnityEngine;
using UnityEditor;
using UnityMeshSimplifier;

public class SkinnedMeshReducer : MonoBehaviour
{
    [MenuItem("Tools/Simplify Skinned Mesh")]
    static void SimplifySkinnedMesh()
    {
        GameObject selectedObj = Selection.activeGameObject;
        if (selectedObj == null)
        {
            Debug.LogError("No GameObject selected.");
            return;
        }

        var skinnedMeshRenderer = selectedObj.GetComponentInChildren<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer == null || skinnedMeshRenderer.sharedMesh == null)
        {
            Debug.LogError("No SkinnedMeshRenderer with a mesh found on the selected GameObject.");
            return;
        }

        Mesh originalMesh = skinnedMeshRenderer.sharedMesh;

        // Simplify the mesh
        var meshSimplifier = new MeshSimplifier();
        meshSimplifier.Initialize(originalMesh);
        meshSimplifier.SimplifyMesh(0.50f);

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

        Debug.Log("Simplified skinned mesh saved to: " + fullPath);
    }
}
