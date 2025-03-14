using UnityEngine;

public class UVLogger : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Mesh renderer UV debugger");
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            Debug.Log("MeshFilter found!");
            Vector2[] uvs = meshFilter.mesh.uv;
            Debug.Log(uvs);
            for (int i = 0; i < uvs.Length; i++)
            {
                Debug.Log($"Vertex {i} UV: {uvs[i]}");
            }
        }
        else
        {
            Debug.LogError("MeshFilter not found!");
        }
    }
}
