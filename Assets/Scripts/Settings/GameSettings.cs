using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class GameSettings : MonoBehaviour
{
    [SerializeField]
    private MenuSettings _menuSettings;
    public MenuSettings MenuSettings { get => _menuSettings; }

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
    private Vector3 _spawnPosition;
    public Vector3 SpawnPosition { get => _spawnPosition; }

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
    private BlockType[] _blocktypes;
    public BlockType[] Blocktypes { get => _blocktypes; }

    [SerializeField]
    private Clouds _clouds;
    public Clouds Clouds { get => _clouds; }

    [SerializeField]
    private GameObject _debugScreen;
        public GameObject DebugScreen { get => _debugScreen; }

    [SerializeField]
    private GameObject _creativeInventoryWindow;
    public GameObject CreativeInventoryWindow { get => _creativeInventoryWindow; }

    [SerializeField]
    private GameObject _cursorSlot;
    public GameObject CursorSlot { get => _cursorSlot; }

    [SerializeField]
    private string _appPath;
    public string AppPath { get => _appPath; }

    [SerializeField]
    private BiomeAttributes[] _biomes;
    public BiomeAttributes[] Biomes { get => _biomes; }

}

