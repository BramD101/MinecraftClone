using System;
using System.Drawing;
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

    public ChunkGameObject(GameSettings settings, Transform worldTransform, ChunkCoord coord)
    {
        _object = new GameObject();
        _meshFilter = _object.AddComponent<MeshFilter>();
        _meshRenderer = _object.AddComponent<MeshRenderer>();

        Material[] materials = new Material[3];
        materials[0] = settings.Material;
        materials[1] = settings.TransparentMaterial;
        materials[2] = settings.WaterMaterial;
        _meshRenderer.materials = materials;

        _object.transform.SetParent(worldTransform);
        _object.transform.position = new Vector3(coord.X * VoxelData.ChunkWidth, 0f, coord.Z * VoxelData.ChunkWidth);
        _object.name = $"Chunk ({coord.X}, {coord.Z})";
    }

    public void UpdateMesh(ChunkMeshData meshData)
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



