using System.Collections.Generic;
using UnityEngine;

public static class Structure
{

    public static Queue<VoxelMod> GenerateMajorFlora(int index, GlobalVoxelPos position, int minTrunkHeight, int maxTrunkHeight)
    {

        switch (index)
        {

            case 0:
                return MakeTree(position, minTrunkHeight, maxTrunkHeight);

            case 1:
                return MakeCacti(position, minTrunkHeight, maxTrunkHeight);

        }

        return new Queue<VoxelMod>();

    }

    public static Queue<VoxelMod> MakeTree(GlobalVoxelPos position, int minTrunkHeight, int maxTrunkHeight)
    {

        Queue<VoxelMod> queue = new();

        int height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(position.X, position.Z), 250f, 3f));

        if (height < minTrunkHeight)
            height = minTrunkHeight;

        for (int i = 1; i < height; i++)
            queue.Enqueue(new VoxelMod
            {
                GlobalVoxelPosition = new GlobalVoxelPos(position.X, position.Y + i, position.Z),
                NewVoxelType = VoxelType.Wood
            });

        for (int x = -3; x < 4; x++)
        {
            for (int y = 0; y < 7; y++)
            {
                for (int z = -3; z < 4; z++)
                {
                    queue.Enqueue(new VoxelMod
                    {
                        GlobalVoxelPosition = new GlobalVoxelPos(position.X + x, position.Y + height + y, position.Z + z),
                        NewVoxelType = VoxelType.Leaves
                    });
                }
            }
        }

        return queue;

    }

    public static Queue<VoxelMod> MakeCacti(GlobalVoxelPos position, int minTrunkHeight, int maxTrunkHeight)
    {
        Queue<VoxelMod> queue = new();

        int height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(position.X, position.Z), 23456f, 2f));

        if (height < minTrunkHeight)
            height = minTrunkHeight;

        for (int i = 1; i < height; i++)
        {
            queue.Enqueue(new VoxelMod
            {
                GlobalVoxelPosition = new GlobalVoxelPos(position.X, position.Y + i, position.Z),
                NewVoxelType = VoxelType.Cactus
            });
        }
        queue.Enqueue(new VoxelMod
        {
            GlobalVoxelPosition = new GlobalVoxelPos(position.X, position.Y + height, position.Z),
            NewVoxelType = VoxelType.CactusTop
        });

        return queue;

    }

}
