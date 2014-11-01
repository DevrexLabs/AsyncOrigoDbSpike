using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Akka.Actor;

namespace AckAck
{
    
    public class TplJournalWriter
    {
        private ActionBlock<CommandContext[]> _writer;
        private IJournalWriter _journalWriter;
        private readonly Timer _timer;
        private readonly BatchBlock<CommandContext> _requestQueue;
        private readonly Dispatcher _dispatcher;

        public TimeSpan Interval { get; set; }
        public TplJournalWriter(IJournalWriter journalWriter, Dispatcher dispatcher, int batchSize = 100)
        {
            Interval = TimeSpan.FromMilliseconds(1);
            _journalWriter = journalWriter;
            _dispatcher = dispatcher;

            _writer = new ActionBlock<CommandContext[]>(batch => Go(batch));

            _requestQueue = new BatchBlock<CommandContext>(batchSize);
            _timer = new Timer(_ => _requestQueue.TriggerBatch());
            _timer.Change(Interval, Interval);
            _requestQueue.LinkTo(_writer);
            
        }

        public void Post(CommandContext request)
        {
            _requestQueue.Post(request);
        }

        private void Go(CommandContext[] batch)
        {
            _journalWriter.AppendAsync(batch.Select(ctx => ctx.Command))
                .ContinueWith(t => _dispatcher.Post(batch));
        }
    }
    /// <summary>
    /// Append multiple commands accumulated during a specific time period or up 
    /// to a specific limit. 
    /// </summary>
    public class JournalWriter
    {

        readonly private IJournalWriter _journalWriter;

        //number of commands at a time to journal
        public int BatchSize = 100;

        //or after a specific time elapsed, whichever comes first
        public TimeSpan Interval = TimeSpan.FromMilliseconds(10);

        //buffered commands waiting to be written to the journal
        private readonly List<CommandContext> _commandBuffer = new List<CommandContext>(200000);

        private readonly Queue<CommandContext[]> _waitingForJournalAck = new Queue<CommandContext[]>(200000);

        

        public JournalWriter(ActorRef executor, int batchSize, IJournalWriter journalWriter)
        {
            _journalWriter = journalWriter;
            BatchSize = batchSize;
        }

        private void Go()
        {
            //if (_commandBuffer.Count > 0)
            //{
            //    var self = Self;
            //    var batch = _commandBuffer.ToArray();
            //    var task = _journalWriter.AppendAsync(batch.Select(item => item.Command));
            //    _commandBuffer.Clear();
            //    _waitingForJournalAck.Enqueue(batch);
            //    task.ContinueWith(t => self.Tell(JournalAcknowledgement.Instance));
            //}
        }

        private void Accept(CommandContext command)
        {
            _commandBuffer.Add(command);
            if (_commandBuffer.Count == BatchSize) Go();
        }
    }
}
