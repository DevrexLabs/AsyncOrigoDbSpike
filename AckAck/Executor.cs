using Akka.Actor;

namespace AckAck
{
    public class Executor : ReceiveActor
    {
        readonly Kernel _kernel;
        public Executor(Kernel kernel)
        {
            _kernel = kernel;
            Receive<CommandContext[]>(c => ExecuteCommands(c));
        }

        private void ExecuteCommands(CommandContext[] commandContexts)
        {
            foreach (var context in commandContexts)
            {
                var result = _kernel.Execute(context.Command);

                //send a return message to the external caller
                // will correlate with the call to Ask<>() in Prevayler.ExecuteAsync()
                context.Initiator.Tell(result, Context.Parent);
            }
        }
    }
}