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


    public static RelativeToChunkVoxelPosition<int> CreateFromGlobal(GlobalVoxelPosition<int> globalPosition)
    {
        RelativeToChunkVoxelPosition<int> relPos = new(
            globalPosition.X % VoxelData.ChunkWidth,
            globalPosition.Y,
            globalPosition.Z % VoxelData.ChunkWidth);

        return relPos;
    }
    public static RelativeToChunkVoxelPosition<float> CreateFromGlobal(GlobalVoxelPosition<float> globalPosition)
    {
        RelativeToChunkVoxelPosition<float> relPos = new(
            globalPosition.X % VoxelData.ChunkWidth,
            globalPosition.Y,
            globalPosition.Z % VoxelData.ChunkWidth);

        return relPos;
    }
}

