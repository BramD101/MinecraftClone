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

    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

    public ConcurrentUniqueQueue<ChunkCoord> ActiveChunks = new ConcurrentUniqueQueue<ChunkCoord>();
    public ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;

    private BackgroundThread backgroundThread;

    public ConcurrentQueue<Chunk> chunksToDraw = new ConcurrentQueue<Chunk>();
    public bool WorldIsReady { get; set; }

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
        backgroundThread = new BackgroundThread();

    }

    private void Start()
    {

        Debug.Log("Generating new world using seed " + VoxelData.seed);

        worldData = SaveSystem.LoadWorld("Testing");
     

        //string jsonExport = JsonUtility.ToJson(settings);
        //Debug.Log(jsonExport);

        //File.WriteAllText(Application.dataPath + "/settings.cfg", jsonExport);

        string jsonImport = File.ReadAllText(Application.dataPath + "/settings.cfg");
        settings = JsonUtility.FromJson<Settings>(jsonImport);


        UnityEngine.Random.InitState(VoxelData.seed);

        Shader.SetGlobalFloat("minGlobalLightLevel", VoxelData.minLightLevel);
        Shader.SetGlobalFloat("maxGlobalLightLevel", VoxelData.maxLightLevel);

        LoadWorld();

        backgroundThread.Init(worldData, ActiveChunks);
        backgroundThread.Start();

        SetGlobalLightValue();
        spawnPosition = new Vector3(VoxelData.WorldCentre, VoxelData.ChunkHeight - 50f, VoxelData.WorldCentre);
        player.position = spawnPosition;
        CheckViewDistance();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);

        

        StartCoroutine(Tick());

        WorldIsReady = true;
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
            ActiveChunks.DoActionOnAllMembers(l => chunks[l.x, l.z].TickUpdate());

            yield return new WaitForSeconds(VoxelData.tickLength);

        }

    }


    private void Update()
    {

        playerChunkCoord = GetChunkCoordFromVector3(player.position);

        // Only update the chunks if the player has moved from the chunk they were previously on.
        if (!playerChunkCoord.Equals(playerLastChunkCoord))
            CheckViewDistance();

        if (chunksToDraw.Count > 0)
        {
            Chunk result;
            if(chunksToDraw.TryDequeue(out result))
            {
                result.CreateMesh();
            }    
        }


        if (Input.GetKeyDown(KeyCode.F3))
            debugScreen.SetActive(!debugScreen.activeSelf);

        if (Input.GetKeyDown(KeyCode.F1))
            SaveSystem.SaveWorld(worldData);

       
            

    }

    void LoadWorld()
    {

        for (int x = (VoxelData.WorldSizeInChunks / 2) - settings.loadDistance; x < (VoxelData.WorldSizeInChunks / 2) + settings.loadDistance; x++)
        {
            for (int z = (VoxelData.WorldSizeInChunks / 2) - settings.loadDistance; z < (VoxelData.WorldSizeInChunks / 2) + settings.loadDistance; z++)
            {

                worldData.LoadChunk(new Vector2Int(x, z));

            }
        }

    }









    private void OnDisable()
    {
        backgroundThread?.Abort();


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
        return chunks[x, z];

    }

    void CheckViewDistance()
    {

        clouds.UpdateClouds();

        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        playerLastChunkCoord = playerChunkCoord;

        ConcurrentUniqueQueue<ChunkCoord> previouslyActiveChunks = ActiveChunks.DeepCopy();

        ActiveChunks.Clear();

        // Loop through all chunks currently within view distance of the player.
        for (int x = coord.x - settings.viewDistance; x < coord.x + settings.viewDistance; x++)
        {
            for (int z = coord.z - settings.viewDistance; z < coord.z + settings.viewDistance; z++)
            {

                ChunkCoord thisChunkCoord = new ChunkCoord(x, z);

                // If the current chunk is in the world...
                if (IsChunkInWorld(thisChunkCoord))
                {

                    // Check if it active, if not, activate it.
                    if (chunks[x, z] == null)
                        chunks[x, z] = new Chunk(thisChunkCoord);

                    chunks[x, z].isActive = true;
                    ActiveChunks.TryEnqueue(thisChunkCoord);
                }

                // Check through previously active chunks to see if this chunk is there. If it is, remove it from the list.
                for (int i = 0; i < previouslyActiveChunks.Count; i++)
                {
                    previouslyActiveChunks.TryRemove(thisChunkCoord);

                }

            }
        }

        // Any chunks left in the previousActiveChunks list are no longer in the player's view distance, so loop through and disable them.
        previouslyActiveChunks.DoActionOnAllMembers(l => chunks[l.x, l.z].isActive = false);
    }

    public bool CheckForVoxel(Vector3 pos)
    {

        var voxel = worldData.GetVoxel(pos);

        if(voxel == null)
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
        backgroundThread.AddChunkToUpdate(chunk);
    }
    public void AddChunkToUpdate(Chunk chunk, bool insert)
    {
        backgroundThread.AddChunkToUpdate(chunk, insert);
    }

    public void GenerateStructure(Queue<VoxelMod> structure)
    {
        backgroundThread.EnQueueModification(structure);
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
