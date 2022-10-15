using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
public class GameWorld
{
    private readonly ChunkInRangeDictionary _chunksInLoadDistance;
    private readonly ChunkInRangeDictionary _chunksInViewDistance;
    private readonly Dictionary<ChunkCoord, Chunk> _chunksOutsideLoadDistance = new();
    private readonly ChunkVoxelMapRepository _voxelMapRepository;
    private Task<IEnumerable<WorldGenerationData>> _voxelMapCreationTask;
    private readonly System.Diagnostics.Stopwatch _voxelMapCreationTaskWatch = new();
    private readonly ConcurrentQueue<ChunkMeshDataDTO> _chunkRenderQueue = new();
    private ChunkCoord _playerChunkCoord;
    private readonly ChunkRenderer _chunkRenderer;
    private readonly GameSettings _gameSettings;
    private readonly Transform _worldTransform;
    public GameWorld(int viewDistance, int loadDistance, ChunkVoxelMapRepository repo
        , GameSettings gameSettings, Transform worldTransform)
    {
        _voxelMapRepository = repo;
        _chunksInLoadDistance = new(disposeMethod: c => _chunksOutsideLoadDistance.TryAdd(c.ChunkCoord, c), loadDistance);
        _chunksInViewDistance = new(disposeMethod: c => { }, viewDistance);
        _chunkRenderer = new ChunkRenderer(this);
        _gameSettings = gameSettings;
        _worldTransform = worldTransform;
    }
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

        if (_chunkRenderQueue.TryDequeue(out ChunkMeshDataDTO chunkMeshDataDTO))
        {
            if (_chunksInViewDistance.TryGetChunk(chunkMeshDataDTO.ChunkCoord, out Chunk chunk))
            {
                chunk.UpdateMesh(chunkMeshDataDTO, _gameSettings, _worldTransform);

            }
        }
        RenderChunksInViewDistance();
    }
    public bool TryGetVoxel(GlobalVoxelPosition<int> globalPos, out Voxel voxel)
    {
        ChunkCoord coord = ChunkCoord.FromGlobalVoxelPosition(globalPos);
        if (TryGetChunkAtChunkCoord(coord, out Chunk chunk))
        {
            RelativeToChunkVoxelPosition<int> relPos = RelativeToChunkVoxelPosition<int>.CreateFromGlobal(globalPos);
            voxel = chunk.GetVoxel(relPos);
            return true;
        }
        voxel = null;
        return false;
    }

    private void RenderChunksInViewDistance()
    {
        foreach (Chunk chunk in _chunksInViewDistance.Chunks)
        {
            if (chunk.RenderStatus == RenderStatus.NotUpToDate && chunk.IsFilledIn)
            {
                chunk.StartRender();

                Task task = Task.Run(() =>
                {
                    ChunkMeshDataDTO chunkMeshData = _chunkRenderer.CreateChunkMeshData(chunk.ChunkCoord, chunk.ChunkVoxelMap);
                    _chunkRenderQueue.Enqueue(chunkMeshData);
                });
            }
        }
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
        if (_voxelMapCreationTask == null)
        {
            _voxelMapCreationTask = Task.Run(() => CreateVoxelMapsTaskAsync(_chunksInLoadDistance.Chunks));
            _voxelMapCreationTaskWatch.Start();
        }
    }
    private void UpdateCreateVoxelMaps()
    {
        if (_voxelMapCreationTask != null)
        {
            if (_voxelMapCreationTask.IsCompletedSuccessfully)
            {
                _voxelMapCreationTaskWatch.Stop();
                Debug.Log($"CreateVoxelMapsTaskAsync(async) afgehandeld in {_voxelMapCreationTaskWatch.ElapsedMilliseconds}ms");
                _voxelMapCreationTaskWatch.Reset();

                System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();

                OnVoxelMapCreationTaskSuccessfull(_voxelMapCreationTask.Result);
                _voxelMapCreationTask = null;

                watch.Stop();
                Debug.Log($"OnVoxelMapCreationTaskSuccessfull afgehandeld in {watch.ElapsedMilliseconds}ms");
            }
        }
    }
    private void OnVoxelMapCreationTaskSuccessfull(IEnumerable<WorldGenerationData> worldGenerationData)
    {
        // set voxels 
        Dictionary<ChunkCoord, Queue<VoxelMod>> voxelmods = new();
        foreach (WorldGenerationData result in worldGenerationData)
        {
            if (TryGetChunkAtChunkCoord(result.ChunkCoord, out Chunk chunk))
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
            if (!TryGetChunkAtChunkCoord(coord, out chunk))
            {
                chunk = new Chunk(coord);
            }
            chunk.EnqueueVoxelMods(voxelmods[coord]);
        }
    }
    private async Task<IEnumerable<WorldGenerationData>> CreateVoxelMapsTaskAsync(List<Chunk> chunks)
    {
        Debug.Log("Start CreateVoxelMapsTask(async)");

        ConcurrentDictionary<ChunkCoord, WorldGenerationData> worldGenDataDict = new();

        List<Task<WorldGenerationData>> tasks = new();
        foreach (Chunk chunk in chunks)
        {
            if (!chunk.IsFilledIn)
            {
                tasks.Add(Task.Run(() => chunk.CreateOrRetrieveVoxelmap(_voxelMapRepository)));
            }
        }
        WorldGenerationData[] results = await Task.WhenAll(tasks);

        return results;
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

