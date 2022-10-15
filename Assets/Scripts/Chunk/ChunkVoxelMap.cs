using System.Collections.Generic;

public class ChunkVoxelMap
{
	private Voxel[,,] _map = null;
	private readonly Queue<VoxelMod> _voxelModBacklog = new();
	public bool IsFilledIn => _map != null;

	public void EnqueueVoxelMods(Queue<VoxelMod> voxelMods)
	{
		while (voxelMods.TryDequeue(out VoxelMod result))
		{
			_voxelModBacklog.Enqueue(result);
		}
	}

	public bool TrySetMap(Voxel[,,] map)
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
}

public struct VoxelMod
{
	public GlobalVoxelPosition<int> GlobalVoxelPosition { get; set; }
	public VoxelType NewVoxelType { get; set; }
}

