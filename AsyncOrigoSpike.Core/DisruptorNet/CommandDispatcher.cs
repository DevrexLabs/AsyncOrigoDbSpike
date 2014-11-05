using Disruptor;
using Disruptor.Dsl;

namespace AsyncOrigoSpike
{

    /// <summary>
    /// pushes journaled commands to the execution ring
    /// </summary>
    public class CommandDispatcher : IEventHandler<Request>
    {

        private readonly Disruptor<Request> _executionBuffer;

        public CommandDispatcher(Disruptor<Request> executionBuffer)
        {
            _executionBuffer = executionBuffer;
        }

        public void OnNext(Request data, long sequence, bool endOfBatch)
        {
            _executionBuffer.PublishEvent((e,i) => 
            { 
                e.Transaction = data.Transaction;
                e.Response = data.Response;
                return e;
            });
        }
    }
}