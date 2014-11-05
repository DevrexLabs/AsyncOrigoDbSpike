using Akka.Actor;

namespace AsyncOrigoSpike
{
    public sealed class CommandContext
    {
        public readonly Command Command;
        public readonly ActorRef Initiator;

        public CommandContext(Command command, ActorRef actorRef)
        {
            Command = command;
            Initiator = actorRef;
        }
    }
}