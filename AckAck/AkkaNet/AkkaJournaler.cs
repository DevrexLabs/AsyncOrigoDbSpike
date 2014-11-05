using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;

namespace AsyncOrigoSpike
{
    /// <summary>
    /// Append multiple commands accumulated during a specific time period or up 
    /// to a specific limit. 
    /// </summary>
    public class AkkaJournaler : ReceiveActor
    {

        readonly private IJournalWriter _journalWriter;

        //number of commands at a time to journal
        public int BatchSize = 100;

        //or after a specific time elapsed, whichever comes first
        public TimeSpan Interval = TimeSpan.FromMilliseconds(10);

        //buffered commands waiting to be written to the journal
        private readonly List<CommandContext> _commandBuffer = new List<CommandContext>(200000);

        private readonly Queue<CommandContext[]> _waitingForJournalAck = new Queue<CommandContext[]>(200000);

        //pass on the journaled commands to this actor
        readonly ActorRef _executor; 
        

        public AkkaJournaler(ActorRef executor, int batchSize, IJournalWriter journalWriter)
        {
            _journalWriter = journalWriter;
            BatchSize = batchSize;
            _executor = executor;
            Receive<CommandContext>(t => Accept(t));
            Receive<ReceiveTimeout>(_ => Go());
            Receive<JournalAcknowledgement>(_ => _executor.Tell(_waitingForJournalAck.Dequeue()));

            SetReceiveTimeout(Interval);
        }

        private void Go()
        {
            if (_commandBuffer.Count > 0)
            {
                var self = Self;
                var batch = _commandBuffer.ToArray();
                var task = _journalWriter.AppendAsync(batch.Select(item => item.Command));
                _commandBuffer.Clear();
                _waitingForJournalAck.Enqueue(batch);
                task.ContinueWith(t => self.Tell(JournalAcknowledgement.Instance));
            }
        }

        private void Accept(CommandContext command)
        {
            _commandBuffer.Add(command);
            if (_commandBuffer.Count == BatchSize) Go();
        }
    }
}
