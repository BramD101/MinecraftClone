using UnityEngine;
using UnityEngine.UIElements;

public class RelativeToChunkVoxelPosition<T>
{
    public T X { get; set; }
    public T Y { get; set; }
    public T Z { get; set; }
    public RelativeToChunkVoxelPosition(T x, T y, T z)
    {
        X = x;
        Y = y;
        Z = z;
    }  
    public static RelativeToChunkVoxelPosition<int> FromVector3Int(Vector3Int vector)
    {
        return new RelativeToChunkVoxelPosition<int>(vector.x, vector.y, vector.z);
    }
    public static RelativeToChunkVoxelPosition<int> CreateFromGlobal(GlobalVoxelPosition<int> globalPosition)
    {
        RelativeToChunkVoxelPosition<int> relPos = new(
            ((globalPosition.X % VoxelData.ChunkWidth) + VoxelData.ChunkWidth) % VoxelData.ChunkWidth,
            globalPosition.Y,
            ((globalPosition.Z % VoxelData.ChunkWidth) + VoxelData.ChunkWidth) % VoxelData.ChunkWidth);

        return relPos;
    }
    public static RelativeToChunkVoxelPosition<float> CreateFromGlobal(GlobalVoxelPosition<float> globalPosition)
    {
        RelativeToChunkVoxelPosition<float> relPos = new(
            ((globalPosition.X % VoxelData.ChunkWidth) + VoxelData.ChunkWidth) % VoxelData.ChunkWidth,
            globalPosition.Y,
            ((globalPosition.Z % VoxelData.ChunkWidth) + VoxelData.ChunkWidth) % VoxelData.ChunkWidth);

        return relPos;
    }

}

