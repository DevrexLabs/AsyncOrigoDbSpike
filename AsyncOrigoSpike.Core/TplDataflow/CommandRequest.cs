
using System.Threading.Tasks.Dataflow;

namespace AsyncOrigoSpike
{
    public sealed class CommandRequest
    {
        public readonly Command Command;
        public readonly WriteOnceBlock<object> Response;

        public CommandRequest(Command command, WriteOnceBlock<object> response)
        {
            Command = command;
            Response = response;
        }
    }
}