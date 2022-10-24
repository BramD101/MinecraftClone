using UnityEngine;
using UnityEngine.UIElements;

public class RelativeVoxelPos
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public RelativeVoxelPos(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }  
    public static RelativeVoxelPos FromVector3Int(Vector3Int vector)
    {
        return new RelativeVoxelPos(vector.x, vector.y, vector.z);
    }
    public static RelativeVoxelPos FromGlobal(GlobalVoxelPos globalPosition)
    {
        RelativeVoxelPos relPos = new(
            ((globalPosition.X % VoxelData.ChunkWidth) + VoxelData.ChunkWidth) % VoxelData.ChunkWidth,
            globalPosition.Y,
            ((globalPosition.Z % VoxelData.ChunkWidth) + VoxelData.ChunkWidth) % VoxelData.ChunkWidth);

        return relPos;
    }
   


}

