using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace AsyncOrigoSpike
{
    /// <summary>
    /// Schedules and executes transactions
    ///
    /// </summary>
    public class ExecutionPipeline
    {
        readonly Kernel _kernel;
        readonly BufferBlock<CommandRequest[]> _commandQueue;

        /// <summary>
        /// groups queries into batches for parallel processing
        /// </summary>
        readonly BatchBlock<QueryRequest> _queryQueue;

        /// <summary>
        /// Triggers a batch from the queryqueue at given intervals
        /// </summary>
        readonly Timer _timer;

        /// <summary>
        /// Maximum latency for queries
        /// </summary>
        public TimeSpan Interval = TimeSpan.FromMilliseconds(1);

        public int MaxConcurrentQueries = 4;

        public ExecutionPipeline(Kernel kernel)
        {
            _kernel = kernel;
            _commandQueue = new BufferBlock<CommandRequest[]>();
            _queryQueue = new BatchBlock<QueryRequest>(MaxConcurrentQueries);
            
            var transactionHandler = new ActionBlock<object>(t =>
            {
                if (t is QueryRequest[])
                {
                    var queries = t as QueryRequest[];
                    Task[] tasks = queries.Select(q => Task.Factory.StartNew(_ => ExecuteQuery(q), null)).ToArray();
                    Task.WaitAll(tasks);
                }
                else if (t is CommandRequest[])
                {
                    var commands = t as CommandRequest[];
                    foreach (var commandContext in commands)
                    {
                        var result = _kernel.Execute(commandContext.Command);
                        commandContext.Response.Post(result);
                    }
                }

            });
            _commandQueue.LinkTo(transactionHandler);
            _queryQueue.LinkTo(transactionHandler);
            _timer = new Timer(_ => _queryQueue.TriggerBatch());
            _timer.Change(Interval, Interval);
        }

        void ExecuteQuery(QueryRequest context)
        {
            var result = _kernel.Execute(context.Query);
            context.Response.Post(result);
        }

        //accept a batch of commands to be executed
        internal void Post(CommandRequest[] commands)
        {
            _commandQueue.Post(commands);
        }

        /// <summary>
        /// accept a query to be executed
        /// </summary>
        /// <param name="query"></param>
        internal void Post(QueryRequest query)
        {
            _queryQueue.Post(query);
        }
    }
}