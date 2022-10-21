﻿using System;

public struct ChunkCoord : IComparable<ChunkCoord>
{
    public int X { get; set; }
    public int Z { get; set; }
    public ChunkCoord(int x, int z)
    {
        X = x;
        Z = z;
    } 
 
  
    public static ChunkCoord FromGlobalVoxelPosition(GlobalVoxelPos position)
    {
        return new ChunkCoord(
            (int)MathF.Floor((float)position.X / VoxelData.ChunkWidth),
            (int)MathF.Floor((float)position.Z / VoxelData.ChunkWidth));
    }
    public override string ToString()
    {
        return $"(X: {X}, Z: {Z})";
    }

    public int CompareTo(ChunkCoord other)
    {
        if(X == other.X)
        {
            return Z.CompareTo(other.Z);
        }
        return X.CompareTo(other.X);
    }
}

