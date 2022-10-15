
using System;
using UnityEngine;

public class Voxel
{
    public static GameSettings _gameSettings;

    public VoxelMinimal _voxelMinimal { get; private set; }
    public VoxelTypeData _voxelTypeData { get; private set; }

    public VoxelType Type => _voxelMinimal.Type;
    public bool RenderNeighborFaces => _voxelTypeData.RenderNeighborFaces;
    public bool IsWater => _voxelTypeData.IsWater;
    public bool IsSolid => _voxelTypeData.IsSolid;
    public VoxelMeshData MeshData => _voxelTypeData.MeshData;
    
    public Voxel(VoxelMinimal voxelMinimal, VoxelTypeData voxelTypeData)
    {
        _voxelMinimal = voxelMinimal;
        _voxelTypeData = voxelTypeData;
    }
    public int GetTextureID(Direction direction)
    {
        return _voxelTypeData.GetTextureID(direction);
    }
    public static Voxel CreateFromType(VoxelType type)
    {
        return new Voxel
        (
            new VoxelMinimal() { Type = type },
            _gameSettings.Blocktypes[(int)type]
        );
    }
}

public struct VoxelMinimal
{
    private byte _type;
    public VoxelType Type { get => (VoxelType)_type; set => _type = (byte)value; }
}

public enum VoxelType : byte
{
    Air = 0,
    Bedrock = 1,
    Stone = 2,
    Grass = 3,
    Sand = 4,
    Dirt = 5,
    Wood = 6,
    Planks = 7,
    Furnace = 8,
    Cobblestone = 9,
    Glass = 10,
    Leaves = 11,
    Cactus = 12,
    CactusTop = 32,
    Water = 14
}

