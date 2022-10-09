using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    internal class BackgroundThread
    {
        private WorldData _worldData;
        private ConcurrentUniqueQueue<ChunkCoord> _activeChunks;
        private Clouds _clouds;
        private Chunk[,] _chunks;


        private Thread thread;
        internal void Abort()
        {
            thread?.Abort();
        }

        public static readonly AutoResetEvent ResetEvent = new AutoResetEvent(true);
      
        public void Init(WorldData worldData, ConcurrentUniqueQueue<ChunkCoord> activeChunks)
        {
            _worldData = worldData;
            _activeChunks = activeChunks;
        }
        public void Start()
        {
            thread = new Thread(new ThreadStart(Update));
            thread.Start();
        }

        void Update()
        {
            // temp
            while (true)
            {

                if(_modifications.Count > 0)
                {
                    ApplyModifications(_worldData);
                }
              

                if (_chunksToUpdate.Count > 0)
                    UpdateChunks();

                if (_chunksToUpdate.Count == 0)
                {
                    ResetEvent.WaitOne();
                }

            }
        }
        public object ChunkModificationsThreadLock = new object();
        Queue<Queue<VoxelMod>> _modifications = new Queue<Queue<VoxelMod>>();
        public void EnQueueModification(Queue<VoxelMod> mod)
        {
            lock (ChunkModificationsThreadLock)
            {
                _modifications.Enqueue(mod);
            }

            ResetEvent.Set();
        }

   
        void ApplyModifications(WorldData worldData)
        {
            while (_modifications.Count > 0)
            {
                Queue<VoxelMod> queue;
                lock (ChunkModificationsThreadLock)
                {
                    queue = _modifications.Dequeue();
                }
                while (queue.Count > 0)
                {
                    VoxelMod v = queue.Dequeue();
                    worldData.SetVoxel(v.position, v.id, 1);
                }
            }

        }



        void CheckViewDistance(UnityEngine.Vector3 playerPosition)
        {

            _clouds.UpdateClouds();

            ChunkCoord coord = new ChunkCoord(playerPosition);


            ConcurrentUniqueQueue<ChunkCoord> previouslyActiveChunks = _activeChunks.DeepCopy();

            _activeChunks.Clear();
            
            // Loop through all chunks currently within view distance of the player.
            for (int x = coord.x - World.Instance.settings.viewDistance; x < coord.x + World.Instance.settings.viewDistance; x++)
            {
                for (int z = coord.z - World.Instance.settings.viewDistance; z < coord.z + World.Instance.settings.viewDistance; z++)
                {

                    ChunkCoord thisChunkCoord = new ChunkCoord(x, z);

                    // If the current chunk is in the world...
                    if (IsChunkInWorld(thisChunkCoord))
                    {
                        
                        // Check if it active, if not, activate it.
                        if (_chunks[x, z] == null)
                            _chunks[x, z] = new Chunk(thisChunkCoord);

                        _chunks[x, z].isActive = true;
                        _activeChunks.TryEnqueue(thisChunkCoord);
                    }

                    // Check through previously active chunks to see if this chunk is there. If it is, remove it from the list.

                    previouslyActiveChunks.TryRemove(thisChunkCoord);



                }
            }

            // Any chunks left in the previousActiveChunks list are no longer in the player's view distance, so loop through and disable them.
            previouslyActiveChunks.DoActionOnAllMembers(l => _chunks[l.x, l.z].isActive = false);
        }

        bool IsChunkInWorld(ChunkCoord coord)
        {

            if (coord.x > 0 && coord.x < VoxelData.WorldSizeInChunks - 1 && coord.z > 0 && coord.z < VoxelData.WorldSizeInChunks - 1)
                return true;
            else
                return
                    false;

        }

        public object ChunkUpdateThreadLock = new object();
        private List<Chunk> _chunksToUpdate = new List<Chunk>();
        public void AddChunkToUpdate(Chunk chunk)
        {

            AddChunkToUpdate(chunk, false);

        }
        public void AddChunkToUpdate(Chunk chunk, bool insert)
        {

            // Lock list to ensure only one thing is using the list at a time.
            lock (ChunkUpdateThreadLock)
            {

                // Make sure update list doesn't already contain chunk.
                if (!_chunksToUpdate.Contains(chunk))
                {
                    // If insert is true, chunk gets inserted at the top of the list.
                    if (insert)
                        _chunksToUpdate.Insert(0, chunk);
                    else
                        _chunksToUpdate.Add(chunk);

                }
                ResetEvent.Set();
            }
        }
        void UpdateChunks()
        {
            lock (ChunkUpdateThreadLock)
            {

                _chunksToUpdate[0].UpdateChunk();

                _activeChunks.TryEnqueue(_chunksToUpdate[0].coord);
                _chunksToUpdate.RemoveAt(0);
                ResetEvent.Set();
            }
        }

    }

}
