using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;
using Unity.VisualScripting;
using Assets.Scripts;
using System;
using System.Collections.Concurrent;
using UnityEngine.SceneManagement;
using System.Linq;

public class World : MonoBehaviour
{

    public Settings settings;


    [Range(0f, 1f)]
    public float globalLightLevel;
    public Color day;
    public Color night;

    public Transform player;
    public Player _player;
    public Vector3 spawnPosition;

    public Material material;
    public Material transparentMaterial;
    public Material waterMaterial;

    public BlockType[] blocktypes;

    //Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

    public ConcurrentUniqueQueue<ChunkCoord> ActiveChunks = new ConcurrentUniqueQueue<ChunkCoord>();
    public ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;


    public ConcurrentQueue<Chunk> chunksToDraw = new ConcurrentQueue<Chunk>();
    private bool _worldIsReady;
    public bool WorldIsReady
    {
        get
        {
            if (!_worldIsReady)
            {
                List<ChunkCoord> chunksInViewDistance = GetChunksInViewDistance();
                            
                _worldIsReady = chunksInViewDistance.All(l => worldData.RequestChunk(l.x, l.z, false)?.chunk.IsRendered ?? false);
                if (_worldIsReady)
                {
                    Debug.Log($"World is ready, time = {Time.realtimeSinceStartup}s");
                }
                return _worldIsReady;
            }
            else
            {
                return _worldIsReady;
            }

        }
        private set { _worldIsReady = value; }
    }

    private bool _inUI = false;

    public Clouds clouds;

    public GameObject debugScreen;

    public GameObject creativeInventoryWindow;
    public GameObject cursorSlot;


    public object ChunkListThreadLock = new object();

    private static World _instance;
    public static World Instance { get { return _instance; } }

    public WorldData worldData;

    public string appPath;

    private void Awake()
    {
        WorldIsReady = false;
        // If the instance value is not null and not *this*, we've somehow ended up with more than one World component.
        // Since another one has already been assigned, delete this one.
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        // Else set this to the instance.
        else
            _instance = this;

        appPath = Application.persistentDataPath;

        _player = player.GetComponent<Player>();


        spawnPosition = new Vector3(VoxelData.WorldCentre, VoxelData.ChunkHeight - 50f, VoxelData.WorldCentre);
        player.position = spawnPosition;
        worldData = new WorldData();

    }

    private void Start()
    {
        using (var logger = new DebugLogger())
        {
            Debug.Log("Generating new world using seed " + VoxelData.seed);


            worldData = SaveSystem.LoadWorld("Testing");
            BackgroundThread.Instance.Setup(worldData, ActiveChunks);
           


            string jsonImport = File.ReadAllText(Application.dataPath + "/settings.cfg");
            settings = JsonUtility.FromJson<Settings>(jsonImport);


            UnityEngine.Random.InitState(VoxelData.seed);

            Shader.SetGlobalFloat("minGlobalLightLevel", VoxelData.minLightLevel);
            Shader.SetGlobalFloat("maxGlobalLightLevel", VoxelData.maxLightLevel);

            LoadWorld();
            CreateWorld();

            SetGlobalLightValue();

            UpdateChunksInViewDistance();
            playerLastChunkCoord = GetChunkCoordFromVector3(player.position);



            StartCoroutine(Tick());

            // only start when everything is loaded
            BackgroundThread.Instance.Start();
        }
    }

    public void SetGlobalLightValue()
    {

        Shader.SetGlobalFloat("GlobalLightLevel", globalLightLevel);
        Camera.main.backgroundColor = Color.Lerp(night, day, globalLightLevel);

    }

    IEnumerator Tick()
    {

        while (true)
        {
            
            ActiveChunks.DoActionOnAllMembers(l => worldData.RequestChunk(l.x, l.z, false)?.chunk.TickUpdate());

            yield return new WaitForSeconds(VoxelData.tickLength);

        }

    }


    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.position);

        // Only update the chunks if the player has moved from the chunk they were previously on.
        if (!playerChunkCoord.Equals(playerLastChunkCoord))
        {
            UpdateChunksInViewDistance();
        }

        if (chunksToDraw.Count > 0)
        {
            Chunk result;
            if (chunksToDraw.TryDequeue(out result))
            {
                result.CreateMesh();
            }
        }


        if (Input.GetKeyDown(KeyCode.F3))
            debugScreen.SetActive(!debugScreen.activeSelf);

        if (Input.GetKeyDown(KeyCode.F1))
            SaveSystem.SaveWorld(worldData);

        if (Input.GetKeyDown(KeyCode.F5))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }


    }

    void LoadWorld()
    {
        using (var debugLogger = new DebugLogger())
        {
            for (int x = (VoxelData.WorldSizeInChunks / 2) - settings.loadDistance; x < (VoxelData.WorldSizeInChunks / 2) + settings.loadDistance; x++)
            {
                for (int z = (VoxelData.WorldSizeInChunks / 2) - settings.loadDistance; z < (VoxelData.WorldSizeInChunks / 2) + settings.loadDistance; z++)
                {

                    worldData.LoadChunk(new ChunkCoord(x, z));

                }
            }
        }        
    }
    void CreateWorld()
    {
        using (var debugLogger = new DebugLogger())
        {
            for (int x = (VoxelData.WorldSizeInChunks / 2) - settings.loadDistance; x < (VoxelData.WorldSizeInChunks / 2) + settings.loadDistance; x++)
            {
                for (int z = (VoxelData.WorldSizeInChunks / 2) - settings.loadDistance; z < (VoxelData.WorldSizeInChunks / 2) + settings.loadDistance; z++)
                {

                    worldData.CreateChunk(new ChunkCoord(x, z));

                }
            }
        }
    }









    private void OnDisable()
    {
        BackgroundThread.Instance.Abort();
    }



    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, z);

    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return worldData.RequestChunk(x, z, false).chunk;

    }

    void UpdateChunksInViewDistance()
    {

        clouds.UpdateClouds();


        playerLastChunkCoord = playerChunkCoord;

        ConcurrentUniqueQueue<ChunkCoord> previouslyActiveChunks = ActiveChunks.DeepCopy();

        ActiveChunks.Clear();

        // Loop through all chunks currently within view distance of the player.

        foreach (var chunkCoord in GetChunksInViewDistance())
        {

            // If the current chunk is in the world...
            if (IsChunkInWorld(chunkCoord))
            {

                ChunkData ChunkData = worldData.RequestChunk(chunkCoord.x, chunkCoord.z, true);
                // Check if it active, if not, activate it.


                ChunkData.IsActive = true;
                ActiveChunks.TryEnqueue(chunkCoord);
            }

            // Check through previously active chunks to see if this chunk is there. If it is, remove it from the list.
           
            previouslyActiveChunks.TryRemove(chunkCoord);

            
        }
        
        // Any chunks left in the previousActiveChunks list are no longer in the player's view distance, so loop through and disable them.
        previouslyActiveChunks.DoActionOnAllMembers(l => worldData.RequestChunk(l.x, l.z, false).IsActive = false);
    }

    public List<ChunkCoord> GetChunksInViewDistance()
    {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        var chunksInViewDistance = new List<ChunkCoord>();
        for (int x = coord.x - settings.viewDistance; x < coord.x + settings.viewDistance; x++)
        {
            for (int z = coord.z - settings.viewDistance; z < coord.z + settings.viewDistance; z++)
            {
                chunksInViewDistance.Add(new ChunkCoord(x, z));
            }
        }
        return chunksInViewDistance;
    }

    public bool CheckForVoxel(Vector3 pos)
    {

        var voxel = worldData.GetVoxel(pos);

        if (voxel == null)
        {
            return false;
        }

        if (blocktypes[voxel.id].isSolid)
            return true;
        else
            return false;

    }

    public VoxelState GetVoxelState(Vector3 pos)
    {

        return worldData.GetVoxel(pos);

    }

    public bool inUI
    {

        get { return _inUI; }

        set
        {

            _inUI = value;
            if (_inUI)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                creativeInventoryWindow.SetActive(true);
                cursorSlot.SetActive(true);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                creativeInventoryWindow.SetActive(false);
                cursorSlot.SetActive(false);
            }
        }

    }


    bool IsChunkInWorld(ChunkCoord coord)
    {

        if (coord.x > 0 && coord.x < VoxelData.WorldSizeInChunks - 1 && coord.z > 0 && coord.z < VoxelData.WorldSizeInChunks - 1)
            return true;
        else
            return
                false;

    }

    public bool IsVoxelInWorld(Vector3 pos)
    {

        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.ChunkHeight && pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
            return true;
        else
            return false;

    }
    public void AddChunkToUpdate(Chunk chunk)
    {
        BackgroundThread.Instance.EnQueueuChunkToUpdate(chunk);
    }
  

    public void GenerateStructure(Queue<VoxelMod> structure)
    {
        BackgroundThread.Instance.EnQueueModification(structure);
    }
}

[System.Serializable]
public class BlockType
{

    public string blockName;
    public bool isSolid;
    public VoxelMeshData meshData;
    public bool renderNeighborFaces;
    public bool isWater;
    public byte opacity;
    public Sprite icon;
    public bool isActive;

    [Header("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    // Back, Front, Top, Bottom, Left, Right

    public int GetTextureID(int faceIndex)
    {

        switch (faceIndex)
        {

            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("Error in GetTextureID; invalid face index");
                return 0;


        }

    }

}

public class VoxelMod
{

    public Vector3 position;
    public byte id;

    public VoxelMod()
    {

        position = new Vector3();
        id = 0;

    }

    public VoxelMod(Vector3 _position, byte _id)
    {

        position = _position;
        id = _id;

    }

}


[System.Serializable]
public class Settings
{

    [Header("Game Data")]
    public string version = "0.0.0.01";

    [Header("Performance")]
    public int loadDistance = 3;
    public int viewDistance = 1;
    public bool dynamicLighting = false;
    public CloudStyle clouds = CloudStyle.Fast;
    public bool enableAnimatedChunks = false;


    [Header("Controls")]
    [Range(0.1f, 10f)]
    public float mouseSensitivity = 2.0f;

}
