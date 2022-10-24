
public interface IChunkRepository
{

    public bool TryGetChunk(ChunkCoord currentChunkCoord, out Chunk chunk);

    
}