
using System.Collections.Generic;
using UnityEngine;

public class Clouds
{
    bool[,] cloudData; // Array of bools representing where cloud is.

    int cloudTexWidth;

    int cloudTileSize;
    Vector3Int offset;

    Dictionary<Vector2Int, GameObject> clouds = new Dictionary<Vector2Int, GameObject>();
}

public enum CloudStyle
{

    Off,
    Fast,
    Fancy

}
