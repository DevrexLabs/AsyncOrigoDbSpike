using System;
using System.Threading.Tasks;

namespace AsyncOrigoSpike
{
    /// <summary>
    /// Core transaction handling capabilities of the OrigoDB engine
    /// </summary>
    /// <typeparam name="M"></typeparam>
    public interface IEngine<M> : IDisposable
    {
        Task<R> ExecuteAsync<R>(Command<M, R> command);
        Task ExecuteAsync(Command<M> command);
        Task<R> ExecuteAsync<R>(Query<M, R> query);

        R Execute<R>(Command<M, R> command);
        void Execute(Command<M> command);
        R Execute<R>(Query<M, R> query);
    }
}