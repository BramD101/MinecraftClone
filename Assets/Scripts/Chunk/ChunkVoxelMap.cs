using System.Collections.Generic;

public class ChunkVoxelMap
{
    public static GameSettings GameSettings { get; set; }

    private VoxelMinimal[,,] _map = null;
    private readonly Queue<VoxelMod> _voxelModBacklog = new();
    public bool IsFilledIn => _map != null;

    public void EnqueueVoxelMods(Queue<VoxelMod> voxelMods)
    {
        while (voxelMods.TryDequeue(out VoxelMod result))
        {
            _voxelModBacklog.Enqueue(result);
        }
    }
    public Voxel GetVoxel(RelativeToChunkVoxelPosition<int> pos)
    {
        if (!IsInVoxelMap(pos))
        {

            throw new System.Exception($"Not in voxelmap: {pos.X},{pos.Y},{pos.Z}");
        }

        VoxelMinimal voxel = _map[pos.X, pos.Y, pos.Z];     

        return new Voxel(voxel, GameSettings.Blocktypes[(int)voxel.Type]);
    }
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
            RelativeToChunkVoxelPosition<int> relPos = RelativeToChunkVoxelPosition<int>.CreateFromGlobal(mod.GlobalVoxelPosition);
            _map[relPos.X, relPos.Y, relPos.Z].Type = mod.NewVoxelType;
        }
        return true;
    }

    public static bool IsInVoxelMap(RelativeToChunkVoxelPosition<int> pos)
    {
        return pos.X >= 0 && pos.X < VoxelData.ChunkWidth
            && pos.Y >= 0 && pos.Y < VoxelData.ChunkHeight
            && pos.Z >= 0 && pos.Z < VoxelData.ChunkWidth;
    }
}

public struct VoxelMod
{
    public GlobalVoxelPosition<int> GlobalVoxelPosition { get; set; }
    public VoxelType NewVoxelType { get; set; }
}

