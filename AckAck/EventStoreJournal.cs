using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace AckAck
{

    [Serializable]
    public class CommandBatch
    {
        public readonly Command[] Commands;

        public CommandBatch(IEnumerable<Command> commands)
        {
            Commands = commands.ToArray();
        }
    }
    public class EventStoreJournal : IJournalWriter
    {
        private readonly IEventStoreConnection _eventStore;
        private readonly IFormatter _formatter;


        public EventStoreJournal(IEventStoreConnection connection)
        {
            _formatter = new BinaryFormatter();
            _eventStore = connection;
        }

        public Task AppendAsync(IEnumerable<Command> commands)
        {
            return _eventStore.AppendToStreamAsync("akka", ExpectedVersion.Any, ToEventData(commands));
        }

        private EventData ToEventData(IEnumerable<Command> commands)
        {
            var id = Guid.NewGuid();
            var stream = new MemoryStream();
            _formatter.Serialize(stream, new CommandBatch(commands));
            return new EventData(id, "akka", false, stream.ToArray(), null);
        }

        public void Dispose()
        {
            _eventStore.Close();
        }
    }
}