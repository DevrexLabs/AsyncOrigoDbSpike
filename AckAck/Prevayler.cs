using System;
using System.Threading.Tasks;
using Akka.Actor;

namespace AckAck
{
    /// <summary>
    /// Prevalence engine 
    /// </summary>
    /// <typeparam name="M"></typeparam>
    public class Prevayler<M> : IDisposable
    {
        readonly ActorSystem _actorSystem;
        readonly ActorRef _dispatcher;

        public Prevayler(M model, int batchSize = 100)
        {
            // the kernel is an origodb component which 
            // synchronizes reads and writes to the model
            // will be shared by 
            var kernel = new Kernel(model);

            

            //build the chain of actors backwards
            _actorSystem = ActorSystem.Create("prevayler");

            //executor executes commands
            //it could also handle queries but would allow either a single query or command at time.
            //better to add a group of actors that can execute queries concurrently
            var executor = _actorSystem.ActorOf(Props.Create(() => new Executor(kernel)));

            //journaler writes commands to the journal in batches or at specific intervals
            //before passing to the executor
            var journaler = _actorSystem.ActorOf(Props.Create(() => new JournalWriter(executor, batchSize)));

            //dispatcher prepares initial message and passes to journaler
            _dispatcher = _actorSystem.ActorOf(Props.Create(() => new Dispatcher(journaler)));
        }

        public Task<R> ExecuteAsync<R>(Command<M,R> command)
        {
            return _dispatcher.Ask<R>(command);
        }

        public Task ExecuteAsync(Command<M> command)
        {
            return _dispatcher.Ask(command);
        }

        public R Execute<R>(Command<M, R> command)
        {
            return ExecuteAsync(command).Result;
        }

        public void Execute(Command<M> command)
        {
            ExecuteAsync(command).Wait();
        }

        public void Dispose()
        {
            _actorSystem.Shutdown();
            _actorSystem.WaitForShutdown();
        }

    }
}