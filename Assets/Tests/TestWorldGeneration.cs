using NUnit.Framework;
using UnityEngine;

public class TestWorldGeneration
{


    // A Test behaves as an ordinary method
    [Test]
    public void TestWorldGenerationSimplePasses()
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();

      

        WorldGenerationSettings settings = Resources.Load<WorldGenerationSettings>("Data/WorldGeneration/DefaultWorldGenerationSettings");

        WorldGenerator generator = new(settings);
        VoxelData.seed = 0;

        watch.Restart();
        generator.GenerateChunkVoxelMap(new ChunkCoord(0, 0));

        watch.Stop();
        Debug.Log($"Time to create chunk: {watch.ElapsedMilliseconds}ms");

    }
    [Test]
    public void CopySingleVoxelMap()
    {
        System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();


        VoxelMinimal[,,] referenceMap = new VoxelMinimal[16, 16, 128];
        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                for (int k = 0; k < 128; k++)
                {
                    referenceMap[i, j, k] = new VoxelMinimal() { Type = VoxelType.Dirt };
                }
            }
        }

        watch.Restart();
        var newMap = new VoxelMinimal[16, 16, 128];
        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                for (int k = 0; k < 128; k++)
                {
                    newMap[i, j, k] = referenceMap[i, j, k];
                }
            }
        }

        watch.Stop();
        Debug.Log($"Time to copy voxelMap: {watch.Elapsed.TotalMilliseconds}ms");

        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                for (int k = 0; k < 128; k++)
                {
                    var temp = newMap[i, j, k].Type;
                }
            }
        }
    }
}
