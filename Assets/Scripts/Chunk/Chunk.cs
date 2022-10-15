using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

public class Chunk
{

    private ChunkVoxelMap _map =new();
    private Task _mapGenerationTask = null;

    private readonly ChunkGameObject _gameObject = null;
    public ChunkCoord ChunkCoord { get; private set; }
    public bool IsFilledIn => _map.IsFilledIn;

    public Chunk(ChunkCoord chunkCoord)
    {
        ChunkCoord = chunkCoord;
    }

    public void UpdateMesh(ChunkMeshDataDTO meshData)
    {
        _gameObject.UpdateMesh(meshData);
    }

    public WorldGenerationData CreateOrRetrieveVoxelmap(ChunkVoxelMapRepository chunkRepository)
    {
        return chunkRepository.CreateOrRetrieveChunkVoxelMap(ChunkCoord);

    }

    public bool TrySetVoxelMap(Voxel[,,] map)
    {
        return _map.TrySetMap(map);
    }

    public void EnqueueVoxelMods(Queue<VoxelMod> voxelMods)
    {
        _map.EnqueueVoxelMods(voxelMods);
    }
}

