using System.Runtime.Remoting.Messaging;

namespace AckAck
{
    public abstract class Command<M, R> : Command
    {
        public abstract R Execute(M model);

        public override object ExecuteImpl(object model)
        {
            return Execute((M) model);
        }
    }

    public abstract class Command<M> : Command
    {
        public abstract void Execute(M model);
        public override object ExecuteImpl(object model)
        {
            Execute((M) model);
            return null;
        }
    }


    public class Message
    {
        public readonly int Slot;

        public Message(int slot)
        {
            Slot = slot;
        }
    }


    public class Query
    {
        public readonly string Value;

        public Query(string query)
        {
            Value = query;
        }
    }

    public abstract class Command
    {
        public abstract object ExecuteImpl(object model);
    }
}
