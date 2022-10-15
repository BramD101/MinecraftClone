using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CloudsSettings", menuName = "MinecraftAsset/Clouds Settings")]
[Serializable]
public class CloudsSettings : ScriptableObject
{

    [SerializeField]
    private int _cloudHeight;
    [SerializeField]
    private int _cloudDepth;

    [SerializeField]
    private Texture2D _cloudPattern;
    [SerializeField]
    private Material _cloudMaterial;


}


