public class WorldPlayerInteractons
{
    private readonly World _world;

    private Chunk _bufferedChunk;

    public WorldPlayerInteractons(World world)
    {
        _world = world;
    }
    public bool ContainsSolidVoxel(GlobalVoxelPos pos)
    {
        ChunkCoord coord = ChunkCoord.FromGlobalVoxelPosition(pos);
        var relPos = RelativeVoxelPos.CreateFromGlobal(pos);

        if (_bufferedChunk != null && _bufferedChunk.ChunkCoord.Equals(coord))
        {
            return _bufferedChunk.IsVoxelSolid(relPos);
        }

        _world.ChunksInLoadDistance.TryGetChunk(coord, out Chunk chunk);

        _bufferedChunk = chunk;
        return chunk.IsVoxelSolid(relPos);
    }
}