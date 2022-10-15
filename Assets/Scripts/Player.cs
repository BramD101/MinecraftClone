using System;
using UnityEngine;

public class Player : MonoBehaviour, IPlayer
{
    [SerializeField]
    private Transform _highlightBlock;
    public Transform HighlightBlock { get => _highlightBlock; }

    [SerializeField]
    private Transform _placeBlock;
    public Transform PlaceBlock { get => _placeBlock; }
    [SerializeField]
    private Toolbar _toolbar;
    public Toolbar Toolbar { get => _toolbar; }

    private ChunkCoord _playerChunkCoord;
    public ChunkCoord PlayerChunkCoord
    {
        get { return _playerChunkCoord; }
        private set
        {
            _playerChunkCoord = value;
            OnPlayerMovedChunks();
        }
    }

    public GlobalVoxelPosition<float> GlobalPosition
    {
        get
        {
            Vector3 pos = _playerGameObject.transform.position;
            return new GlobalVoxelPosition<float>(pos.x, pos.y, pos.z);
        }
    }

    private GameObject _playerGameObject;

    public event EventHandler<ChunkCoord> PlayerMovedChunks;

    private void OnPlayerMovedChunks()
    {
        PlayerMovedChunks?.Invoke(this, PlayerChunkCoord);
    }

    private void Start()
    {
        _playerGameObject = GameObject.Find("Player");

        PlayerChunkCoord = ChunkCoord.FromGlobalVoxelPosition(GlobalPosition);
    }
    private void Update()
    {
        ChunkCoord playerChunkCoord = ChunkCoord.FromGlobalVoxelPosition(GlobalPosition);

        if (!PlayerChunkCoord.Equals(playerChunkCoord))
        {
            PlayerChunkCoord = playerChunkCoord;
        }
    }
}