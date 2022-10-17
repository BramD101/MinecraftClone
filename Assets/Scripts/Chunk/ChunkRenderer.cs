using System;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class ChunkRenderer
{
    private readonly GameWorld _gameWorld;
    public ChunkRenderer(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }
    public ChunkMeshDataDTO CreateChunkMeshData(ChunkCoord coord, ChunkVoxelMap voxelMap)
    {
        ChunkMeshDataDTO chunkMeshDataDTO = new();
        chunkMeshDataDTO.ChunkCoord = coord;

        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    RelativeToChunkVoxelPosition<int> pos = new(x, y, z);

                    AddVoxelMeshDataToMeshData(ref chunkMeshDataDTO, pos, coord, voxelMap);
                }
            }
        }
        return chunkMeshDataDTO;
    }


    private void AddVoxelMeshDataToMeshData(ref ChunkMeshDataDTO chunkMeshDataDTO,
        RelativeToChunkVoxelPosition<int> relPos, ChunkCoord coord, ChunkVoxelMap voxelMap)
    {     

        Voxel voxel = voxelMap.GetVoxel(relPos);

        float rot = 0f;

        if (!voxel.IsSolid)
        {
            return;
        }

        for (int p = 0; p < 6; p++)
        {
            
            Direction direction = (Direction)p;
            Vector3Int offset = GetRelativePosition(direction);
            RelativeToChunkVoxelPosition<int> neighbourRelCoord = new(relPos.X + offset.x, relPos.Y + offset.y, relPos.Z + offset.z);


            


            if (neighbourRelCoord.Y < 0 || neighbourRelCoord.Y >= VoxelData.ChunkHeight)
            {
                continue;
            }

            Voxel neighbour;
            if (ChunkVoxelMap.IsInVoxelMap(neighbourRelCoord))
            {
                neighbour = voxelMap.GetVoxel(neighbourRelCoord);
            }
            else
            {
                GlobalVoxelPosition<int> absPos = GlobalVoxelPosition<int>.FromRelativeToChunkPosition(neighbourRelCoord, coord);
                if (!_gameWorld.TryGetVoxel(absPos, out neighbour))
                {
                    throw new Exception("Out of world");
                }
            }

            if (neighbour.RenderNeighborFaces && !(voxel.IsWater && voxelMap.GetVoxel(new RelativeToChunkVoxelPosition<int>(relPos.X, relPos.Y + 1, relPos.Z)).IsWater))
            {
                float lightLevel = 15;
                int faceVertCount = 0;

                for (int i = 0; i < voxel.MeshData.Faces[p].VertData.Length; i++)
                {

                    VertData vertData = voxel.MeshData.Faces[p].GetVertData(i);
                    chunkMeshDataDTO.Vertices.Add(new Vector3(relPos.X,relPos.Y,relPos.Z) + vertData.GetRotatedPosition(new Vector3(0, rot, 0)));
                    chunkMeshDataDTO.Normals.Add(VoxelData.faceChecks[p]);
                    chunkMeshDataDTO.Colors.Add(new Color(0, 0, 0, lightLevel));
                    if (voxel.IsWater)
                        chunkMeshDataDTO.Uvs.Add(voxel.MeshData.Faces[p].VertData[i].uv);
                    else
                        chunkMeshDataDTO.Uvs.Add(GetUvs(voxel.GetTextureID((Direction)p), vertData.uv));
                    faceVertCount++;

                }

                if (!voxel.RenderNeighborFaces)
                {
                    for (int i = 0; i < voxel.MeshData.Faces[p].Triangles.Length; i++)
                        chunkMeshDataDTO.Triangles.Add(chunkMeshDataDTO.VertexIndex + voxel.MeshData.Faces[p].Triangles[i]);
                }
                else
                {
                    if (voxel.IsWater)
                    {
                        for (int i = 0; i < voxel.MeshData.Faces[p].Triangles.Length; i++)
                            chunkMeshDataDTO.WaterTriangles.Add(chunkMeshDataDTO.VertexIndex + voxel.MeshData.Faces[p].Triangles[i]);
                    }
                    else
                    {
                        for (int i = 0; i < voxel.MeshData.Faces[p].Triangles.Length; i++)
                            chunkMeshDataDTO.TransparentTriangles.Add(chunkMeshDataDTO.VertexIndex + voxel.MeshData.Faces[p].Triangles[i]);
                    }
                }

                chunkMeshDataDTO.VertexIndex += faceVertCount;

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

    private static Vector3Int GetRelativePosition(Direction direction)
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
public enum Direction
{
    LowerZ = 0,
    HigherZ = 1,
    HigherY = 2,
    LowerY = 3,
    LowerX = 4,
    HigherX = 5
}
