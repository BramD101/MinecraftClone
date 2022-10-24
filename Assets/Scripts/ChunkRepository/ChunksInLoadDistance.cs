
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class ChunksInLoadDistance : ChunkInRangeDictionary
{
   

    public ChunksInLoadDistance(Action<Chunk> disposeMethod,int loadDistance) 
        : base(disposeMethod, loadDistance)
	{
        
    }
    public void ApplyPendingMods(CancellationToken token)
    {
        foreach (var chunk in Chunks)
        {
            chunk.ChunkVoxelMap.TryUpdateBackLog();
        }
    }
    public void LoadVoxelDataForChunksInLoadDistance(ChunkVoxelMapRepository voxelMapRepository,CancellationToken token)
    {
        IEnumerable<WorldGenerationData> generatedChunks = LoadVoxelData(Chunks, voxelMapRepository, token);

        if (generatedChunks != null)
        {
            AddWorldGenerationDataToWorldAsync(generatedChunks);
        }
    }
    public ChunksNeighbours GetNeighbourChunks(ChunkCoord coord)
    {
        ChunksNeighbours neighbours = new();

        foreach (ChunkNeighbourType neighbourtype in ChunksNeighbours.GetChunkNeighbourTypes())
        {
            ChunkCoord neighbourCoord = ChunksNeighbours.GetNeighbourCoord(coord, neighbourtype);
            TryGetChunk(neighbourCoord, out Chunk chunk);
            neighbours.Add(neighbourtype, chunk);
        }
        return neighbours;
    }
    private void AddWorldGenerationDataToWorldAsync(IEnumerable<WorldGenerationData> worldGenerationData)
    {
        // set voxels 
        Dictionary<ChunkCoord, Queue<VoxelMod>> voxelmods = new();
        foreach (WorldGenerationData result in worldGenerationData)
        {
            
            if (TryGetChunk(result.ChunkCoord, out Chunk chunk))
            {
                //debug
                if (result.Map == null)
                {
                    throw new System.Exception("result.Map == null");
                }

                chunk.TrySetVoxelMap(result.Map);
            }
        }

        // aggregate voxelmods

        foreach (WorldGenerationData result in worldGenerationData)
        {
            foreach (ChunkCoord vmCoord in result.Structures.Keys)
            {
                if (!voxelmods.ContainsKey(vmCoord))
                {
                    voxelmods.Add(vmCoord, new Queue<VoxelMod>());
                }
                Queue<VoxelMod> voxelModsinStruct = result.Structures[vmCoord];
                while (voxelModsinStruct.TryDequeue(out VoxelMod vm))
                {
                    voxelmods[vmCoord].Enqueue(vm);
                }
            }
        }

        // enqueue voxelmods    
        foreach (ChunkCoord coord in voxelmods.Keys)
        {
            Chunk chunk;
            if (!TryGetChunk(coord, out chunk))
            {
                chunk = new Chunk(coord);
            }
            chunk.EnqueueVoxelMods(voxelmods[coord]);
        }
    }

    private IEnumerable<WorldGenerationData> LoadVoxelData(List<Chunk> chunks
        ,ChunkVoxelMapRepository voxelMapRepository, CancellationToken token)
    {
        ConcurrentDictionary<ChunkCoord, WorldGenerationData> worldGenDataDict = new();

        List<Task<WorldGenerationData>> tasks = new();
        List<WorldGenerationData> results = new();
        foreach (Chunk chunk in chunks)
        {
            if (!chunk.IsFilledIn)
            {
                results.Add(chunk.CreateOrRetrieveVoxelmap(voxelMapRepository));
            }
            if (token.IsCancellationRequested)
            {
                return null;
            }
        }


        return results;
    }
}

