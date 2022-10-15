
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "PlayerSettings", menuName = "MinecraftAsset/PlayerSettings")]
public class PlayerSettings : ScriptableObject
{
    [SerializeField]
    private float _walkSpeed;
    public float WalkSpeed { get => _walkSpeed; } 

    [SerializeField]
    private float _sprintSpeed;
    public float SprintSpeed { get => _sprintSpeed; } 

    [SerializeField]
    private float _jumpForce;
    public float JumpForce { get => _jumpForce; } 

    [SerializeField]
    private float _gravity;
    public float Gravity { get => _gravity; } 

    [SerializeField]
    private float _playerWidth;
    public float PlayerWidth { get => _playerWidth; } 

    [SerializeField]
    private float _boundsTolerance;
    public float BoundsTolerance { get => _boundsTolerance; } 

    [SerializeField]
    private int _orientation;
    public int Orientation { get => _orientation; } 



    [SerializeField]
    private float _checkIncrement;
    public float CheckIncrement { get => _checkIncrement; } 

    [SerializeField]
    private float _reach;
    public float Reach { get => _reach; } 


}

