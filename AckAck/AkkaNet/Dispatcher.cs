using Akka.Actor;

namespace AsyncOrigoSpike
{
    public class Dispatcher : ReceiveActor
    {
        readonly ActorRef _journalWriter;
        
        public Dispatcher(ActorRef journalWriter)
        {
            _journalWriter = journalWriter;
            Receive<Command>(command => _journalWriter.Tell(new CommandContext(command, Sender)));
        }
    }
}