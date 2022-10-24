using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class ChunkVoxelMap
{
    public static GameSettings GameSettings { get; set; }

    private VoxelMinimal[,,] _map = null;
    private readonly ConcurrentQueue<VoxelMod> _voxelModBacklog = new();
    
    public bool IsFilledIn => _map != null;

    public void EnqueueVoxelMods(Queue<VoxelMod> voxelMods)
    {
        while (voxelMods.TryDequeue(out VoxelMod result))
        {
            _voxelModBacklog.Enqueue(result);
        }
    }

    private VoxelTypeData GetVoxelTypeData(RelativeVoxelPos pos)
    {
        return GameSettings.Blocktypes[(int)_map[pos.X, pos.Y, pos.Z].Type];
    }

    //public Voxel GetVoxel(RelativeVoxelPos pos)
    //{
    //    if (!IsInVoxelMap(pos))
    //    {
    //        throw new System.Exception($"Not in voxelmap: {pos.X},{pos.Y},{pos.Z}");
    //    }

    //    VoxelMinimal voxel = _map[pos.X, pos.Y, pos.Z];

    //    return new Voxel(voxel, GameSettings.Blocktypes[(int)voxel.Type]);
    //}



    //public bool TryGetVoxel(RelativeVoxelPos relPos, out Voxel voxel)
    //{
    //    if (!IsInVoxelMap(relPos))
    //    {
    //        voxel = null;
    //        return false;
    //    }

    //    voxel = GetVoxel(relPos);
    //    return true;
    //}

    public bool TrySetMap(VoxelMinimal[,,] map)
    {
        if (IsFilledIn) { return false; }

        _map = map;
        return true;
    }
    public bool TryUpdateBackLog()
    {
        if (!IsFilledIn)
        {
            return false;
        }

        while (_voxelModBacklog.TryDequeue(out VoxelMod mod))
        {
            RelativeVoxelPos relPos = mod.RelativeVoxelPos;
            _map[relPos.X, relPos.Y, relPos.Z].Type = mod.NewVoxelType;
        }
        return true;
    }

    public static bool IsInVoxelMap(RelativeVoxelPos pos)
    {
        return pos.X >= 0 && pos.X < VoxelData.ChunkWidth
            && pos.Y >= 0 && pos.Y < VoxelData.ChunkHeight
            && pos.Z >= 0 && pos.Z < VoxelData.ChunkWidth;
    }

    public void QueueVoxel(GlobalVoxelPos globPos, VoxelType type)
    {
        _voxelModBacklog.Enqueue(new VoxelMod(globPos,type) );
    }

    public bool IsVoxelSolid(RelativeVoxelPos relPos)
    {
        return GetVoxelTypeData(relPos).IsSolid;
    }

    public bool IsVoxelWater(RelativeVoxelPos relPos)
    {
        return GetVoxelTypeData(relPos).IsWater;
    }

    public VoxelMeshData GetMeshData(RelativeVoxelPos relPos)
    {
        return GetVoxelTypeData(relPos).MeshData;
    }

    public bool RenderNeighborFaces(RelativeVoxelPos relPos)
    {
        return GetVoxelTypeData(relPos).RenderNeighborFaces;
    }

    public int GetTextureID(RelativeVoxelPos relPos, VoxelDirection p)
    {
        return GetVoxelTypeData(relPos).GetTextureID(p);
    }
}

public struct VoxelMod
{
    public ChunkCoord ChunkCoord { get; private set; }
    public RelativeVoxelPos RelativeVoxelPos { get; private set; }

    public GlobalVoxelPos GlobalVoxelPos
    {
        get
        {
            return GlobalVoxelPos.FromRelativePosition(RelativeVoxelPos, ChunkCoord);
        }
    }
    public VoxelType NewVoxelType { get; set; }


    public VoxelMod(GlobalVoxelPos globVoxelPos, VoxelType newVoxelType)
    {
        ChunkCoord = ChunkCoord.FromGlobalVoxelPosition(globVoxelPos);
        RelativeVoxelPos = RelativeVoxelPos.FromGlobal(globVoxelPos);
        NewVoxelType = newVoxelType;
    }
    public VoxelMod(ChunkCoord chunkCoord, RelativeVoxelPos relativeVoxelPos, VoxelType newVoxelType)
    {
        ChunkCoord = chunkCoord;
        RelativeVoxelPos = relativeVoxelPos;
        NewVoxelType = newVoxelType;
    }
}

