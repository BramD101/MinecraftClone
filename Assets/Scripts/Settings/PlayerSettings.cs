
using UnityEngine;


public class PlayerSettings : MonoBehaviour
{
    [SerializeField]
    private bool _isGrounded;
    public bool IsGrounded { get => _isGrounded; } 

    [SerializeField]
    private bool _isSprinting;
    public bool IsSprinting{get => _isSprinting;}

    [SerializeField]
    private float _walkSpeed = 3f;
    public float WalkSpeed { get => _walkSpeed; } 

    [SerializeField]
    private float _sprintSpeed = 6f;
    public float SprintSpeed { get => _sprintSpeed; } 

    [SerializeField]
    private float _jumpForce = 5f;
    public float JumpForce { get => _jumpForce; } 

    [SerializeField]
    private float _gravity = -9.8f;
    public float Gravity { get => _gravity; } 

    [SerializeField]
    private float _playerWidth = 0.15f;
    public float PlayerWidth { get => _playerWidth; } 

    [SerializeField]
    private float _boundsTolerance = 0.1f;
    public float BoundsTolerance { get => _boundsTolerance; } 

    [SerializeField]
    private int _orientation;
    public int Orientation { get => _orientation; } 

    [SerializeField]
    private Transform _highlightBlock;
    public Transform HighlightBlock { get => _highlightBlock; } 

    [SerializeField]
    private Transform _placeBlock;
    public Transform PlaceBlock { get => _placeBlock; } 

    [SerializeField]
    private float _checkIncrement = 0.1f;
    public float CheckIncrement { get => _checkIncrement; } 

    [SerializeField]
    private float _reach = 8f;
    public float Reach { get => _reach; } 

    [SerializeField]
    private Toolbar _toolbar;
    public Toolbar Toolbar { get => _toolbar; } 
}

