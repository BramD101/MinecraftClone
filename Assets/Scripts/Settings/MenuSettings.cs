using System;
using UnityEngine;



[System.Serializable]
public class MenuSettings
{
    [Header("Game Data")]
    [SerializeField]
    private string _version = "0.0.0.01";

    [Header("Performance")]
    private int _loadDistance;
    public int LoadDistance { get => _loadDistance; }

    [SerializeField]
    private int _viewDistance;
    public int ViewDistance { get => _viewDistance; } 

    [SerializeField]
    private bool _dynamicLighting;
    public bool DynamicLighting { get => _dynamicLighting; } 

    [SerializeField]
    private CloudStyle _clouds;
    public CloudStyle Clouds { get => _clouds; } 

    [SerializeField]
    private bool _enableAnimatedChunks;
    public bool EnableAnimatedChunks { get => _enableAnimatedChunks; } 


    [Header("Controls")]
    [Range(0.1f, 10f)]
    [SerializeField]
    private float _mouseSensitivity;
    public float MouseSensitivity { get => _mouseSensitivity; } 
}

