using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AckAck
{
    public interface IJournalWriter
    {
        Task AppendAsync(IEnumerable<Command> commands);
    }

    public class NullJournalWriter : IJournalWriter
    {
        public Task AppendAsync(IEnumerable<Command> commands)
        {
            return Task.FromResult(0);
        }
    }
}