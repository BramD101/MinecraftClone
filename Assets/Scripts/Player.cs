using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public GameWorld GameWorld { get; set; }
    public PlayerSettings Settings { get; set; }
    public MenuSettings MenuSettings { get; set; }

    [SerializeField]
    private Transform _highlightBlock;
    public Transform HighlightBlock { get => _highlightBlock; }
    [SerializeField]
    private Transform _placeBlock;
    public Transform PlaceBlock { get => _placeBlock; }
    [SerializeField]
    private Toolbar _toolbar;
    [SerializeField]
    private Transform _camera;
    [SerializeField]
    private Transform _worldTransform;

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
        PlaceCursorBlocks();
    }
    [SerializeField]
    private float _verticalVelocity = 0.0f;
    private void FixedUpdate()
    {
        Vector3 currentPos = transform.position;

        // acceleration
        float verticalAcceleration = Settings.Gravity;
        _verticalVelocity += verticalAcceleration * Time.fixedDeltaTime;

        if (IsGrounded)
        {
            _verticalVelocity = Mathf.Max(0, _verticalVelocity);
        }

        var movementSpeed = _isSprinting ? Settings.SprintSpeed : Settings.WalkSpeed;
        // player inputs (Add with infinite acceleration)
        Vector3 proposedMovement =
            (((transform.forward * _movementDirection.y) + (transform.right * _movementDirection.x)) * movementSpeed
            + _verticalVelocity * _worldTransform.up) * Time.fixedDeltaTime;

        // collision detection with chunks

        var allowedMovement = MaxAllowedMovement(proposedMovement);
        if (allowedMovement.magnitude < proposedMovement.magnitude)
        {
            List<Vector3> allowedRemainingPrincipalMovementList = new();
            allowedRemainingPrincipalMovementList.Add(MaxAllowedMovement((proposedMovement - allowedMovement).x * Vector3.right));
            allowedRemainingPrincipalMovementList.Add(MaxAllowedMovement((proposedMovement - allowedMovement).y * Vector3.up));
            allowedRemainingPrincipalMovementList.Add(MaxAllowedMovement((proposedMovement - allowedMovement).z * Vector3.forward));

            Vector3 remainingMovement = allowedRemainingPrincipalMovementList.OrderBy(s => s.magnitude).Last();

            proposedMovement = MaxAllowedMovement(allowedMovement + remainingMovement);
        }



        _verticalVelocity = proposedMovement.y / Time.fixedDeltaTime;
        transform.Translate(proposedMovement, _worldTransform);
    }

    private Vector3 MaxAllowedMovement(Vector3 proposedMovement)
    {
        while (true)
        {
            if (!GameWorld.ContainsSolidVoxel(GlobalVoxelPosition<float>.FromVector3(transform.position + proposedMovement)))
            {
                break;
            }
            else
            {
                proposedMovement = (proposedMovement) / 2.0f;
                if ((proposedMovement).magnitude < Settings.BoundsTolerance)
                {
                    proposedMovement = Vector3.zero;
                    break;
                }
            }
        }
        return proposedMovement;
    }



    private void PlaceCursorBlocks()
    {
        var checkIncrement = 0.1f;

        float step = checkIncrement;
        Vector3 lastPos = new Vector3();

        while (step < Settings.Reach)
        {

            Vector3 pos = _camera.position + (_camera.forward * step);

            if (GameWorld.ContainsSolidVoxel(GlobalVoxelPosition<float>.FromVector3(pos)))
            {

                _highlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                _placeBlock.position = lastPos;

                _highlightBlock.gameObject.SetActive(true);
                _placeBlock.gameObject.SetActive(true);

                return;

            }

            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

            step += checkIncrement;

        }

        // not found
        _highlightBlock.gameObject.SetActive(false);
        _placeBlock.gameObject.SetActive(false);

    }

    private void OnPlayerMovedChunks()
    {
        PlayerMovedChunks?.Invoke(this, PlayerChunkCoord);
    }
    public void SetActive(bool isActive)
    {
        _playerGameObject.SetActive(isActive);
    }



    public void OnJump(InputAction.CallbackContext context)
    {
        if (!IsGrounded)
        {
            return;
        }
        if (_verticalVelocity > Settings.BoundsTolerance)
        {
            return;
        }

        _verticalVelocity += Settings.JumpForce;
    }

    [SerializeField]
    private Vector2 _movementDirection = Vector2.zero;
    public void OnMove(InputAction.CallbackContext context)
    {
        _movementDirection = context.ReadValue<Vector2>();

    }


    public void OnLookAround(InputAction.CallbackContext context)
    {
        Vector2 movement = context.ReadValue<Vector2>();

        transform.Rotate(Vector3.up * movement.x * MenuSettings.MouseSensitivity);
        _camera.Rotate(Vector3.right * -movement.y * MenuSettings.MouseSensitivity);
    }

    private bool _isSprinting = false;
    public void OnSprint(InputAction.CallbackContext context)
    {
        var isSprinting = context.ReadValue<float>();

        _isSprinting = isSprinting > 0;
    }
}

