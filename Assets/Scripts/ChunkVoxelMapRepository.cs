using System.Collections.Generic;
using TMPro;

public class ChunkVoxelMapRepository
{
    private WorldGenerator _worldGenerator;
    public ChunkVoxelMapRepository(WorldGenerator worldGenerator)
    {
        _worldGenerator = worldGenerator;
    }

    public (Voxel[,,], Dictionary<ChunkCoord, Queue<VoxelMod>>) CreateOrRetrieveChunkVoxelMap(ChunkCoord Coord)
    {
        return _worldGenerator.GenerateChunkVoxelMap(Coord);
    }
}