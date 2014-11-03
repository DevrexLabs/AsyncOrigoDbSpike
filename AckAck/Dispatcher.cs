using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace AckAck
{
public class Dispatcher
{
    private Kernel _kernel;
    readonly BufferBlock<CommandContext[]> _commandQueue;
    readonly BatchBlock<QueryContext> _queryQueue;
    readonly ActionBlock<object> _executor;
    readonly Timer _timer;

    /// <summary>
    /// Maximum latency for queries
    /// </summary>
    public TimeSpan Interval = TimeSpan.FromMilliseconds(1);

    public int MaxConcurrentQueries = 4;

    public Dispatcher(Kernel kernel)
    {
        _kernel = kernel;
        _commandQueue = new BufferBlock<CommandContext[]>();
        _queryQueue = new BatchBlock<QueryContext>(MaxConcurrentQueries);
        _executor = new ActionBlock<object>(t =>
        {
            if (t is QueryContext[])
            {
                var queries = t as QueryContext[];
                Task[] tasks = queries.Select(q => Task.Factory.StartNew(_ => ExecuteQuery(q), null)).ToArray();
                Task.WaitAll(tasks);
            }
            else if (t is CommandContext[])
            {
                var commands = t as CommandContext[];
                foreach (var commandContext in commands)
                {
                    var result = _kernel.Execute(commandContext.Command);
                    commandContext.Response.Post(result);
                }
            }

        });
        _commandQueue.LinkTo(_executor);
        _queryQueue.LinkTo(_executor);
        _timer = new Timer(_ => _queryQueue.TriggerBatch());
        _timer.Change(Interval, Interval);
    }

    void ExecuteQuery(QueryContext context)
    {
        var result = _kernel.Execute(context.Query);
        context.Response.Post(result);
    }

    internal void Post(CommandContext[] commands)
    {
        _commandQueue.Post(commands);
    }

    internal void Post(QueryContext query)
    {
        _queryQueue.Post(query);
    }
}
}