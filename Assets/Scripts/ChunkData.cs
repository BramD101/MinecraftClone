using Assembly_CSharp;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class ChunkData
{
    public ChunkCoord Position { get; set; }
    [System.NonSerialized] public Chunk chunk;
    public VoxelState[,,] map = new VoxelState[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
    private bool _isActive;
    public bool IsActive
    {

        get { return _isActive; }
        set
        {
            if (_isActive != value)
            {
                _isActive = value;
                if (_isActive && chunk == null)
                {
                    chunk = new Chunk(this);
                }
                chunk.SetActive(_isActive);
            }
        }
    }

    public ChunkData(ChunkCoord pos, bool populate = true)
    {
        Position = pos;
        if (populate)
        {
            Populate();
        }

    }





    public void Populate()
    {

        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {

                    Vector3 voxelGlobalPos = new Vector3(x + Position.x * VoxelData.ChunkWidth, y, z + Position.z * VoxelData.ChunkWidth);

                    var voxelGenerationData = WorldGenerator.Instance.GenerateVoxel(voxelGlobalPos);
                    if (voxelGenerationData.Structure?.Count > 0)
                    {
                        World.Instance.GenerateStructure(voxelGenerationData.Structure);
                    }
                    map[x, y, z] = new VoxelState(voxelGenerationData.VoxelType, this, new Vector3Int(x, y, z));

                    // Loop through each of the voxels neighbours and attempt to set them.
                    for (int p = 0; p < 6; p++)
                    {

                        Vector3Int neighbourV3 = new Vector3Int(x, y, z) + VoxelData.faceChecks[p];
                        if (IsVoxelInChunk(neighbourV3)) // If in chunk, get voxel straight from map.
                            map[x, y, z].neighbours[p] = VoxelFromV3Int(neighbourV3);
                        else // Else see if we can get the neighbour from WorldData.
                            map[x, y, z].neighbours[p] = World.Instance.worldData.GetVoxel(voxelGlobalPos + VoxelData.faceChecks[p]);

                    }

                }
            }
        }

        //Lighting.RecalculateNaturaLight(this);
        World.Instance.worldData.AddToModifiedChunkList(this);

    }

    public void ModifyVoxel(Vector3Int pos, byte _id, int direction)
    {

        // If we've somehow tried to change a block for the same block, just return.
        if (map[pos.x, pos.y, pos.z].id == _id)
            return;

        // Cache voxels for easier code.
        VoxelState voxel = map[pos.x, pos.y, pos.z];
        BlockType newVoxel = World.Instance.blocktypes[_id];

        // Cache the old opacity value.
        byte oldOpacity = voxel.properties.opacity;

        // Set voxel to new ID.
        voxel.id = _id;
        voxel.orientation = direction;

        // If the opacity values of the voxel have changed and the voxel above is in direct sunlight
        // (or is above the world) recast light from that voxel downwards.

        if (World.Instance.settings.dynamicLighting && voxel.properties.opacity != oldOpacity &&
            (pos.y == VoxelData.ChunkHeight - 1 || map[pos.x, pos.y + 1, pos.z].light == 15))
        {

            Lighting.CastNaturalLight(this, pos.x, pos.z, pos.y + 1);

        }

        if (voxel.properties.isActive && BlockBehaviour.Active(voxel))
            voxel.chunkData.chunk.AddActiveVoxel(voxel);
        for (int i = 0; i < 6; i++)
        {
            if (voxel.neighbours[i] != null)
                if (voxel.neighbours[i].properties.isActive && BlockBehaviour.Active(voxel.neighbours[i]))
                    voxel.neighbours[i].chunkData.chunk.AddActiveVoxel(voxel.neighbours[i]);
        }

        // Add this ChunkData to the modified chunks list.
        World.Instance.worldData.AddToModifiedChunkList(this);

        // If we have a chunk attached, add that for updating.
        if (chunk != null)
            World.Instance.AddChunkToUpdate(chunk);

    }

    public bool IsVoxelInChunk(int x, int y, int z)
    {

        if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1)
            return false;
        else
            return true;

    }

    public bool IsVoxelInChunk(Vector3Int pos)
    {

        return IsVoxelInChunk(pos.x, pos.y, pos.z);

    }

    public VoxelState VoxelFromV3Int(Vector3Int pos)
    {

        return map[pos.x, pos.y, pos.z];

    }




    public void Serialize(Stream stream)
    {
        using (var bw = new BinaryWriter(stream))
        {
            bw.Write(Position.x);
            bw.Write(Position.z);
            bw.Write(map.GetLength(0));
            bw.Write(map.GetLength(1));
            bw.Write(map.GetLength(2));
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    for (int k = 0; k < map.GetLength(2); k++)
                    {
                        bw.Write(map[i, j, k].id);
                        bw.Write(map[i, j, k].orientation);
                    }
                }
            }
        }
    }
    public static ChunkData Deserialize(Stream stream)
    {


        using (var sr = new BinaryReader(stream))
        {


            int x = sr.ReadInt32();
            int z = sr.ReadInt32();
            var mapLength0 = sr.ReadInt32();
            var mapLength1 = sr.ReadInt32();
            var mapLength2 = sr.ReadInt32();
            ChunkData chunkData = new ChunkData(new ChunkCoord(x, z), false);



            for (int i = 0; i < mapLength0; i++)
            {
                for (int j = 0; j < mapLength1; j++)
                {
                    for (int k = 0; k < mapLength2; k++)
                    {
                        var id = sr.ReadByte();
                        var orientation = sr.ReadInt32();

                        chunkData.map[i, j, k] = new VoxelState(id, chunkData, new Vector3Int(i, j, k), orientation);

                    }
                }
            }
            return chunkData;
        }


    }



}
