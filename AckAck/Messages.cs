using System;
using System.Net.Configuration;
using System.Runtime.Remoting.Messaging;

namespace AckAck
{
    [Serializable]
    public abstract class Command<M, R> : Command
    {
        public abstract R Execute(M model);

        public override object ExecuteImpl(object model)
        {
            return Execute((M) model);
        }
    }

    [Serializable]
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

    [Serializable]
    public abstract class Query<M, R> : Query
    {
        public abstract R Execute(M model);
        public override object ExecuteImpl(object model)
        {
            return Execute((M)model);
        }
    }

    [Serializable]
    public abstract class Query
    {
        public abstract object ExecuteImpl(object model);
    }

    [Serializable]
    public abstract class Command
    {
        public abstract object ExecuteImpl(object model);
    }
}
