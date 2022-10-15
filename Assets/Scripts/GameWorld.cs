using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
public class GameWorld : IGameWorldMainThreadInteractions
{
    private readonly ChunkInRangeDictionary _chunksInLoadDistance;
    private readonly ChunkInRangeDictionary _chunksInViewDistance;
    private readonly Dictionary<ChunkCoord, Chunk> _chunksOutsideLoadDistance = new();
    private readonly ChunkVoxelMapRepository _voxelMapRepository;
    private Task<Dictionary<ChunkCoord, WorldGenerationData>> _voxelMapCreationTask;

    private ChunkCoord _playerChunkCoord;

    public void Player_Moved(object sender, ChunkCoord args)
    {
        _playerChunkCoord = args;
        UpdateChunksInRange();
    }
    public void Start()
    {
        UpdateChunksInRange();
    }
    public void Update()
    {
        UpdateCreateVoxelMaps();
    }
    public GameWorld(int viewDistance, int loadDistance, ChunkVoxelMapRepository repo)
    {
        _voxelMapRepository = repo;
        _chunksInLoadDistance = new(disposeMethod: c => _chunksOutsideLoadDistance.TryAdd(c.ChunkCoord, c), loadDistance);
        _chunksInViewDistance = new(disposeMethod: c => { }, viewDistance);

    }
    private void UpdateChunksInRange()
    {
        _chunksInLoadDistance.UpdateChunksInDistance(_playerChunkCoord, c =>
        {
            bool success = _chunksOutsideLoadDistance.TryGetValue(c, out Chunk outChunk);
            return (outChunk, success);
        });

        _chunksInViewDistance.UpdateChunksInDistance(_playerChunkCoord, c =>
        {
            bool success = _chunksInLoadDistance.TryGetChunk(c, out Chunk outChunk);
            return (outChunk, success);
        });

        StartCreateVoxelMaps();

    }


    private void StartCreateVoxelMaps()
    {

        if (_voxelMapCreationTask == null)
        {
            _voxelMapCreationTask = Task.Run(() => CreateVoxelMapsTask(_chunksInLoadDistance.Chunks));
        }
    }
    private void UpdateCreateVoxelMaps()
    {
        if (_voxelMapCreationTask != null)
        {
            if (_voxelMapCreationTask.IsCompletedSuccessfully)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();

                OnVoxelMapCreationTaskSuccessfull(_voxelMapCreationTask.Result);
                _voxelMapCreationTask = null;

                watch.Stop();
                Debug.Log($"CreateVoxelMapsTask(async) afgehandeld in {watch.ElapsedMilliseconds}ms");
            }
        }
    }

    private void OnVoxelMapCreationTaskSuccessfull(Dictionary<ChunkCoord, WorldGenerationData> result)
    {
        // set voxels 
        Dictionary<ChunkCoord, Queue<VoxelMod>> voxelmods = new();
        foreach (ChunkCoord coord in result.Keys)
        {
            if (TryGetChunkAtChunkCoord(coord, out Chunk chunk))
            {
                chunk.TrySetVoxelMap(result[coord].Map);
            }
        }

        // aggregate voxelmods
        foreach (ChunkCoord coord in result.Keys)
        {
            foreach (ChunkCoord vmCoord in result[coord].Structures.Keys)
            {
                if (!voxelmods.ContainsKey(vmCoord))
                {
                    voxelmods.Add(vmCoord, new Queue<VoxelMod>());
                }
                Queue<VoxelMod> voxelModsinStruct = result[coord].Structures[vmCoord];
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
            if (!TryGetChunkAtChunkCoord(coord, out chunk))
            {
                chunk = new Chunk(coord);
            }
            chunk.EnqueueVoxelMods(voxelmods[coord]);
        }
    }




    private Dictionary<ChunkCoord, WorldGenerationData> CreateVoxelMapsTask(List<Chunk> chunks)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        Debug.Log("Start CreateVoxelMapsTask(async)");

        Dictionary<ChunkCoord, WorldGenerationData> worldGenData = new();
        foreach (Chunk chunk in chunks)
        {
            if (!chunk.IsFilledIn)
            {
                worldGenData.Add(chunk.ChunkCoord, chunk.CreateOrRetrieveVoxelmap(_voxelMapRepository));
            }
        }
        watch.Stop();
        Debug.Log($"CreateVoxelMapsTask(async) afgehandeld in {watch.ElapsedMilliseconds}ms");
        return worldGenData;
    }

    //Dictionary<ChunkCoord, Queue<VoxelMod>> voxelMods = new();

    //foreach (Chunk chunk in _chunksInLoadDistance.Chunks)
    //{
    //    if (!chunk.IsFilledIn)
    //    {
    //        WorldGenerationData data = chunk.CreateOrRetrieveVoxelmap(_voxelMapRepository);
    //        if (!chunk.TrySetVoxelMap(data.Map))
    //        {
    //            throw new Exception(chunk.ChunkCoord + " Voxelmap generated for chunk which already contained one!");
    //        }

    //        foreach (ChunkCoord chunkCoord in data.Structures.Keys)
    //        {
    //            if (!voxelMods.ContainsKey(chunkCoord))
    //            {
    //                voxelMods.Add(chunkCoord, new Queue<VoxelMod>());
    //            }
    //            while (data.Structures[chunkCoord].TryDequeue(out VoxelMod vm))
    //            {
    //                voxelMods[chunkCoord].Enqueue(vm);
    //            }

    //        }
    //    }
    //}

    //foreach (ChunkCoord coord in voxelMods.Keys)
    //{
    //    if (!TryGetChunkAtChunkCoord(coord, out Chunk chunk))
    //    {
    //        chunk = new Chunk(coord);
    //        _chunksOutsideLoadDistance.Add(coord, chunk);
    //    }
    //    chunk.EnqueueVoxelMods(voxelMods[coord]);
    //}




    private bool TryGetChunkAtChunkCoord(ChunkCoord coord, out Chunk chunk)
    {
        if (_chunksInLoadDistance.TryGetChunk(coord, out chunk))
        {
            return true;
        }
        if (_chunksOutsideLoadDistance.TryGetValue(coord, out chunk))
        {
            return true;
        }
        return false;
    }
}

