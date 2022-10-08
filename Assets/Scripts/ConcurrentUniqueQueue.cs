using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting.ReorderableList.Element_Adder_Menu;
using UnityEngine.Rendering;
using static UnityEngine.ParticleSystem;

namespace Assets.Scripts
{
    public class ConcurrentUniqueQueue<T> where T : class, IEquatable<T>
    {
        private List<T> _queue;
        private object _lock = new object();
        public ConcurrentUniqueQueue()
        {
            _queue = new List<T>();
        }
    
        public ConcurrentUniqueQueue<T> DeepCopy()
        {
            lock (_lock)
            {
                var newQueue = new ConcurrentUniqueQueue<T>();
                foreach(var elem in _queue)
                {
                    newQueue.TryEnqueue(elem);
                }
                return newQueue;
            }
        }
      

        public int Count { get { return _queue.Count; } }

        public bool TryEnqueue(T obj)
        {
            lock (_lock)
            {
                if (_queue.Contains(obj))
                {
                    return false;
                }
                _queue.Add(obj);
            }
            return true;
        }
        public T DeQueue()
        {
            lock (_lock)
            {
                T first = null;
                if (_queue.Count > 0)
                {
                    first = _queue.First<T>();
                    _queue.RemoveAt(0);
                }
                return first;
            }
        }

        public void DoActionOnAllMembers(Action<T> action)
        {
            lock (_lock)
            {
                foreach(var elem in _queue)
                {
                    action(elem);
                }
            }
        }

        public bool TryRemove(T obj)
        {
            lock (_lock)
            {
                if (_queue.Contains(obj))
                {
                    _queue.Remove(obj);
                    return true;
                }
                return false;
            }
        }
        public void Clear()
        {
            lock (_lock)
            {
                _queue.Clear(); 
            }
        }
        public bool Remove(T obj)
        {
            lock (_lock)
            {
                foreach(var elem in _queue)
                {
                    if(elem.Equals(obj))
                    {
                        _queue.Remove(obj);
                        return true;
                    }
                }
                return false;
            }
        }
       

    }
}
