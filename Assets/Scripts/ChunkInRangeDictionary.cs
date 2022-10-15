using System;
using System.Collections.Generic;
using System.Linq;

public class ChunkInRangeDictionary
{
    private readonly Dictionary<ChunkCoord, Chunk> _chunksInDistance = new();


    private readonly Action<Chunk> _disposeMethod;
    private readonly int _distance;

    public List<Chunk>  Chunks => _chunksInDistance.Values.ToList<Chunk>();
    public List<ChunkCoord> ChunksCoords => _chunksInDistance.Keys.ToList<ChunkCoord>();
    public ChunkInRangeDictionary(Action<Chunk> disposeMethod, int distance)
    {
        _disposeMethod = disposeMethod;
        _distance = distance;
    }

 
    public void UpdateChunksInDistance(ChunkCoord coord, Func<ChunkCoord, ValueTuple<Chunk, bool>> TryGetChunkFromRepo)
    {


        Dictionary<ChunkCoord, Chunk> oldChunks = new(_chunksInDistance);
        _chunksInDistance.Clear();

        for (int i = coord.X - _distance; i <= coord.X + _distance; i++)
        {
            for (int j = coord.Z - _distance; j <= coord.Z + _distance; j++)
            {
                ChunkCoord currentChunkCoord = new(i, j);

                if (oldChunks.TryGetValue(currentChunkCoord, out Chunk newChunk))
                {
                    oldChunks.Remove(currentChunkCoord);
                }
                else
                {
                    bool success;
                    (newChunk, success) = TryGetChunkFromRepo(currentChunkCoord);

                    if (!success)
                    {
                        newChunk = new Chunk(currentChunkCoord);
                    }
                }
                _chunksInDistance.Add(currentChunkCoord, newChunk);
            }

        }


        foreach (Chunk chunk in oldChunks.Values)
        {
            _disposeMethod(chunk);
        }

    }

    public bool TryGetChunk(ChunkCoord chunkCoord, out Chunk outChunk)
    {
        if (_chunksInDistance.ContainsKey(chunkCoord))
        {
            outChunk = _chunksInDistance[chunkCoord];
            return true;
        }
        outChunk = null;
        return false;
    }

   
}

