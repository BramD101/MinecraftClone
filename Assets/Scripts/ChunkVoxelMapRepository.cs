public class ChunkVoxelMapRepository : IChunkRepository
{
    private readonly WorldGenerator _worldGenerator;
    private readonly ChunkRepository _chunksOutsideLoadDistance = new();
    public ChunkVoxelMapRepository(WorldGenerator worldGenerator)
    {
        _worldGenerator = worldGenerator;
    }

    public WorldGenerationData CreateOrRetrieveChunkVoxelMap(ChunkCoord Coord)
    {
        return _worldGenerator.GenerateChunkVoxelMap(Coord);
    }
    public bool TryAdd(ChunkCoord coord, Chunk chunk)
    {
        return _chunksOutsideLoadDistance.TryAdd(coord, chunk);
    }

    // only retrieve chunk if in buffer
    public bool TryGetChunk(ChunkCoord currentChunkCoord, out Chunk chunk)
    {
        return _chunksOutsideLoadDistance.TryGetValue(currentChunkCoord, out chunk);
    }
}
