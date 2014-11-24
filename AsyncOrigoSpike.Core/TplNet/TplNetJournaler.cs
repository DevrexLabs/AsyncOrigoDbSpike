using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncOrigoSpike.TplNet
{
    public class TplNetJournaler
    {
        readonly BlockingCollection<Request> _commandQueue = new BlockingCollection<Request>();
        readonly IJournalWriter _journalWriter;
        readonly int _batchSize;
        readonly TplNetExecutor _executor;


        public void Push(Request request)
        {
            _commandQueue.Add(request);
        }

        public void Start()
        {
            Task.Factory.StartNew(QueueConsumer);
        }

        public TplNetJournaler(int batchSize, IJournalWriter journalWriter, TplNetExecutor executor)
        {
            _batchSize = batchSize;
            _journalWriter = journalWriter;
            _executor = executor;
        }

        private void QueueConsumer()
        {
            var buf = new List<Request>(10000);
            while (true)
            {
                Request request;
                if (_commandQueue.IsCompleted) break;
                //wait for a first item
                if (!_commandQueue.TryTake(out request, TimeSpan.FromSeconds(1))) continue;
                buf.Add(request);

                //take the rest but don't wait
                while (buf.Count < _batchSize && _commandQueue.TryTake(out request))
                {
                    buf.Add(request);
                }

                //at this point we have at least one request to process
                var requests = buf.ToArray();
                buf.Clear();
                var commands = requests.Select(r => (Command) r.Transaction);

                _journalWriter.AppendAsync(commands).ContinueWith(t =>
                {
                    foreach (var r in requests) _executor.Push(r);
                });
                
            }
        }

        public void Stop()
        {
            _commandQueue.CompleteAdding();
        }
    }
}