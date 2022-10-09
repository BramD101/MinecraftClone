﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class WorldData : IDeserializationCallback
{

    public string worldName = "Prototype";
    public int seed;

    [System.NonSerialized]
    public Dictionary<Vector2Int, ChunkData> chunks = new ();

    [System.NonSerialized]
    public List<ChunkData> modifiedChunks = new();



    public void OnDeserialization(object sender)
    {
        chunks = new();
        modifiedChunks = new();
    }
    public void AddToModifiedChunkList (ChunkData chunk) {

        if (!modifiedChunks.Contains(chunk))
            modifiedChunks.Add(chunk);

    }

    

    public ChunkData RequestChunk (Vector2Int coord, bool create) {

        ChunkData c;

        lock (World.Instance.ChunkListThreadLock) {

            if (chunks.ContainsKey(coord))
                c = chunks[coord];
            else if (!create)
                c = null;
            else {
                if (!LoadChunk(coord))
                {
                    CreateChunk(coord);
                }
                c = chunks[coord];
            }
        }

        return c;

    }

    public bool LoadChunk (Vector2Int coord) {

        if (chunks.ContainsKey(coord))
            return false;

        ChunkData chunk = SaveSystem.LoadChunk(worldName, coord);
        if (chunk != null) {
            chunks.Add(coord, chunk);
            return true;
        }
        return false;

       
    }
    public void CreateChunk(Vector2Int coord)
    {
        if (chunks.ContainsKey(coord))
            return;

        chunks.Add(coord, new ChunkData(coord));
        chunks[coord].Populate();
    }


    bool IsVoxelInWorld (Vector3 pos) {

        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.ChunkHeight && pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
            return true;
        else
            return false;

    }

    public void SetVoxel (Vector3 pos, byte value, int direction) {

        // If the voxel is outside of the world we don't need to do anything with it.
        if (!IsVoxelInWorld(pos))
            return;

        // Find out the ChunkDCoord value of our voxel's chunk.
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

        // Then reverse that to get the position of the chunk.
        x *= VoxelData.ChunkWidth;
        z *= VoxelData.ChunkWidth;

        // Check if the chunk exists. If not, create it.
        ChunkData chunk = RequestChunk(new Vector2Int(x, z), true);

        // Then create a Vector3Int with the position of our voxel *within* the chunk.
        Vector3Int voxel = new Vector3Int((int)(pos.x - x), (int)pos.y, (int)(pos.z - z));

        // Then set the voxel in our chunk.
        chunk.ModifyVoxel(voxel, value, direction);

    }

    public VoxelState GetVoxel (Vector3 pos) {

        // If the voxel is outside of the world we don't need to do anything with it.
        if (!IsVoxelInWorld(pos))
            return null;

        // Find out the ChunkDCoord value of our voxel's chunk.
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

        // Then reverse that to get the position of the chunk.
        x *= VoxelData.ChunkWidth;
        z *= VoxelData.ChunkWidth;

        // Check if the chunk exists. If not, create it.
        ChunkData chunk = RequestChunk(new Vector2Int(x, z), false);

        if (chunk == null)
            return null;

        // Then create a Vector3Int with the position of our voxel *within* the chunk.
        Vector3Int voxel = new Vector3Int((int)(pos.x - x), (int)pos.y, (int)(pos.z - z));

        // Then set the voxel in our chunk.
        return chunk.map[voxel.x, voxel.y, voxel.z];

    }

}
