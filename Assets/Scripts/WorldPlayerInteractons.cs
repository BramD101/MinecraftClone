using System;

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
        if (!VoxelData.IsVoxelInWorld(pos))
        {
            return false;
        }

        RelativeVoxelPos relPos = RelativeVoxelPos.FromGlobal(pos);
             
        return GetChunk(pos).IsVoxelSolid(relPos);
    }

    public void SetBlock(GlobalVoxelPos pos, VoxelType dirt)
    {
        var chunk = GetChunk(pos);
        chunk.SetVoxel(pos, dirt);
        _world.StartWorldUpdateWithoutUpdatingRange();
    }

    private Chunk GetChunk(GlobalVoxelPos pos)
    {
        ChunkCoord coord = ChunkCoord.FromGlobalVoxelPosition(pos);

        if (_bufferedChunk != null && _bufferedChunk.ChunkCoord.Equals(coord))
        {
            return _bufferedChunk;
        }

        _world.ChunksInLoadDistance.TryGetChunk(coord, out Chunk chunk);

        _bufferedChunk = chunk;
        return chunk;
    }
}