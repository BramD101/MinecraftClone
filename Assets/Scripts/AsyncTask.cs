using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    internal class AsyncTask
    {
        private object _sender;
        private EventArgs _eventArgs;
        private Action<object,EventArgs> _callback;

        public AsyncTask(object sender, EventArgs eventArgs, Action<object, EventArgs> callback)
        {
            _sender = sender;
            _eventArgs = eventArgs;
            _callback = callback;
        }
        public void Invoke()
        {
            _callback(_sender, _eventArgs);
        }
    }
}
