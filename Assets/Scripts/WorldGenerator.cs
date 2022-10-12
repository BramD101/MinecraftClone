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
    }

}
