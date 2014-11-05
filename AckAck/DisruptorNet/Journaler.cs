using System.Linq;
using Disruptor;

namespace AsyncOrigoSpike
{
    public class Journaler : IEventHandler<Request>
    {
        public const int DefaultBufferSize = 2048;

        readonly Request[] _buffer;

        //number of items in the buffer
        int _bufferedRequests;

        readonly IJournalWriter _journal;

        public Journaler(IJournalWriter journal, int bufferSize = DefaultBufferSize)
        {
            _journal = journal;
            _buffer = new Request[bufferSize];
        }

        public void OnNext(Request data, long sequence, bool endOfBatch)
        {
            if (_bufferedRequests == _buffer.Length) Flush();
            _buffer[_bufferedRequests++] = data;
            if (endOfBatch) Flush();
        }

        /// <summary>
        /// Send the contents of the buffer to the
        /// journal and wait for the response
        /// </summary>
        private void Flush()
        {
            var commands = _buffer
                .Take(_bufferedRequests)
                .Select(e => e.Transaction as Command);

            _journal.AppendAsync(commands).Wait();
            _bufferedRequests = 0;
        }
    }
}