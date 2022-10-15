using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "MinecraftAsset/Biome Attribute")]
[Serializable]
public class BiomeAttributes : ScriptableObject {

    [Header("Major Flora")]
    [SerializeField]
    private string _biomeName;
    public string BiomeName { get => _biomeName;}

    [SerializeField]
    private int _offset;
    public int Offset { get => _offset;}

    [SerializeField]
    private float _scale;
    public float Scale { get => _scale;}
    
    [SerializeField]
    private int _terrainHeight;
    public int TerrainHeight { get => _terrainHeight;}

    [SerializeField]
    private float _terrainScale;
    public float TerrainScale { get => _terrainScale;}

    [SerializeField]
    private VoxelType _surfaceBlock;
    public VoxelType SurfaceBlock { get => _surfaceBlock;}

    [SerializeField]
    private VoxelType _subSurfaceBlock;
    public VoxelType SubSurfaceBlock { get=>_subSurfaceBlock; }

    [Header("Major Flora")]
    [SerializeField]
    private int _majorFloraIndex;
    public int MajorFloraIndex { get => _majorFloraIndex;}

    [SerializeField]
    private float _majorFloraZoneScale;
    public float MajorFloraZoneScale { get => _majorFloraZoneScale;}

    [Range(0.1f, 1f)]
    [SerializeField]
    private float _majorFloraZoneThreshold;
    public float MajorFloraZoneThreshold { get => _majorFloraZoneThreshold;}

    [SerializeField]
    private float _majorFloraPlacementScale;
    public float MajorFloraPlacementScale { get => _majorFloraPlacementScale; }

    [Range(0.1f, 1f)]
    [SerializeField]
    private float _majorFloraPlacementThreshold;
    public float MajorFloraPlacementThreshold { get => _majorFloraPlacementThreshold; }

    [SerializeField]
    private bool _placeMajorFlora = true;
    public bool PlaceMajorFlora { get => _placeMajorFlora; }

    [SerializeField]
    private int _maxHeight;
    public int MaxHeight { get => _maxHeight; }

    [SerializeField]
    private int _minHeight;
    public int MinHeight { get => _minHeight; }

    [SerializeField]
    private Lode[] _lodes;
    public Lode[] Lodes { get => _lodes; }
    
}

[System.Serializable]
public class Lode {
    [SerializeField]
    private string _nodeName;
    public string NodeName { get => _nodeName; }

    [SerializeField]
    private VoxelType _blockID;
    public VoxelType BlockID { get => _blockID; }

    [SerializeField]
    private int _minHeight;
    public int MinHeight { get => _minHeight; }

    [SerializeField]
    private int _maxHeight;
    public int MaxHeight { get => _maxHeight; }

    [SerializeField]
    private float _scale;
    public float Scale { get => _scale; }

    [SerializeField]
    private float _threshold;
    public float Threshold { get => _threshold; }

    [SerializeField]
    private float _noiseOffset;
    public float NoiseOffset { get => _noiseOffset; }

}
