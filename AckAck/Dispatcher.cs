using System;
using Akka.Actor;

namespace AckAck
{
    public class Dispatcher : ReceiveActor
    {
        readonly ActorRef _journalWriter;
        
        public Dispatcher(ActorRef journalWriter)
        {
            _journalWriter = journalWriter;
            Receive<Command>(EnqueueCommand);
            Receive<Query>(ExecuteQuery);
        }

        private bool EnqueueCommand(Command command)
        {
            _journalWriter.Tell(Tuple.Create(command, Sender));
            return true;
        }

        private bool ExecuteQuery(Query query)
        {
            _journalWriter.Tell(Tuple.Create(query, Sender));
            return true;
        }

    }
}