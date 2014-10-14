using Akka.Actor;

namespace AckAck
{
    public class BufferEntry
    {
        
        public bool IsCommand { get; set; }
        
        //Command or Query object
        public object Payload { get; set; }

        public object Result { get; set; } //boxing

        public ActorRef Sender { get; set; }
        public ActorRef Self { get; set; }
    
    }
}