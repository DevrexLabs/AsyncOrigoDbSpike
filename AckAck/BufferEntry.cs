using Akka.Actor;

namespace AckAck
{
    public class BufferEntry
    {
        public Command Command;
        public long Entry;
    }
}