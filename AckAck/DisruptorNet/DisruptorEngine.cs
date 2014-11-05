using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;

namespace AsyncOrigoSpike
{
    /// <summary>
    /// Engine implementation using disruptor.net
    /// Uses 2 ringbuffers, one for journaling/preprocessing and
    /// one for command/query execution
    /// </summary>
    public class DisruptorEngine<M> : IEngine<M>
    {
        //batches and writes to the journal
        readonly Disruptor<Request> _commandJournaler;

        //executes commands and queries
        readonly Disruptor<Request> _transactionHandler;

        readonly IJournalWriter _journalWriter;

        //wire up the disruptor ring buffers and handlers
        public DisruptorEngine(M model, IJournalWriter journalWriter, int batchSize)
        {
            _journalWriter = journalWriter;
            var kernel = new Kernel(model);

            _transactionHandler = new Disruptor<Request>(
                () => new Request(),
                new MultiThreadedClaimStrategy(1024 * 64),
                new YieldingWaitStrategy(),
                TaskScheduler.Default);

            _commandJournaler = new Disruptor<Request>(
                () => new Request(),
                new MultiThreadedClaimStrategy(1024 * 64),
                new YieldingWaitStrategy(),
                TaskScheduler.Default);

            _transactionHandler.HandleEventsWith(new SerializedTransactionHandler(kernel));

            _commandJournaler.HandleEventsWith(new Journaler(_journalWriter, batchSize))
                .Then(new CommandDispatcher(_transactionHandler));

            _transactionHandler.Start();
            _commandJournaler.Start();

        }

        public async Task<R> ExecuteAsync<R>(Command<M, R> command)
        {
            var completion = new TaskCompletionSource<object>();

            _commandJournaler.PublishEvent((e, i) =>
            {
                e.Transaction = command;
                e.Response = completion;
                return e;
            });
            return (R)await completion.Task;
        }

        public Task ExecuteAsync(Command<M> command)
        {
            var completion = new TaskCompletionSource<object>();
            _commandJournaler.PublishEvent((e, i) =>
            {
                e.Transaction = command;
                e.Response = completion;
                return e;
            });
            return completion.Task;

        }

        public async Task<R> ExecuteAsync<R>(Query<M, R> query)
        {
            var completion = new TaskCompletionSource<object>();

            _transactionHandler.PublishEvent((e, i) =>
            {
                e.Transaction = query;
                e.Response = completion;
                return e;
            });
            return (R)await completion.Task;
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
            _commandJournaler.Shutdown();
            _transactionHandler.Shutdown();
            _journalWriter.Dispose();
        }
    }
}
