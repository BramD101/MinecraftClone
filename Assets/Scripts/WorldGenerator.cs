using System.Collections.Generic;
using UnityEngine;


public class WorldGenerator
{
    private readonly WorldGenerationSettings _settings;

    public WorldGenerator(WorldGenerationSettings settings)
    {
        _settings = settings;
    }

    public WorldGenerationData GenerateChunkVoxelMap(ChunkCoord coord)
    {
        VoxelMinimal[,,] voxelMap = new VoxelMinimal[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

        Dictionary<ChunkCoord, Queue<VoxelMod>> voxelModifications = new();
        for (int x = 0; x < VoxelData.ChunkWidth; x++)
        {
            for (int y = 0; y < VoxelData.ChunkHeight; y++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    RelativeToChunkVoxelPosition<int> relativeVoxelPositing = new(x, y, z);
                    GlobalVoxelPosition<int> globPos = GlobalVoxelPosition<int>.CreateFromRelativeToChunkPosition(relativeVoxelPositing, coord);


                    Queue<VoxelMod> structure;
                    (voxelMap[x, y, z].Type, structure) = GenerateChunkVoxel(globPos);

                    foreach (VoxelMod mod in structure)
                    {
                        ChunkCoord currentChunk = ChunkCoord.FromGlobalVoxelPosition(mod.GlobalVoxelPosition);

                        if (!voxelModifications.ContainsKey(currentChunk))
                        {
                            voxelModifications.Add(currentChunk, new());
                        }
                        voxelModifications[currentChunk].Enqueue(mod);
                    }
                }
            }
        }

        return new WorldGenerationData(coord, voxelMap, voxelModifications );
    }


    private (VoxelType, Queue<VoxelMod>) GenerateChunkVoxel(GlobalVoxelPosition<int> globPos)
    {
        //debug
        //if(globPos.Y >= 1)
        //{
        //    return (VoxelType.Air, new Queue<VoxelMod>());
        //}
        //else
        //{
        //    return (VoxelType.Dirt, new Queue<VoxelMod>());
        //}



        BiomeAttributes[] biomes = _settings.Biomes;
        /* IMMUTABLE PASS */

        // If outside world, return air.
        if (!VoxelData.IsVoxelInWorld(globPos))
        {
            return (VoxelType.Air, new Queue<VoxelMod>());
        }

        // If bottom block of chunk, return bedrock.
        if (globPos.Y == 0)
        {
            return (VoxelType.Bedrock, new Queue<VoxelMod>());
        }

        /* BIOME SELECTION PASS*/

        int solidGroundHeight = 42;
        float sumOfHeights = 0f;
        int count = 0;
        float strongestWeight = 0f;
        int strongestBiomeIndex = 0;
        for (int i = 0; i < biomes.Length; i++)
        {

            float weight = Noise.Get2DPerlin(new Vector2(globPos.X, globPos.Z), biomes[i].Offset, biomes[i].Scale);

            // Keep track of which weight is strongest.
            if (weight > strongestWeight)
            {
                strongestWeight = weight;
                strongestBiomeIndex = i;
            }

            // Get the height of the terrain (for the current biome) and multiply it by its weight.
            float height = biomes[i].TerrainHeight * Noise.Get2DPerlin(new Vector2(globPos.X, globPos.Z), 0, biomes[i].TerrainScale) * weight;

            // If the height value is greater 0 add it to the sum of heights.
            if (height > 0)
            {
                sumOfHeights += height;
                count++;
            }

        }

        // Set biome to the one with the strongest weight.
        BiomeAttributes biome = biomes[strongestBiomeIndex];

        // Get the average of the heights.
        sumOfHeights /= count;

        int terrainHeight = Mathf.FloorToInt(sumOfHeights + solidGroundHeight);


        if (globPos.Y > terrainHeight)
        {
            if (globPos.Y < 51)
            {
                return (VoxelType.Water, new Queue<VoxelMod>());
            }
            else
            {
                return (VoxelType.Air, new Queue<VoxelMod>());
            }
        }

        /* BASIC TERRAIN PASS */

        VoxelType voxelType = VoxelType.Air;

        if (globPos.Y == terrainHeight)
            voxelType = biome.SurfaceBlock;
        else if (globPos.Y < terrainHeight && globPos.Y > terrainHeight - 4)
            voxelType = biome.SubSurfaceBlock;
        else
            voxelType = VoxelType.Dirt;

        /* SECOND PASS */

        if (voxelType == VoxelType.Dirt)
        {

            foreach (Lode lode in biome.Lodes)
            {

                if (globPos.Y > lode.MinHeight && globPos.Y < lode.MaxHeight)
                    if (Noise.Get3DPerlin(new Vector3(globPos.X, globPos.Y, globPos.Z), lode.NoiseOffset, lode.Scale, lode.Threshold))
                        voxelType = lode.BlockID;

            }

        }
        Queue<VoxelMod> structure = new();
        /* TREE PASS */
        if (globPos.Y == terrainHeight && biome.PlaceMajorFlora)
        {
            if (Noise.Get2DPerlin(new Vector2(globPos.X, globPos.Z), 0, biome.MajorFloraZoneScale) > biome.MajorFloraZoneThreshold)
            {
                if (Noise.Get2DPerlin(new Vector2(globPos.X, globPos.Z), 0, biome.MajorFloraPlacementScale) > biome.MajorFloraPlacementThreshold)
                {
                    structure = Structure.GenerateMajorFlora(biome.MajorFloraIndex, globPos, biome.MinHeight, biome.MaxHeight);

                    //backgroundThread.EnQueueModification();
                }
            }
        }

        return (voxelType, structure);
    }
}
public struct WorldGenerationData
{
    public ChunkCoord ChunkCoord { get; set; }
    public VoxelMinimal[,,] Map { get; set; }
    public Dictionary<ChunkCoord, Queue<VoxelMod>> Structures { get; set; }

    public WorldGenerationData(ChunkCoord chunkCoord, VoxelMinimal[,,] map, Dictionary<ChunkCoord, Queue<VoxelMod>> structures)
    {
        ChunkCoord = chunkCoord;
        Map = map;
        Structures = structures;

    }
}
