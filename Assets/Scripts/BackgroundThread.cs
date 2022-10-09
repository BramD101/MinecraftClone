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
