using Akka.Actor;

namespace AsyncOrigoSpike
{
    public sealed class RequestContext
    {
        public readonly object Transaction;
        public readonly ActorRef Initiator;

        public RequestContext(object transaction, ActorRef actorRef)
        {
            Transaction = transaction;
            Initiator = actorRef;
        }
    }
}