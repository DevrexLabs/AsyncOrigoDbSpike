using System.Collections.Generic;
using Akka.Actor;

namespace AsyncOrigoSpike
{
    public class Executor : ReceiveActor
    {
        readonly Kernel _kernel;
        public Executor(Kernel kernel)
        {
            _kernel = kernel;
            Receive<RequestContext[]>(commands => ExecuteCommands(commands));
            Receive<RequestContext>(ctx => ExecuteQuery(ctx));
        }

        private void ExecuteQuery(RequestContext queryContext)
        {
            var result = _kernel.Execute((Query) queryContext.Transaction);
            queryContext.Initiator.Tell(result, Context.Parent);
        }

        private void ExecuteCommands(IEnumerable<RequestContext> commandContexts)
        {
            foreach (var context in commandContexts)
            {
                var result = _kernel.Execute((Command)context.Transaction);

                //send a return message to the external caller
                // will correlate with the call to Ask<>() in AkkaEngine.ExecuteAsync()
                context.Initiator.Tell(result, Context.Parent);
            }
        }
    }
}