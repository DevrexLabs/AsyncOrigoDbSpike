
using System.Threading.Tasks.Dataflow;

namespace AckAck
{
    public sealed class CommandContext
    {
        public readonly Command Command;
        public readonly WriteOnceBlock<object> Response;

        public CommandContext(Command command, WriteOnceBlock<object> response)
        {
            Command = command;
            Response = response;
        }
    }
}