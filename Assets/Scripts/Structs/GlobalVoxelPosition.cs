public struct GlobalVoxelPosition<T>
{
    public T X { get; set; }
    public T Y { get; set; }
    public T Z { get; set; }
    public GlobalVoxelPosition(T x, T y, T z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static GlobalVoxelPosition<int> CreateFromRelativeToChunkPosition(RelativeToChunkVoxelPosition<int> relPosition, ChunkCoord chunkCoord)
    {
        GlobalVoxelPosition<int> globPos = new(
            relPosition.X + VoxelData.ChunkWidth * chunkCoord.X,
            relPosition.Y,
            relPosition.Z + VoxelData.ChunkWidth * chunkCoord.Z);

        return globPos;
    }
    public static GlobalVoxelPosition<float> CreateFromRelativeToChunkPosition(RelativeToChunkVoxelPosition<float> relPosition, ChunkCoord chunkCoord)
    {
        GlobalVoxelPosition<float> globPos = new(
            relPosition.X + VoxelData.ChunkWidth * chunkCoord.X,
            relPosition.Y,
            relPosition.Z + VoxelData.ChunkWidth * chunkCoord.Z);

        return globPos;
    }
}


