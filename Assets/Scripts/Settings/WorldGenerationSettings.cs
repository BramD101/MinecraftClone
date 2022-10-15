using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "WorldGenerationSettings", menuName = "MinecraftAsset/WorldGenerationSettings")]
public class WorldGenerationSettings : ScriptableObject
{
    [SerializeField]
    private BiomeAttributes[] _biomes;
    public BiomeAttributes[] Biomes  => _biomes; 
   
}

