using System;
using System.Net;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using EventStore.ClientAPI;

namespace AckAck
{
    public class Prevayler<M> : IDisposable
    {

        private TplJournalWriter _journalWriter;
        private Dispatcher _dispatcher;

        public Prevayler(M model, int batchSize = 100)
        {
            // the kernel is an origodb component which 
            // synchronizes reads and writes to the model
            // will be shared by 
            var kernel = new Kernel(model);
            _dispatcher = new Dispatcher(kernel);


            var eventStore = EventStoreConnection.Create(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1113));
            eventStore.ConnectAsync().Wait();
            var journalWriter = new EventStoreJournal(eventStore);
            _journalWriter = new TplJournalWriter(journalWriter, _dispatcher, batchSize);

        }

        public async Task<R> ExecuteAsync<R>(Command<M,R> command)
        {
            var response = new WriteOnceBlock<object>(r => r);
            _journalWriter.Post(new CommandContext(command, response));
            return (R) await response.ReceiveAsync();
        }

        public Task ExecuteAsync(Command<M> command)
        {
            var response = new WriteOnceBlock<object>(b => b);
            _journalWriter.Post(new CommandContext(command, response));
            return response.ReceiveAsync();
        }

        public async Task<R> ExecuteAsync<R>(Query<M, R> query)
        {
            var response = new WriteOnceBlock<object>(r => r);
            _dispatcher.Post(new QueryContext(query, response));
            return (R)await response.ReceiveAsync();
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
        }

    }
}