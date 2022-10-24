using System;
using System.Collections.Generic;

public class ChunkRepository
{
    private SortedDictionary<ChunkCoord, Chunk> _repo = new();


    public bool TryGetValue(ChunkCoord c, out Chunk outChunk)
    {
        return _repo.TryGetValue(c, out outChunk);

    }
    public bool TryAdd(ChunkCoord coord, Chunk chunk)
    {
        return _repo.TryAdd(coord, chunk);

    }
}