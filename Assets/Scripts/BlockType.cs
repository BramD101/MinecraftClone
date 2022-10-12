﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


    [System.Serializable]
    public class BlockType
    {

        public string blockName;
        public bool isSolid;
        public VoxelMeshData meshData;
        public bool renderNeighborFaces;
        public bool isWater;
        public byte opacity;
        public Sprite icon;
        public bool isActive;

        [Header("Texture Values")]
        public int backFaceTexture;
        public int frontFaceTexture;
        public int topFaceTexture;
        public int bottomFaceTexture;
        public int leftFaceTexture;
        public int rightFaceTexture;

        // Back, Front, Top, Bottom, Left, Right

        public int GetTextureID(int faceIndex)
        {

            switch (faceIndex)
            {

                case 0:
                    return backFaceTexture;
                case 1:
                    return frontFaceTexture;
                case 2:
                    return topFaceTexture;
                case 3:
                    return bottomFaceTexture;
                case 4:
                    return leftFaceTexture;
                case 5:
                    return rightFaceTexture;
                default:
                    Debug.Log("Error in GetTextureID; invalid face index");
                    return 0;


            }

        }

    }

