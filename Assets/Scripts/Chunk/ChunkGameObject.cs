using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

public class ChunkGameObject
{
    private GameObject _object;
    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;
    public bool IsActive
    {
        get => _object.activeSelf;
        set => _object.SetActive(value);
    }

    public ChunkGameObject()
    {
        _object = new GameObject();
        _meshFilter = _object.AddComponent<MeshFilter>();
        _meshRenderer = _object.AddComponent<MeshRenderer>();
    }
    ~ChunkGameObject()
    {
        UnityEngine.Object.Destroy(_object);
    }

    internal void UpdateMesh(ChunkMeshDataDTO meshData)
    {
        var mesh = new Mesh();
        mesh.SetVertices(meshData.Vertices.ToArray());
        mesh.subMeshCount = 3;
        mesh.SetTriangles(meshData.Triangles.ToArray(), 0);
        mesh.SetTriangles(meshData.TransparentTriangles.ToArray(), 1);
        mesh.SetTriangles(meshData.WaterTriangles.ToArray(), 2);
        //mesh.triangles = triangles.ToArray();
        mesh.uv = meshData.Uvs.ToArray();
        mesh.colors = meshData.Colors.ToArray();
        mesh.normals = meshData.Normals.ToArray();

        _meshFilter.mesh = mesh;

    }
}



