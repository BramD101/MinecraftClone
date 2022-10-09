

using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts
{
    internal class DebugLogger : IDisposable
    {
        private float _startTime;
        private string _caller;

        public DebugLogger([CallerMemberName] string caller = null)
        {
            _startTime = Time.realtimeSinceStartup;
            _caller = caller;
            Debug.Log($"Start {_caller}, current time = {_startTime}s");

        }
        public void Dispose()
        {
            Debug.Log($"End {_caller}, ellapsed time = {Time.realtimeSinceStartup - _startTime}s");

        }
    }
}
