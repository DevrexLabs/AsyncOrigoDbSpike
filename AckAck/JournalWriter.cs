using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Akka.Actor;
using EventStore.ClientAPI;
namespace AckAck
{

    
    /// <summary>
    /// Append multiple commands accumulated during a specific time period or up 
    /// to a specific limit. 
    /// </summary>
    public class JournalWriter : ReceiveActor
    {

        private readonly IEventStoreConnection _eventStore;
        private readonly IFormatter _formatter;

        //number of commands at a time to journal
        public int BatchSize = 100;

        //or after a specific time elapsed, whichever comes first
        public TimeSpan Interval;

        //buffered commands waiting to be written to the journal
        readonly List<Tuple<Command,ActorRef>> _commandBuffer = new List<Tuple<Command,ActorRef>>();

        //pass on the journaled commands to this actor
        readonly ActorRef _executor; 
        

        public JournalWriter(ActorRef executor, int batchSize)
        {
            BatchSize = batchSize;
            _executor = executor;
            Receive<Tuple<Command, ActorRef>>(t => Accept(t));
            Receive<ReceiveTimeout>(_ => Go());

            SetReceiveTimeout(Interval);
            

            _eventStore = EventStoreConnection.Create(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1113));
            _eventStore.ConnectAsync().Wait();
            _formatter = new BinaryFormatter();

        }

        public void HandleTimeout(ReceiveTimeout _)
        {
            Go();
        }

        private void Go()
        {
            if (_commandBuffer.Count > 0)
            {
                //Console.WriteLine("JOURNALER: Writing {0} commands", _commandBuffer.Count);
                
                _eventStore.AppendToStreamAsync("akka", ExpectedVersion.Any,
                    _commandBuffer.Select(ToEventData).ToArray()).Wait();

                //pass on for execution
                _executor.Tell(_commandBuffer.ToArray());

                _commandBuffer.Clear();
            }
        }

        byte[] _bytes = new byte[200];
        private EventData ToEventData(Tuple<Command, ActorRef> arg)
        {
            
            var id = Guid.NewGuid();
            //var stream = new MemoryStream();
            //_formatter.Serialize(stream, arg.Item1);
            return new EventData(id, "akka", false, _bytes, null);
            
        }

        public void Accept(Tuple<Command, ActorRef> command)
        {
            _commandBuffer.Add(command);
            if (_commandBuffer.Count == BatchSize) Go();
        }

        protected override void PostStop()
        {
            _eventStore.Close();
        }
    }
}
