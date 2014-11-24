using System;
using System.Threading.Tasks;

namespace AsyncOrigoSpike.TplNet
{
    public class TplNetEngine<M> : IEngine<M>
    {
        readonly TplNetExecutor _executor;
        readonly TplNetJournaler _journaler;


        public TplNetEngine(M model, int batchSize, IJournalWriter journalWriter)
        {
            var kernel = new Kernel(model);
            _executor = new TplNetExecutor(kernel);
            _journaler = new TplNetJournaler(batchSize, journalWriter, _executor);
            
            _journaler.Start();
            _executor.Start();
        }

        public void Dispose()
        {
            _journaler.Stop();
            _executor.Stop();

        }

        public async Task<R> ExecuteAsync<R>(Command<M, R> command)
        {
            var request = new Request {Response = new TaskCompletionSource<object>(), Transaction = command};
            _journaler.Push(request);
            return (R) await request.Response.Task;
        }

        public Task ExecuteAsync(Command<M> command)
        {
            var request = new Request { Response = new TaskCompletionSource<object>(), Transaction = command };
            _journaler.Push(request);
            return request.Response.Task;
        }

        public async Task<R> ExecuteAsync<R>(Query<M, R> query)
        {
            var request = new Request { Response = new TaskCompletionSource<object>(), Transaction = query };
            _executor.Push(request);
            return (R) await request.Response.Task;

        }

        public R Execute<R>(Command<M, R> command)
        {
            return ExecuteAsync(command).Result;
        }

        public void Execute(Command<M> command)
        {
            ExecuteAsync(command).Wait();
        }

        public R Execute<R>(Query<M, R> query)
        {
            return ExecuteAsync(query).Result;
        }
    }
}
