using System.Collections.Generic;
using System.Linq;

public class ChunksNeighbours
{
    private readonly Dictionary<ChunkNeighbourType, Chunk> _dictionaryByDirection = new();
    private readonly Dictionary<ChunkCoord, Chunk> _dictionaryByCoord = new();

    public static List<ChunkNeighbourType> GetChunkNeighbourTypes()
    {
        List<ChunkNeighbourType> neighbourTypes = new();
        for (int i = 0; i < 8; i++)
        {
            neighbourTypes.Add((ChunkNeighbourType)i);
        }
        return neighbourTypes;
    }

    public static ChunkCoord GetNeighbourCoord(ChunkCoord coord, ChunkNeighbourType direction)
    {
        switch (direction)
        {
            case ChunkNeighbourType.LowerZLowerX:
                return new ChunkCoord(coord.X - 1, coord.Z - 1);

            case ChunkNeighbourType.LowerZSameX:
                return new ChunkCoord(coord.X, coord.Z - 1);

            case ChunkNeighbourType.LowerZHigherX:
                return new ChunkCoord(coord.X + 1, coord.Z - 1);

            case ChunkNeighbourType.SameZLowerX:
                return new ChunkCoord(coord.X - 1, coord.Z);

            case ChunkNeighbourType.SameZHigherX:
                return new ChunkCoord(coord.X + 1, coord.Z);

            case ChunkNeighbourType.HigherZLowerX:
                return new ChunkCoord(coord.X - 1, coord.Z + 1);

            case ChunkNeighbourType.HigherZSameX:
                return new ChunkCoord(coord.X, coord.Z + 1);

            case ChunkNeighbourType.HigherZHigherX:
                return new ChunkCoord(coord.X + 1, coord.Z + 1);

            default:
                throw new System.Exception();
        }
    }


    public Chunk GetChunkByCoord(ChunkCoord coord)
    {
        if(!_dictionaryByCoord.TryGetValue(coord,out Chunk chunk))
        {
            throw new System.Exception();
        };
        return chunk;
    }

    public void Add(ChunkNeighbourType direction, Chunk chunk)
    {
        _dictionaryByDirection.Add(direction, chunk);
        _dictionaryByCoord.Add(chunk.ChunkCoord, chunk);
    }

}
public enum ChunkNeighbourType
{
    LowerZLowerX = 0,
    LowerZSameX = 1,
    LowerZHigherX = 2,
    SameZLowerX = 3,
    SameZHigherX = 4,
    HigherZLowerX = 5,
    HigherZSameX = 6,
    HigherZHigherX = 7,
}
