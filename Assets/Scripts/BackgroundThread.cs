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

        private ConcurrentUniqueQueue<ChunkCoord> _activeChunks;

        private Thread _thread;

        private ConcurrentQueue<AsyncTask> _eventBacklog = new();

        public static readonly AutoResetEvent ResetEvent = new AutoResetEvent(true);


        private BackgroundThread() { }


        private static BackgroundThread _instance;
        public static BackgroundThread Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BackgroundThread();
                }
                return _instance;
            }
        }

        public void Setup(WorldData worldData, ConcurrentUniqueQueue<ChunkCoord> activeChunks)
        {
            _worldData = worldData;
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

                AsyncTask result = null;
                if (_eventBacklog.TryDequeue(out result))
                {
                    result.Invoke();
                }

                if (_eventBacklog.Count == 0)
                {
                    ResetEvent.WaitOne();
                }
            }
        }
        public void EnQueueModification(Queue<VoxelMod> mod)
        {

            _eventBacklog.Enqueue(new AsyncTask(_worldData, new ApplyModificationsEventArgs(mod), OnApplyModification));

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
        public class UpdateChunkEventArgs : EventArgs
        {
            public UpdateChunkEventArgs()
            {
            }

        }


        private object _isWaitingForRender = new();
        public void EnQueueuChunkToUpdate(Chunk chunk)
        {
            lock (_isWaitingForRender)
            {
                if (!chunk.IsWaitingForRender)
                {
                    _eventBacklog.Enqueue(new AsyncTask(chunk, null, OnUpdateChunk));
                    chunk.IsWaitingForRender = true;
                }
            }
            ResetEvent.Set();
        }
        public void OnUpdateChunk(object sender, EventArgs args)
        {
            var chunk = sender as Chunk;
            _activeChunks.TryEnqueue(chunk.coord);
            lock (_isWaitingForRender)
            {
                chunk.IsWaitingForRender = false; ;
            }
            chunk.UpdateChunk();
        }


    }


}
