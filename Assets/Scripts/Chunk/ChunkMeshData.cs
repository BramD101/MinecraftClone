using System.Collections.Generic;
using UnityEngine;

public class ChunkMeshData
{
    public ChunkCoord ChunkCoord { get; set; }
    public List<Vector3> Vertices { get; set; } = new List<Vector3>();
    public List<int> Triangles { get; set; } = new List<int>();
    public List<int> TransparentTriangles { get; set; } = new List<int>();
    public List<int> WaterTriangles { get; set; } = new List<int>();
    public List<Vector2> Uvs { get; set; } = new List<Vector2>();
    public List<Color> Colors { get; set; } = new List<Color>();
    public List<Vector3> Normals { get; set; } = new List<Vector3>();
    public int VertexIndex { get; set; } = 0;
    public ChunkMeshData(ChunkCoord chunkCoord)
    {
        ChunkCoord = chunkCoord;
    }
}

