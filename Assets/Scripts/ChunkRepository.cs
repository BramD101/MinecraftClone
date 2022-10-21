using System;
using System.Collections.Generic;

public class ChunkConcurrentRepository
{
    private SortedDictionary<ChunkCoord, Chunk> _repo = new();
    private object _locker = new object();

    public bool TryGetValue(ChunkCoord c, out Chunk outChunk)
    {
        lock (_locker)
        {
            return _repo.TryGetValue(c, out outChunk);
        }
    }
    public bool TryAdd(ChunkCoord coord, Chunk chunk)
    {
        lock(_locker)
        {
            return _repo.TryAdd(coord, chunk);
        }
    }
}