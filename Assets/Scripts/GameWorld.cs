using System;
using System.Collections.Generic;

public class GameWorld : IGameWorldMainThreadInteractions
{
    private readonly ChunkInRangeDictionary _chunksInLoadDistance;
    private readonly ChunkInRangeDictionary _chunksInViewDistance;
    private readonly Dictionary<ChunkCoord, Chunk> _chunksOutsideLoadDistance = new();
    private readonly ChunkVoxelMapRepository _voxelMapRepository;

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
    }
    public GameWorld(int viewDistance, int loadDistance,ChunkVoxelMapRepository repo)
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

        Dictionary<ChunkCoord, Queue<VoxelMod>> voxelMods = new();

        foreach (Chunk chunk in _chunksInLoadDistance.Chunks)
        {
            if (!chunk.IsFilledIn)
            {
                (Voxel[,,] map, Dictionary<ChunkCoord, Queue<VoxelMod>> VoxelMod)
                                    = chunk.CreateOrRetrieveVoxelmap(_voxelMapRepository);
                if (!chunk.TrySetVoxelMap(map))
                {
                    throw new Exception(chunk.ChunkCoord + " Voxelmap generated for chunk which already contained one!");
                }


                foreach (ChunkCoord chunkCoord in voxelMods.Keys)
                {
                    if (!voxelMods.ContainsKey(chunkCoord))
                    {
                        voxelMods.Add(chunkCoord, new Queue<VoxelMod>());
                    }
                    while (voxelMods[chunkCoord].TryDequeue(out VoxelMod vm))
                    {
                        voxelMods[chunkCoord].Enqueue(vm);
                    }

                }
            }
        }

        foreach (ChunkCoord coord in voxelMods.Keys)
        {
            if (!TryGetChunkAtChunkCoord(coord, out Chunk chunk))
            {
                chunk = new Chunk(coord);
                _chunksOutsideLoadDistance.Add(coord,chunk);
            }
            chunk.EnqueueVoxelMods(voxelMods[coord]);            
        }

        //if (_map == null && _mapGenerationTask == null)
        //{
        //    _mapGenerationTask = Task.Run(() => _map = ChunkVoxelMapRepository.Instance.CreateOrRetrieveChunkVoxelMap(ChunkCoord))
        //        .ContinueWith((t) =>
        //        {
        //            if (t.IsFaulted)
        //            {
        //                Debug.LogError($"Could not create ChunkVoxelMap {ChunkCoord}, " + t.Exception);
        //            }
        //        });
        //}
    }


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

