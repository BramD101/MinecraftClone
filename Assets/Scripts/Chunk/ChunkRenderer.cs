public class ChunkRenderer
{
    public ChunkMeshData CreateChunkMeshData(ChunkCoord coord, ChunkVoxelMap voxelMap, ChunksNeighbours neighbourVoxelMaps)
    {
        ChunkRenderInstance instance = new(coord, voxelMap,neighbourVoxelMaps);

        return instance.CreateChunkMeshData();
    }
}

