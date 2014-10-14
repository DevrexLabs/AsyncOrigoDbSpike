using System;
using System.Collections.Generic;
using System.Threading;
using Akka.Actor;

namespace AckAck
{

    /// <summary>
    /// Append multiple commands accumulated during a specific time period or up 
    /// to a specific limit. 
    /// </summary>
    public class JournalWriter : ReceiveActor
    {

        //number of commands at a time to journal
        public int BatchSize = 10;

        //or after a specific time elapsed, whichever comes first
        public TimeSpan Interval;

        //buffered commands waiting to be written to the journal
        readonly List<Tuple<Command,ActorRef>> _commandBuffer = new List<Tuple<Command,ActorRef>>();

        //pass on the journaled commands to this actor
        readonly ActorRef _executor; 
        

        public JournalWriter(ActorRef executor)
        {
            _executor = executor;
            Receive<Tuple<Command, ActorRef>>(Accept);
            SetReceiveTimeout(Interval);
            Receive<ReceiveTimeout>(HandleTimeout);

        }

        public bool HandleTimeout(ReceiveTimeout _)
        {
            Go();
            return true;
        }

        private void Go()
        {
            if (_commandBuffer.Count > 0)
            {
                Console.WriteLine("JOURNALER: Writing {0} commands", _commandBuffer.Count);
                
                //simulate delay flushing to disk
                Thread.Sleep(TimeSpan.FromMilliseconds(1));

                //pass on for execution
                _executor.Tell(_commandBuffer.ToArray());

                _commandBuffer.Clear();
            }
        }

        public bool Accept(Tuple<Command, ActorRef> command)
        {
            _commandBuffer.Add(command);
            if (_commandBuffer.Count == BatchSize) Go();
            return true;
        }
    }
}
