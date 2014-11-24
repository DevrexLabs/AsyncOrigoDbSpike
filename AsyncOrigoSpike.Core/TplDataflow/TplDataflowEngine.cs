using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace AsyncOrigoSpike
{
    /// <summary>
    /// IEngine implementation using the TPL Dataflow library
    /// </summary>
    public class TplDataflowEngine<M> : IEngine<M>
    {

        readonly TplBatchingJournaler _journaler;
        readonly ExecutionPipeline _executionPipeline;

        public TplDataflowEngine(M model, int batchSize, IJournalWriter journalWriter)
        {
            var kernel = new Kernel(model);
            _executionPipeline = new ExecutionPipeline(kernel);
            _journaler = new TplBatchingJournaler(journalWriter, _executionPipeline, batchSize);

        }

        public async Task<R> ExecuteAsync<R>(Command<M,R> command)
        {
            var response = new WriteOnceBlock<object>(r => r);
            _journaler.Post(new CommandRequest(command, response));
            return (R) await response.ReceiveAsync();
        }

        public Task ExecuteAsync(Command<M> command)
        {
            var response = new WriteOnceBlock<object>(b => b);
            _journaler.Post(new CommandRequest(command, response));
            return response.ReceiveAsync();
        }

        public async Task<R> ExecuteAsync<R>(Query<M, R> query)
        {
            var response = new WriteOnceBlock<object>(r => r);
            _executionPipeline.Post(new QueryRequest(query, response));
            return (R)await response.ReceiveAsync();
        }

        public R Execute<R>(Command<M, R> command)
        {
            return ExecuteAsync(command).Result;
        }

        public R Execute<R>(Query<M, R> query)
        {
            return ExecuteAsync(query).Result;
        }

        public void Execute(Command<M> command)
        {
            ExecuteAsync(command).Wait();
        }

        public void Dispose()
        {
            _journaler.Dispose();
        }

    }
}