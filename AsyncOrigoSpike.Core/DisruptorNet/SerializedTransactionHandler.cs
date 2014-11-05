using Disruptor;

namespace AsyncOrigoSpike
{
    /// <summary>
    /// Executes transactions one at a time
    /// </summary>
    public class SerializedTransactionHandler : IEventHandler<Request>
    {
        readonly Kernel _kernel;

        public SerializedTransactionHandler(Kernel kernel)
        {
            _kernel = kernel;
        }
        public void OnNext(Request data, long sequence, bool endOfBatch)
        {
            object result = null;
            if (data.Transaction is Command)
            {
                result = _kernel.Execute(data.Transaction as Command);
            }
            else
            {
                result = _kernel.Execute(data.Transaction as Query);
            }
            data.Response.SetResult(result);
        }
    }
}