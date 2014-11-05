using Akka.Actor;

namespace AsyncOrigoSpike
{
    public class Dispatcher : ReceiveActor
    {
        readonly ActorRef _journalWriter;
        readonly ActorRef _executor;
        
        public Dispatcher(ActorRef journalWriter, ActorRef executor)
        {
            _executor = executor;
            _journalWriter = journalWriter;
            Receive<Command>(command => _journalWriter.Tell(new RequestContext(command, Sender)));
            Receive<Query>(query => _executor.Tell(new RequestContext(query, Sender)));
        }
    }
}