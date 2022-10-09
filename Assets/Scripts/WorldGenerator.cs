using Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assembly_CSharp
{
    public class WorldGenerator : MonoBehaviour
    {
        [SerializeField]
        private BiomeAttributes[] _biomes;
        public BiomeAttributes[] Biomes
        {
            get { return _biomes; }
        }

        public static WorldGenerator Instance { get; private set; }
        public void Awake()
        {
            Instance = this;
        }

        public VoxelGenerationData GenerateVoxel(Vector3 pos)
        {
            var voxelGenerationData = new VoxelGenerationData();

            int yPos = Mathf.FloorToInt(pos.y);

            /* IMMUTABLE PASS */

            // If outside world, return air.
            if (!World.Instance.IsVoxelInWorld(pos))
            {
                voxelGenerationData.VoxelType = 0;
                return voxelGenerationData;
            }

            // If bottom block of chunk, return bedrock.
            if (yPos == 0)
            {
                voxelGenerationData.VoxelType = 1;
                return voxelGenerationData;
            }

            /* BIOME SELECTION PASS*/

            int solidGroundHeight = 42;
            float sumOfHeights = 0f;
            int count = 0;
            float strongestWeight = 0f;
            int strongestBiomeIndex = 0;

            for (int i = 0; i < Biomes.Length; i++)
            {

                float weight = Noise.Get2DPerlin(new Vector2(pos.x, pos.z), Biomes[i].offset, Biomes[i].scale);

                // Keep track of which weight is strongest.
                if (weight > strongestWeight)
                {

                    strongestWeight = weight;
                    strongestBiomeIndex = i;

                }

                // Get the height of the terrain (for the current biome) and multiply it by its weight.
                float height = Biomes[i].terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, Biomes[i].terrainScale) * weight;

                // If the height value is greater 0 add it to the sum of heights.
                if (height > 0)
                {

                    sumOfHeights += height;
                    count++;

                }

            }

            // Set biome to the one with the strongest weight.
            BiomeAttributes biome = Biomes[strongestBiomeIndex];

            // Get the average of the heights.
            sumOfHeights /= count;

            int terrainHeight = Mathf.FloorToInt(sumOfHeights + solidGroundHeight);


            //BiomeAttributes biome = biomes[index];

            /* BASIC TERRAIN PASS */

            voxelGenerationData.VoxelType = 0;


            if (yPos == terrainHeight)
                voxelGenerationData.VoxelType = biome.surfaceBlock;
            else if (yPos < terrainHeight && yPos > terrainHeight - 4)
                voxelGenerationData.VoxelType = biome.subSurfaceBlock;
            else if (yPos > terrainHeight)
            {
                if (yPos < 51)
                {
                    voxelGenerationData.VoxelType = 14;
                    return voxelGenerationData;
                }
                else
                {
                    voxelGenerationData.VoxelType = 0;
                    return voxelGenerationData;
                }

            }
            else
                voxelGenerationData.VoxelType = 2;

            /* SECOND PASS */

            if (voxelGenerationData.VoxelType == 2)
            {

                foreach (Lode lode in biome.lodes)
                {

                    if (yPos > lode.minHeight && yPos < lode.maxHeight)
                        if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                            voxelGenerationData.VoxelType = lode.blockID;

                }

            }

            /* TREE PASS */

            if (yPos == terrainHeight && biome.placeMajorFlora)
            {

                if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.majorFloraZoneScale) > biome.majorFloraZoneThreshold)
                {

                    if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.majorFloraPlacementScale) > biome.majorFloraPlacementThreshold)
                    {
                        var structure = Structure.GenerateMajorFlora(biome.majorFloraIndex, pos, biome.minHeight, biome.maxHeight);
                        voxelGenerationData.Structure = structure;
                        //backgroundThread.EnQueueModification();
                    }
                }

            }



            return voxelGenerationData;


        }
    }
    public class VoxelGenerationData
    {
        public byte VoxelType { get; set; }
        public Queue<VoxelMod> Structure { get; set; }
        public VoxelGenerationData()
        {
            Structure = new Queue<VoxelMod>();
        }
    }

}
