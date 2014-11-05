using System;
using System.Threading.Tasks;
using Akka.Actor;

namespace AsyncOrigoSpike
{
    /// <summary>
    /// Prevalence engine 
    /// </summary>
    public class AkkaEngine<M> : IEngine<M>
    {
        readonly ActorSystem _actorSystem;
        readonly ActorRef _dispatcher;

        public AkkaEngine(M model, int batchSize, IJournalWriter journalWriter)
        {
            // the kernel is an origodb component which 
            // synchronizes reads and writes to the model
            // will be shared by 
            var kernel = new Kernel(model);


            //var journalWriter = new NullJournalWriter();
            //build the chain of actors backwards
            _actorSystem = ActorSystem.Create("prevayler");

            //executor executes commands
            //it could also handle queries but would allow either a single query or command at time.
            //better to add a group of actors that can execute queries concurrently
            var executor = _actorSystem.ActorOf(Props.Create(() => new Executor(kernel)));

            //journaler writes commands to the journal in batches or at specific intervals
            //before passing to the executor
            var journaler = _actorSystem.ActorOf(Props.Create(() => new AkkaJournaler(executor, batchSize, journalWriter)));

            //dispatcher prepares initial message and passes to journaler
            _dispatcher = _actorSystem.ActorOf(Props.Create(() => new Dispatcher(journaler, executor)));
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

        public Task<R> ExecuteAsync<R>(Query<M, R> query)
        {
            return _dispatcher.Ask<R>(query);
        }

        public R Execute<R>(Query<M, R> query)
        {
            return ExecuteAsync(query).Result;
        }

        public void Dispose()
        {
            _actorSystem.Shutdown();
            _actorSystem.WaitForShutdown();
        }
    }
}