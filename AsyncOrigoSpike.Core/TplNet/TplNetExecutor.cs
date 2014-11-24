using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace AsyncOrigoSpike.TplNet
{
    public class TplNetExecutor
    {
        BlockingCollection<Request> _requestQueue = new BlockingCollection<Request>();
        readonly Kernel _kernel;

        public TplNetExecutor(Kernel kernel)
        {
            _kernel = kernel;
        }

        public void Push(Request request)
        {
            _requestQueue.Add(request);
        }

        public void Start()
        {
            Task.Factory.StartNew(QueueConsumer);
        }

        private void QueueConsumer()
        {
            while (true)
            {
                if (_requestQueue.IsCompleted) break;
                var request = _requestQueue.Take();
                try
                {
                    Object result;
                    if (request.Transaction is Command)
                    {
                        result = _kernel.Execute((Command) request.Transaction);
                    }
                    else
                    {
                        result = _kernel.Execute((Query)request.Transaction);
                    }
                    request.Response.SetResult(result);
                }
                catch (Exception e)
                {
                    request.Response.SetException(e);
                }
            }
        }

        public void Stop()
        {
            _requestQueue.CompleteAdding();
        }
    }
}