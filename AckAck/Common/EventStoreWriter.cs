using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace AsyncOrigoSpike
{
    /// <summary>
    /// IJournalWriter implementation that writes to an EventStore 3 instance
    /// </summary>
    public class EventStoreWriter : IJournalWriter
    {
        private readonly IEventStoreConnection _eventStore;
        private readonly IFormatter _formatter;

        public EventStoreWriter(IEventStoreConnection connection, IFormatter formatter = null)
        {
            _formatter = formatter ?? new BinaryFormatter();
            _eventStore = connection;
        }

        public Task AppendAsync(IEnumerable<Command> commands)
        {
            return _eventStore.AppendToStreamAsync("origo", ExpectedVersion.Any, ToEventData(commands));
        }

        private EventData ToEventData(IEnumerable<Command> commands)
        {
            var id = Guid.NewGuid();
            var stream = new MemoryStream();
            _formatter.Serialize(stream, new CommandBatch(commands));
            return new EventData(id, "origo-batch", false, stream.ToArray(), null);
        }

        public void Dispose()
        {
            _eventStore.Close();
        }

        /// <summary>
        /// create an instance with an open connection to an event store instance
        /// </summary>
        /// <returns></returns>
        public static IJournalWriter Create()
        {
            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1113);
            var connection = EventStoreConnection.Create(endPoint);
            connection.ConnectAsync().Wait();
            return new EventStoreWriter(connection);
        }
    }
}