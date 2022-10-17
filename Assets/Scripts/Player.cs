using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public GameWorld GameWorld { get; set; }
    public PlayerSettings Settings { get; set; }

    [SerializeField]
    private Transform _highlightBlock;
    public Transform HighlightBlock { get => _highlightBlock; }

    [SerializeField]
    private Transform _placeBlock;
    public Transform PlaceBlock { get => _placeBlock; }
    [SerializeField]
    private Toolbar _toolbar;

    public Toolbar Toolbar { get => _toolbar; }
    private GameObject _playerGameObject;
    private ChunkCoord _playerChunkCoord;
    public event EventHandler<ChunkCoord> PlayerMovedChunks;

    public bool IsGrounded
    {
        get
        {
            GlobalVoxelPosition<float> pos = GlobalVoxelPosition<float>.FromVector3(
                _playerGameObject.transform.position
                + new Vector3(0, -2.0f * Settings.BoundsTolerance, 0)
                );
            return GameWorld.ContainsSolidVoxel(pos);
        }
    }


    private Vector3 _speed = new();
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
    private void Start()
    {
        _playerGameObject = GameObject.Find("Player");
        _playerGameObject.transform.position = new Vector3(0, VoxelData.ChunkHeight, 0);
        _playerGameObject.SetActive(false);
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
    private void FixedUpdate()
    {
        Vector3 currentPos = _playerGameObject.transform.position;

        // acceleration
        Vector3 acceleration = new(0, Settings.Gravity, 0);
        _speed = _speed + (acceleration * Time.fixedDeltaTime);

        Vector3 proposedNewPos = currentPos + (_speed * Time.fixedDeltaTime);


        while (true)
        {
            if (!GameWorld.ContainsSolidVoxel(new GlobalVoxelPosition<float>(proposedNewPos.x, proposedNewPos.y, proposedNewPos.z)))
            {
                break;
            }
            else
            {
                proposedNewPos = currentPos + ((proposedNewPos - currentPos) / 2.0f);
                if ((proposedNewPos - currentPos).magnitude < Settings.BoundsTolerance)
                {
                    proposedNewPos = currentPos;
                    _speed.y = 0;
                    break;
                }
            }
        }
        _playerGameObject.transform.position = proposedNewPos;
    }
    private void OnPlayerMovedChunks()
    {
        PlayerMovedChunks?.Invoke(this, PlayerChunkCoord);
    }
    public void SetActive(bool isActive)
    {
        _playerGameObject.SetActive(isActive);
    }


    // Inputs
    private void OnJump()
    {
        if (!IsGrounded)
        {
            return;
        }
        if (_speed.y > Settings.BoundsTolerance)
        {
            return;
        }

        _speed += new Vector3(0f, Settings.JumpForce, 0f);
    }
}
   