using UnityEngine;

public class ChunkRenderInstance
{
    private readonly ChunkMeshData _chunkMeshData;
    private ChunkCoord _currentChunkCoord;
    private readonly ChunkVoxelMap _voxelMap;
    private readonly ChunksNeighbours _neighbours;

    public ChunkRenderInstance(ChunkCoord coord, ChunkVoxelMap voxelMap, ChunksNeighbours neighbours)
    {
        _currentChunkCoord = coord;
        _voxelMap = voxelMap;
        _chunkMeshData = new(coord);
        _neighbours = neighbours;
    }
    public ChunkMeshData CreateChunkMeshData()
    {

        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    RelativeVoxelPos pos = new(x, y, z);

                    AddVoxelMeshDataToMeshData(pos);
                }
            }
        }
        return _chunkMeshData;
    }


    private void AddVoxelMeshDataToMeshData(RelativeVoxelPos relPos)
    {

        Voxel voxel = _voxelMap.GetVoxel(relPos);

        float rot = 0f;

        if (!voxel.IsSolid)
        {
            return;
        }

        for (int p = 0; p < 6; p++)
        {

            VoxelDirection direction = (VoxelDirection)p;
            Vector3Int offset = GetRelativePosition(direction);
            RelativeVoxelPos neighbourRelCoord = new(relPos.X + offset.x, relPos.Y + offset.y, relPos.Z + offset.z);

            if (neighbourRelCoord.Y < 0 || neighbourRelCoord.Y >= VoxelData.ChunkHeight)
            {
                continue;
            }

            Voxel neighbour;
            if (ChunkVoxelMap.IsInVoxelMap(neighbourRelCoord))
            {
                neighbour = _voxelMap.GetVoxel(neighbourRelCoord);
            }
            else
            {
                GlobalVoxelPos absPos = GlobalVoxelPos.FromRelativeToChunkPosition(neighbourRelCoord, _currentChunkCoord);
                ChunkCoord neigbourCoord = ChunkCoord.FromGlobalVoxelPosition(absPos);
                neighbour = _neighbours.GetChunkByCoord(neigbourCoord).GetVoxel(RelativeVoxelPos.CreateFromGlobal(absPos));
            }

            if (neighbour.RenderNeighborFaces && !(voxel.IsWater && _voxelMap.GetVoxel(new RelativeVoxelPos(relPos.X, relPos.Y + 1, relPos.Z)).IsWater))
            {
                float lightLevel = 15;
                int faceVertCount = 0;

                for (int i = 0; i < voxel.MeshData.Faces[p].VertData.Length; i++)
                {

                    VertData vertData = voxel.MeshData.Faces[p].GetVertData(i);
                    _chunkMeshData.Vertices.Add(new Vector3(relPos.X, relPos.Y, relPos.Z) + vertData.GetRotatedPosition(new Vector3(0, rot, 0)));
                    _chunkMeshData.Normals.Add(VoxelData.faceChecks[p]);
                    _chunkMeshData.Colors.Add(new Color(0, 0, 0, lightLevel));
                    if (voxel.IsWater)
                        _chunkMeshData.Uvs.Add(voxel.MeshData.Faces[p].VertData[i].uv);
                    else
                        _chunkMeshData.Uvs.Add(GetUvs(voxel.GetTextureID((VoxelDirection)p), vertData.uv));
                    faceVertCount++;

                }

                if (!voxel.RenderNeighborFaces)
                {
                    for (int i = 0; i < voxel.MeshData.Faces[p].Triangles.Length; i++)
                        _chunkMeshData.Triangles.Add(_chunkMeshData.VertexIndex + voxel.MeshData.Faces[p].Triangles[i]);
                }
                else
                {
                    if (voxel.IsWater)
                    {
                        for (int i = 0; i < voxel.MeshData.Faces[p].Triangles.Length; i++)
                            _chunkMeshData.WaterTriangles.Add(_chunkMeshData.VertexIndex + voxel.MeshData.Faces[p].Triangles[i]);
                    }
                    else
                    {
                        for (int i = 0; i < voxel.MeshData.Faces[p].Triangles.Length; i++)
                            _chunkMeshData.TransparentTriangles.Add(_chunkMeshData.VertexIndex + voxel.MeshData.Faces[p].Triangles[i]);
                    }
                }

                _chunkMeshData.VertexIndex += faceVertCount;

            }
        }
    }
    private Vector2 GetUvs(int textureID, Vector2 uv)
    {

        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        x += VoxelData.NormalizedBlockTextureSize * uv.x;
        y += VoxelData.NormalizedBlockTextureSize * uv.y;

        return new Vector2(x, y);

    }

    private static Vector3Int GetRelativePosition(VoxelDirection direction)
    {
        Vector3Int[] faceChecks = new Vector3Int[6] {
        new Vector3Int(0, 0, -1),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(1, 0, 0) };

        return faceChecks[(int)direction];
    }
}

