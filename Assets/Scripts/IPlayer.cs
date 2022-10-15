using System;

public interface IPlayer
{
    public event EventHandler<ChunkCoord> PlayerMovedChunks;
}

