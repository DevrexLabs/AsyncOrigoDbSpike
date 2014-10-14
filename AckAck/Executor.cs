using System;
using Akka.Actor;

namespace AckAck
{
    public class Executor : ReceiveActor
    {
        readonly Kernel _kernel;
        public Executor(Kernel kernel)
        {
            _kernel = kernel;
            Receive<Tuple<Command,ActorRef>[]>(ExecuteCommands);
        }

        private bool ExecuteCommands(Tuple<Command,ActorRef>[] tuples)
        {
            foreach (var tuple in tuples)
            {
                var result = _kernel.Execute(tuple.Item1);

                //send a return message to the external caller
                // will correlate with the call to Ask<>() in Prevayler.ExecuteAsync()
                tuple.Item2.Tell(result, Context.Parent);
            }
            return true;
        }
    }
}