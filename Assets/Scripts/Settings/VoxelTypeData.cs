using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
[CreateAssetMenu(fileName = "VoxelTypeData", menuName = "MinecraftAsset/VoxelTypeData")]
public class VoxelTypeData : ScriptableObject
{
    [SerializeField]
    private VoxelType _voxelType;

    [SerializeField]
    private bool _isSolid;
    public bool IsSolid => _isSolid;

    [SerializeField]
    private VoxelMeshData _meshData;
    public VoxelMeshData MeshData => _meshData;
    public bool RenderNeighborFaces => _opacity < 15;
    [SerializeField]
    private bool _isWater;
    public bool IsWater => _isWater;
    [SerializeField]
    private byte _opacity;
    [SerializeField]
    private Sprite _icon;
    [SerializeField]
    private bool _isActive;
    [SerializeField]
    private bool _isGravityAffected;
    [Header("Texture Values")]
    [SerializeField]
    private int _backFaceTexture;
    [SerializeField]
    private int _frontFaceTexture;
    [SerializeField]
    private int _topFaceTexture;
    [SerializeField]
    private int _bottomFaceTexture;
    [SerializeField]
    private int _leftFaceTexture;
    [SerializeField]
    private int _rightFaceTexture;

    public int GetTextureID(Direction direction)
    {

        switch (direction)
        {

            case Direction.LowerZ:
                return _backFaceTexture;
            case Direction.HigherZ:
                return _frontFaceTexture;
            case Direction.LowerY:
                return _topFaceTexture;
            case Direction.HigherY:
                return _bottomFaceTexture;
            case Direction.LowerX:
                return _leftFaceTexture;
            case Direction.HigherX:
                return _rightFaceTexture;
            default:
                Debug.Log("Error in GetTextureID; invalid face index");
                return 0;
              
        }

    }
}

