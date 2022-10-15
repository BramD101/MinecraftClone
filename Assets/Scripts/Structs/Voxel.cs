public struct Voxel
{
	private byte _type;

	public VoxelType Type { get => (VoxelType)_type; set => _type = (byte)value; }

	
}

public enum VoxelType : byte
{
	Air = 0,
	Bedrock = 1,
	Stone = 2,
	Grass = 3,
	Sand = 4,
	Dirt = 5,
	Wood =6,
	Planks = 7,
	Furnace = 8,
	Cobblestone = 9,
	Glass = 10,
	Leaves = 11,
	Cactus = 12,
	CactusTop = 32,
	Water = 14
}

