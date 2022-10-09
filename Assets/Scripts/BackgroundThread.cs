using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Assertions;

namespace Assets.Scripts
{
    internal class BackgroundThread
    {
        private WorldData _worldData;
        public WorldData WorldData { set { _worldData = value; } }  

        private ConcurrentUniqueQueue<ChunkCoord> _activeChunks;

        private Thread _thread;

        private ConcurrentQueue<AsyncTask> _eventBacklog;
      
        public static readonly AutoResetEvent ResetEvent = new AutoResetEvent(true);

        public BackgroundThread(ConcurrentUniqueQueue<ChunkCoord> activeChunks)
        {
            _eventBacklog = new ConcurrentQueue<AsyncTask>();
            _activeChunks = activeChunks;
        }

      
        public void Start()
        {
            Assert.IsNotNull(_worldData);
            Assert.IsNotNull(_activeChunks);

            _thread = new Thread(new ThreadStart(Update));
            _thread.Start();
        }

        internal void Abort()
        {
            _thread?.Abort();
        }
        void Update()
        {
            // temp
            while (true)
            {              

                if (_chunksToUpdate.Count > 0)
                    UpdateChunks();
                             

                AsyncTask result = null;
                if(_eventBacklog.TryDequeue(out result))
                {
                    result.Invoke();
                }

                if (_chunksToUpdate.Count == 0 && _eventBacklog.Count == 0)
                {
                    ResetEvent.WaitOne();
                }
            }
        }
        public void EnQueueModification(Queue<VoxelMod> mod)
        {

            _eventBacklog.Enqueue(new AsyncTask(_worldData,new ApplyModificationsEventArgs(mod), OnApplyModification));
           
            ResetEvent.Set();
        }

        private void OnApplyModification(object objWorldData, EventArgs eventArgs)
        {
            var args = (ApplyModificationsEventArgs)eventArgs;
            var worldData = (WorldData)objWorldData;

            var queue = args.VoxelMods;
            while (queue.Count > 0)
            {
                VoxelMod v = queue.Dequeue();
                worldData.SetVoxel(v.position, v.id, 1);
            }
        }

        public class ApplyModificationsEventArgs : EventArgs
        {
            public ApplyModificationsEventArgs(Queue<VoxelMod> voxelMods)
            {
                this.VoxelMods = voxelMods;
            }

            public Queue<VoxelMod> VoxelMods { get; set; }
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
