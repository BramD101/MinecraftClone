using System.Collections.Generic;
using TMPro;

public class ChunkVoxelMapRepository
{
    private WorldGenerator _worldGenerator;
    public ChunkVoxelMapRepository(WorldGenerator worldGenerator)
    {
        _worldGenerator = worldGenerator;
    }

    public WorldGenerationData CreateOrRetrieveChunkVoxelMap(ChunkCoord Coord)
    {
        return _worldGenerator.GenerateChunkVoxelMap(Coord);
    }
}