using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UniRx;
using UnityEditor.UIElements;
using System.Runtime.CompilerServices;

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
    private CompositeDisposable _disposables = new CompositeDisposable();

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

    private event EventHandler<EventArgs> UpdateWorldEventAsked;
    IObservable<int> WorldIsReadySource;

    public World(int viewDistance, int loadDistance, ChunkVoxelMapRepository repo
        , GameSettings gameSettings, Transform worldTransform)
    {
        _voxelMapRepository = repo;
        ChunksInLoadDistance = new(disposeMethod: c => _chunksOutsideLoadDistance.TryAdd(c.ChunkCoord, c), loadDistance);
        _chunksInViewDistance = new(disposeMethod: c => { }, viewDistance);
        _chunkRenderer = new ChunkRenderer();
        _gameSettings = gameSettings;
        _worldTransform = worldTransform;

        Observable.FromEventPattern<EventHandler<EventArgs>, EventArgs>(
        h => h.Invoke, 
        h => UpdateWorldEventAsked += h, 
        h => UpdateWorldEventAsked -= h)
        .Subscribe(x => StartWorldUpdate(), // onSuccess
                   ex => Debug.LogException(ex)).AddTo(_disposables);

    }
    public void Player_Moved(object sender, ChunkCoord args)
    {
        _playerChunkCoord = args;
        OnUpdateWorldEventAsked();
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
        UpdateWorldEventAsked?.Invoke(this,EventArgs.Empty);
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

    private void StartWorldUpdate()
    {
        Task.Run(() => StartWorldUpdateAsync());
    }


    private async Task StartWorldUpdateAsync()
    {
        await UpdateChunksInRangeAsync();

        await CreateVoxelMapsForChunksInLoadDistance();

        await RenderChunksInViewDistanceTaskAsync();   
    }

    private async Task CreateVoxelMapsForChunksInLoadDistance()
    {
        IEnumerable<WorldGenerationData> generatedChunks = await Task.Run(() => CreateVoxelMapsTaskAsync(ChunksInLoadDistance.Chunks));

        await AddWorldGenerationDataToWorldAsync(generatedChunks);
    }

    public bool TryGetVoxel(GlobalVoxelPos globalPos, out Voxel voxel)
    {
        ChunkCoord coord = ChunkCoord.FromGlobalVoxelPosition(globalPos);
        if (TryGetChunkAtChunkCoord(coord, out Chunk chunk))
        {
            RelativeVoxelPos relPos = RelativeVoxelPos.CreateFromGlobal(globalPos);
            return chunk.TryGetVoxel(relPos, out voxel);
        }
        voxel = null;
        return false;
    }
       

    private async Task RenderChunksInViewDistanceTaskAsync()
    {
        List<Task> tasks = new();
        foreach (Chunk chunk in _chunksInViewDistance.Chunks)
        {
            if (chunk.RenderStatus == RenderStatus.NotUpToDate && chunk.IsFilledIn)
            {
                chunk.StartRender();

                tasks.Add(Task.Run(() =>
                {
                    ChunkMeshData chunkMeshData = _chunkRenderer.CreateChunkMeshData(chunk.ChunkCoord, chunk.ChunkVoxelMap, GetNeighbourChunks(chunk.ChunkCoord));
                    _chunkRenderQueue.Enqueue(chunkMeshData);
                }));
            }
        }
        await Task.WhenAll(tasks);
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

    private async Task UpdateChunksInRangeAsync()
    {
        await Task.Run(() =>
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
        });
    }

    private async Task AddWorldGenerationDataToWorldAsync(IEnumerable<WorldGenerationData> worldGenerationData)
    {
        await Task.Run(() =>
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
        });
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

