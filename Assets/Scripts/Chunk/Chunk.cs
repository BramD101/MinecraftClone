using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Chunk
{

    private readonly ChunkVoxelMap _map = new();
    public ChunkVoxelMap ChunkVoxelMap => _map;

    private ChunkGameObject _gameObject = null;
    public ChunkCoord ChunkCoord { get; private set; }
    public bool IsFilledIn => _map.IsFilledIn;

    public RenderStatus RenderStatus { get; private set; } = RenderStatus.NotUpToDate; 

    public Chunk(ChunkCoord chunkCoord)
    {
        ChunkCoord = chunkCoord;
    }

    public void UpdateMesh(ChunkMeshDataDTO meshData, GameSettings gameSettings, Transform worldTransform)
    {
        if(_gameObject == null)
        {
            _gameObject = new ChunkGameObject(gameSettings,worldTransform, ChunkCoord);
        }

        _gameObject.UpdateMesh(meshData);
        RenderStatus = RenderStatus.UpToDate;
    }

    public WorldGenerationData CreateOrRetrieveVoxelmap(ChunkVoxelMapRepository chunkRepository)
    {
        return chunkRepository.CreateOrRetrieveChunkVoxelMap(ChunkCoord);
    }

    public bool TrySetVoxelMap(VoxelMinimal[,,] map)
    {
        return _map.TrySetMap(map);
    }
    public Voxel GetVoxel(RelativeToChunkVoxelPosition<int> relPos)
    {
        return _map.GetVoxel(relPos);
    }
    public bool TryGetVoxel(RelativeToChunkVoxelPosition<int> relPos, out Voxel voxel)
    {
        return _map.TryGetVoxel(relPos, out voxel);
    }

    public void EnqueueVoxelMods(Queue<VoxelMod> voxelMods)
    {
        _map.EnqueueVoxelMods(voxelMods);
    }

    public void StartRender()
    {
        RenderStatus = RenderStatus.Rendering;
    }

    public bool IsRendered()
    {
        return RenderStatus == RenderStatus.UpToDate;
    }

    
}
public enum RenderStatus
{
    UpToDate,
    Rendering,
    NotUpToDate
}
