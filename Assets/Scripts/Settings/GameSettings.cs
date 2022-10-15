using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "GameSettings", menuName = "MinecraftAsset/GameSettings")]
public class GameSettings : ScriptableObject
{
    [Range(0f, 1f)]
    [SerializeField]
    private float _globalLightLevel;
    public float GlobalLightLevel { get => _globalLightLevel; }

    [SerializeField]
    private Color _day;
    public Color Day { get => _day; }

    [SerializeField]
    private Color _night;
    public Color Night { get => _night; }

    [SerializeField]
    private Material _material;
    public Material Material { get => _material; }

    [SerializeField]
    private Material _transparentMaterial;
    public Material TransparentMaterial { get => _transparentMaterial; }

    [SerializeField]
    private Material _waterMaterial;
    public Material WaterMaterial { get => _waterMaterial; }

    [SerializeField]
    private VoxelTypeData[] _blocktypes;
    public VoxelTypeData[] Blocktypes { get => _blocktypes; }
     
    [SerializeField]
    private string _appPath;
    public string AppPath { get => _appPath; }


}

