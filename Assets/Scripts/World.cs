using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class World
{
    private readonly ChunkVoxelMapRepository _chunksOutsideLoadDistance;
    public ChunksInLoadDistance ChunksInLoadDistance { get; private set; }

    private readonly ChunkInRangeDictionary _chunksInViewDistance;
    

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
        _chunksOutsideLoadDistance = repo;
        ChunksInLoadDistance = new ChunksInLoadDistance(c => _chunksOutsideLoadDistance.TryAdd(c.ChunkCoord, c), loadDistance);
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

    private readonly object _locker = new();
    private void StartWorldUpdateWithoutUpdatingRange(CancellationToken token)
    {
        lock (_locker)
        {
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            watch.Restart();

            ChunksInLoadDistance.LoadVoxelDataForChunksInLoadDistance(_chunksOutsideLoadDistance, token);

            if (token.IsCancellationRequested)
            {
                return;
            }
            ChunksInLoadDistance.ApplyPendingMods(token);


            CreateRenderDataForChunksInViewDistance(token);

            watch.Stop();
            Debug.Log($"Updated world. Time ellapsed: {watch.ElapsedMilliseconds}ms");
        }
    }

   

    private void StartWorldUpdateAsync(CancellationToken token)
    {
        lock (_locker)
        {
            UpdateChunksInRange();

            if (token.IsCancellationRequested)
            {
                return;
            }

            StartWorldUpdateWithoutUpdatingRange(token);
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
                    chunk.ChunkCoord, chunk.ChunkVoxelMap, ChunksInLoadDistance.GetNeighbourChunks(chunk.ChunkCoord));
                _chunkRenderQueue.Enqueue(chunkMeshData);

                if (token.IsCancellationRequested)
                {
                    return;
                }
            }
        }
    }

   

    private void UpdateChunksInRange()
    {
        ChunksInLoadDistance.UpdateChunksInDistance(_playerChunkCoord,_chunksOutsideLoadDistance);

        _chunksInViewDistance.UpdateChunksInDistance(_playerChunkCoord,ChunksInLoadDistance);
    }

  
  

    

   



}

