using System.Numerics;
using UnityEngine;

public struct GlobalVoxelPos
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public GlobalVoxelPos(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static GlobalVoxelPos FromRelativeToChunkPosition(RelativeVoxelPos relPosition, ChunkCoord chunkCoord)
    {
        GlobalVoxelPos globPos = new(
            relPosition.X + VoxelData.ChunkWidth * chunkCoord.X,
            relPosition.Y,
            relPosition.Z + VoxelData.ChunkWidth * chunkCoord.Z);

        return globPos;
    }
   

    public static GlobalVoxelPos FromPointInWorld(UnityEngine.Vector3 vector)
    {
        GlobalVoxelPos globPos = new(
            (int)Mathf.Floor(vector.x),
            (int)Mathf.Floor(vector.y),
            (int)Mathf.Floor(vector.z));

        return globPos;
    }

}


