using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class World
{
    public ChunkInRangeDictionary ChunksInLoadDistance { get; private set; }
    private readonly ChunkInRangeDictionary _chunksInViewDistance;
    private readonly ChunkConcurrentRepository _chunksOutsideLoadDistance = new();
    private readonly ChunkVoxelMapRepository _voxelMapRepository;
    private readonly ConcurrentQueue<ChunkMeshData> _chunkRenderQueue = new();
    private ChunkCoord _playerChunkCoord;
    private readonly ChunkRenderer _chunkRenderer;
    private readonly GameSettings _gameSettings;
    private readonly Transform _worldTransform;

    private bool _worldIsReady = false;
    public bool WorldIsReady
    {
        get
        {
            if (!_worldIsReady)
            {
                if (_chunksInViewDistance.Chunks.All(c => c.IsRendered()))
                {
                    OnWorldIsReady();
                    _worldIsReady = true;
                }
            }

            return _worldIsReady;
        }
    }

    public event EventHandler WorldIsReadyEvent;

    private event EventHandler UpdateWorldEventAsked;

    private readonly IObservable<int> WorldIsReadySource;

    public World(int viewDistance, int loadDistance, ChunkVoxelMapRepository repo
        , GameSettings gameSettings, Transform worldTransform)
    {
        _voxelMapRepository = repo;
        ChunksInLoadDistance = new(disposeMethod: c => _chunksOutsideLoadDistance.TryAdd(c.ChunkCoord, c), loadDistance);
        _chunksInViewDistance = new(disposeMethod: c => { }, viewDistance);
        _chunkRenderer = new ChunkRenderer();
        _gameSettings = gameSettings;
        _worldTransform = worldTransform;

        UpdateWorldEventAsked += World_StartUpdate;
        
    }
    public void Player_Moved(object sender, ChunkCoord args)
    {
        _playerChunkCoord = args;
        OnUpdateWorldEventAsked();
    }
    public void World_StartUpdate(object sender, EventArgs args)
    {
        StartWorldUpdate();
    }

    
    private void OnWorldIsReady()
    {
        WorldIsReadyEvent?.Invoke(this, EventArgs.Empty);
    }

    public void Start()
    {
        OnUpdateWorldEventAsked();
    }

    public void OnUpdateWorldEventAsked()
    {
        UpdateWorldEventAsked?.Invoke(this, EventArgs.Empty);
    }


    public void PrintRenderedChunkToScreen()
    {
        if (_chunkRenderQueue.TryDequeue(out ChunkMeshData chunkMeshDataDTO))
        {
            if (_chunksInViewDistance.TryGetChunk(chunkMeshDataDTO.ChunkCoord, out Chunk chunk))
            {
                chunk.UpdateMesh(chunkMeshDataDTO, _gameSettings, _worldTransform);
            }
        }
    }

    private CancellationTokenSource _cancelCurrentTaskSource = new();
    private void StartWorldUpdate()
    {
        StartWorldTask(StartWorldUpdateAsync);
    }
    public void StartWorldUpdateWithoutUpdatingRange()
    {
        StartWorldTask(StartWorldUpdateWithoutUpdatingRange);
    }

    private void StartWorldTask(Action<CancellationToken> task)
    {
        _cancelCurrentTaskSource.Cancel();

        _cancelCurrentTaskSource = new CancellationTokenSource();

        Task.Run(() => task(_cancelCurrentTaskSource.Token));
    }

    private object _locker = new();
    private void StartWorldUpdateWithoutUpdatingRange(CancellationToken token)
    {
        lock (_locker)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            watch.Restart();

            LoadVoxelDataForChunksInLoadDistance(token);           

            if (token.IsCancellationRequested)
            {
                return;
            }
           
            ApplyPendingMods(token);                 

            CreateRenderDataForChunksInViewDistance(token);
            
            watch.Stop();
            Debug.Log($"Updated world. Time ellapsed: {(watch.ElapsedMilliseconds)}ms");
        }
    }

    private void ApplyPendingMods(CancellationToken token)
    {
        foreach (var chunk in ChunksInLoadDistance.Chunks)
        {
            chunk.ChunkVoxelMap.TryUpdateBackLog();
        }
    }

    private void StartWorldUpdateAsync(CancellationToken token)
    {
        lock (_locker)
        {
            UpdateChunksInRangeAsync();

            if (token.IsCancellationRequested)
            {
                return;
            }

            StartWorldUpdateWithoutUpdatingRange(token);
        }
    }

    private void LoadVoxelDataForChunksInLoadDistance(CancellationToken token)
    {
        IEnumerable<WorldGenerationData> generatedChunks = LoadVoxelData(ChunksInLoadDistance.Chunks, token);

        if (generatedChunks != null)
        {
            AddWorldGenerationDataToWorldAsync(generatedChunks);
        }
    }


    private void CreateRenderDataForChunksInViewDistance(CancellationToken token)
    {
        foreach (Chunk chunk in _chunksInViewDistance.Chunks)
        {
            if (chunk.RenderStatus == RenderStatus.NotUpToDate && chunk.IsFilledIn)
            {
                chunk.StartRender();
                ChunkMeshData chunkMeshData = _chunkRenderer.CreateChunkMeshData(
                    chunk.ChunkCoord, chunk.ChunkVoxelMap, GetNeighbourChunks(chunk.ChunkCoord));
                _chunkRenderQueue.Enqueue(chunkMeshData);

                if (token.IsCancellationRequested)
                {
                    return;
                }
            }
        }
    }

    private ChunksNeighbours GetNeighbourChunks(ChunkCoord coord)
    {
        ChunksNeighbours neighbours = new();

        foreach (ChunkNeighbourType neighbourtype in ChunksNeighbours.GetChunkNeighbourTypes())
        {
            ChunkCoord neighbourCoord = ChunksNeighbours.GetNeighbourCoord(coord, neighbourtype);
            neighbours.Add(neighbourtype, GetChunkAtChunkCoord(neighbourCoord));
        }
        return neighbours;
    }

    private void UpdateChunksInRangeAsync()
    {
        ChunksInLoadDistance.UpdateChunksInDistance(_playerChunkCoord, c =>
        {
            bool success = _chunksOutsideLoadDistance.TryGetValue(c, out Chunk outChunk);
            return (outChunk, success);
        });

        _chunksInViewDistance.UpdateChunksInDistance(_playerChunkCoord, c =>
        {
            bool success = ChunksInLoadDistance.TryGetChunk(c, out Chunk outChunk);
            return (outChunk, success);
        });
    }

    private void AddWorldGenerationDataToWorldAsync(IEnumerable<WorldGenerationData> worldGenerationData)
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
    private IEnumerable<WorldGenerationData> LoadVoxelData(List<Chunk> chunks, CancellationToken token)
    {
        ConcurrentDictionary<ChunkCoord, WorldGenerationData> worldGenDataDict = new();

        List<Task<WorldGenerationData>> tasks = new();
        List<WorldGenerationData> results = new();
        foreach (Chunk chunk in chunks)
        {
            if (!chunk.IsFilledIn)
            {
                results.Add(chunk.CreateOrRetrieveVoxelmap(_voxelMapRepository));
            }
            if (token.IsCancellationRequested)
            {
                return null;
            }
        }


        return results;
    }

    private Chunk GetChunkAtChunkCoord(ChunkCoord coord)
    {
        if (!TryGetChunkAtChunkCoord(coord, out Chunk chunk))
        {
            throw new Exception("Chunk not found");
        }
        return chunk;
    }

    private bool TryGetChunkAtChunkCoord(ChunkCoord coord, out Chunk chunk)
    {
        if (ChunksInLoadDistance.TryGetChunk(coord, out chunk))
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

